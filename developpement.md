# Développement — Popcorn Tracker

Détails techniques du projet. Pour le contexte général et les conventions, voir `contexte.md`.

## Structure du repo

```
.github/workflows/deploy.yml        CI : tests → publish → GitHub Pages
PopcornTracker.slnx                 Solution (Core, Web, Tests)
supabase/setup.sql                  Tables + index + RLS (idempotent)
src/PopcornTracker.Core/            Logique pure, sans dépendance web
  Models.cs                           Person, Outing, MovieChoice, NextPayers
  PaymentRotation.cs                  Suggestion des prochains payeurs
src/PopcornTracker.Web/             Blazor WebAssembly (PWA)
  Program.cs                          Lecture de appsettings.json + DI
  Services/SupabaseClient.cs          Client REST Auth + PostgREST (session, refresh)
  Services/OutingService.cs           CRUD people / outings
  Services/TmdbService.cs             Recherche TMDB, now playing, options TMDB/cinéma
  Layout/MainLayout.razor             Démarrage, écran de connexion, navigation
  Pages/Home.razor                    "/" : prochaine sortie + historique
  Pages/OutingEditor.razor            "/sortie/nouvelle", "/sortie/{id:guid}"
  Pages/People.razor                  "/personnes" : personnes + déconnexion
  Components/                         Avatar, LoginCard, MovieSearch, OutingCard, PayerLine, PersonPicker
  wwwroot/appsettings.json            Config publique (Supabase, TMDB, cinéma)
  wwwroot/index.html                  <base href> dynamique, enregistrement du SW
  wwwroot/service-worker*.js          Dev (no-op) / publié (cache hors ligne)
  wwwroot/css/app.css                 Tout le style (thème sombre, mobile first)
tests/PopcornTracker.Core.Tests/    xUnit
```

## Modèle de données (Supabase / Postgres)

### `people`
| Colonne | Type | Notes |
|---|---|---|
| `id` | uuid | PK, `gen_random_uuid()` |
| `user_id` | uuid | créé par : `default auth.uid()`, nullable, FK `auth.users` `on delete set null` |
| `name` | text | non vide |
| `color` | text | hex, défaut `#ffc94d` |
| `sort_order` | int | ordre de rotation |
| `created_at` | timestamptz | `now()` |

### `outings`
| Colonne | Type | Notes |
|---|---|---|
| `id` | uuid | PK |
| `user_id` | uuid | créé par (idem `people`) |
| `date` | date | défaut `current_date` |
| `movie_title` | text | non vide |
| `poster_url` | text | URL TMDB complète (w342), nullable |
| `tmdb_id` | int | nullable |
| `tickets_paid_by` | uuid | FK `people`, `on delete set null` |
| `popcorn_paid_by` | uuid | FK `people`, `on delete set null` |
| `notes` | text | nullable |
| `created_at` | timestamptz | `now()` |

Index : `people (sort_order, created_at)`, `outings (date desc, created_at desc)`.

### RLS (données partagées)
- Une politique par table, `for all to authenticated using (true) with check (true)` : **tout utilisateur connecté voit et modifie les mêmes données**.
- `revoke all ... from anon`, `grant select, insert, update, delete ... to authenticated` : un visiteur non connecté n'a rien.
- Ce modèle n'est sûr que parce que **les inscriptions sont désactivées** : les seuls comptes sont ceux créés à la main (moi + mon voisin). Réactiver les inscriptions exigerait de revoir la RLS (ex. table de membres).
- `user_id` sert seulement à savoir qui a créé une ligne ; l'app ne l'envoie jamais (défaut `auth.uid()`). Supprimer un compte ne supprime pas les données partagées (`on delete set null`).

Erreur typique : `new row violates row-level security policy` → `setup.sql` à jour pas exécuté, ou requête sans jeton utilisateur.

## Authentification

- `POST auth/v1/token?grant_type=password` avec `{ email, password }` → session (access + refresh token, `expires_at`).
- Session stockée dans `localStorage` sous la clé `popcorn-tracker:session`.
- Avant chaque appel autorisé : si le jeton expire dans moins d'une minute, `grant_type=refresh_token`. Si le refresh échoue (400/401) → déconnexion, l'événement `SignedOut` ramène `MainLayout` à l'écran de connexion.
- **Ne pas lancer deux appels Supabase en parallèle** au chargement : les deux pourraient renouveler la session en même temps avec le même refresh token. Les pages chargent `people` puis `outings` séquentiellement.
- En-têtes de chaque requête : `apikey: <publishable>` + `Authorization: Bearer <access_token>`.

Réglages Supabase : inscriptions désactivées ; un compte par personne, créé dans *Authentication → Users* avec *Auto Confirm User*. *URL Configuration* : Site URL `https://avallarino1984.github.io/popcorn-tracker/`, redirects vers ce site et `http://localhost:5270/**` (sans effet pour le mot de passe ; utile seulement pour les liens envoyés par courriel).

## Accès aux données (PostgREST)

`SupabaseClient` expose `SelectAsync<T>`, `InsertAsync<T>`, `UpdateAsync<T>` (`PATCH ?id=eq.{id}`), `DeleteAsync`. Insert/Update utilisent `Prefer: return=representation` et lisent la première ligne du tableau retourné.

`OutingService` envoie des objets anonymes en snake_case (`movie_title`, `tickets_paid_by`…). Les **null sont envoyés volontairement** pour pouvoir effacer un payeur, une affiche ou une note. La lecture utilise les `[JsonPropertyName]` des modèles Core.

Tri : `people` par `sort_order, created_at` ; `outings` par `date desc, created_at desc`.

## Rotation des payeurs (`PaymentRotation.Suggest`)

- Entrées : la personne **suivante** (dans l'ordre `sort_order`) après le dernier payeur des entrées connu ; même chose, indépendamment, pour le popcorn.
- Les sorties où un item n'a pas de payeur sont ignorées pour cet item (on remonte plus loin dans l'historique).
- Sans historique, ou si le dernier payeur a été supprimé : entrées → 1re personne, popcorn → 2e (ou la 1re s'il n'y a qu'une personne).
- Tests : `tests/PopcornTracker.Core.Tests/PaymentRotationTests.cs` (8 cas).

## TMDB

- `search/movie` et `movie/now_playing`, avec `language=fr-CA&region=CA` et `api_key=<v3>` en paramètre.
- Affiches : `https://image.tmdb.org/t/p/{w185|w342}{poster_path}` ; on stocke l'URL w342 dans `poster_url`.
- `now_playing` est gardé en cache pour la session. La recherche attend 350 ms après la dernière frappe (debounce avec `CancellationTokenSource`).
- Sans clé : saisie manuelle du titre, sans affiche.

## PWA et GitHub Pages

- `index.html` écrit `<base href="/<repo>/">` sur `*.github.io`, sinon `/`.
- `OverrideHtmlAssetPlaceholders=true` + `blazor.webassembly#[.{fingerprint}].js` + `<script type="importmap">` (template .NET 10).
- `service-worker.published.js` : met en cache les assets du manifeste (avec `integrity`) ; le scope du SW (`self.registration.scope`) sert de base. Les appels Supabase/TMDB (autre origine) ne sont jamais mis en cache.
- Mise à jour : `skipWaiting` à l'installation, `clients.claim` à l'activation, et `index.html` recharge la page une seule fois sur `controllerchange`.
- Le workflow copie `index.html` en `404.html` (liens profonds) et crée `.nojekyll` (sinon `_framework` est ignoré). **Aucun fichier publié ne doit être modifié** par le workflow.
- `BlazorWebAssemblyLoadAllGlobalizationData=true` pour le formatage fr-CA quelle que soit la langue du navigateur.

## Interface

- Un seul fichier CSS, variables dans `:root` (`--butter` jaune popcorn, `--ticket` rouge, fond `--bg` presque noir). Polices Google : Bricolage Grotesque (titres), DM Sans (texte).
- Navigation du bas flottante (`.bottomnav`) ; le `.shell` réserve la place en `padding-bottom`.
- Pièges iOS déjà corrigés :
  - `input[type=date]` déborde → `appearance: none; min-width: 0; max-width: 100%`.
  - Champs à `font-size: 16px` minimum, sinon iOS zoome au focus.
  - Pas de bouton `sticky` dans le formulaire (il recouvrait les choix) ; boutons désactivés en couleur pleine, pas en opacité.
- Icônes PWA : `icon-192.png`, `icon-512.png`, `icon-maskable-512.png` (générées par script Python/PIL).

## Développement local

```bash
dotnet test tests/PopcornTracker.Core.Tests
dotnet watch --project src/PopcornTracker.Web     # http://localhost:5270
```

En local, le service worker de dev ne met rien en cache. Le même `appsettings.json` (Supabase réel) est utilisé.

## Déploiement

Push sur `main` → `.github/workflows/deploy.yml` :
1. `dotnet test tests/PopcornTracker.Core.Tests` (le déploiement s'arrête si un test échoue)
2. `dotnet publish src/PopcornTracker.Web -c Release -o publish`
3. `404.html` + `.nojekyll`
4. `upload-pages-artifact` → `deploy-pages`

Prérequis GitHub : *Settings → Pages → Source : GitHub Actions*. Aucun secret.

## Mettre à jour le repo à partir d'un zip

```bash
cd ~/"avallarino git"/popcorn-tracker
rm -rf /tmp/pt && unzip -q ~/Downloads/popcorn-tracker.zip -d /tmp/pt
ditto /tmp/pt/popcorn-tracker .
git add . && git commit -m "..." && git push
```

`ditto` copie aussi les fichiers cachés (`.github`, `.gitignore`). Si des fichiers ont été supprimés ou renommés, les retirer d'abord avec `git rm`.
