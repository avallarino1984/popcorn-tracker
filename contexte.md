# Contexte — Popcorn Tracker

> À coller (ou joindre) au début d'une nouvelle conversation avec une IA pour modifier l'app.
> Les détails techniques sont dans `developpement.md`.

## L'app en une phrase

PWA perso pour savoir **qui a payé les entrées et qui a payé le popcorn** à chaque sortie au cinéma (Le Clap Loretteville, Québec). Les paiements alternent ; l'app suggère qui paie la prochaine fois.

- Repo public : `avallarino1984/popcorn-tracker`
- Site : https://avallarino1984.github.io/popcorn-tracker/
- Utilisée surtout sur iPhone (installée sur l'écran d'accueil), en français (fr-CA).

## Stack

- **Blazor WebAssembly standalone, .NET 10**, en PWA, hébergée sur **GitHub Pages**.
- **Supabase** (Postgres + Auth) appelé par un **client REST maison** (`SupabaseClient.cs`), sans le SDK NuGet.
- **TMDB** pour la recherche de films et les affiches (appel direct depuis le navigateur).
- Tests **xUnit** sur le projet Core, exécutés avant chaque déploiement.

Le projet reprend volontairement **la même approche que mon autre app CaloriesTracker** (repo public `avallarino1984/CaloriesTracker`). En cas de doute, faire comme dans CaloriesTracker.

## Conventions (à respecter)

- **Commentaires de code en anglais.** Textes de l'interface en **français**.
- Navigateur en fr-CA (virgule décimale) : tout `<select>` lié à un `double` doit avoir `@bind:culture="CultureInfo.InvariantCulture"`.
- Formats de date : `fr-CA` à l'affichage, `yyyy-MM-dd` en culture invariante vers Supabase.
- La logique pure (calculs, règles) va dans `PopcornTracker.Core` avec des tests ; l'UI et l'accès aux données vont dans `PopcornTracker.Web`.
- Pas de nouvelle dépendance NuGet sans raison forte.
- Mobile d'abord : tester mentalement sur un iPhone (largeur ~390 px, Safari iOS).

## Décisions déjà prises (ne pas remettre en question sans raison)

- **Pas de secrets GitHub.** L'URL Supabase, la clé *publishable* et la clé TMDB sont commitées dans `wwwroot/appsettings.json`. Dans une app WASM, tout finit dans le navigateur ; un secret au build ne cache rien.
- **La sécurité repose sur la RLS Supabase** : le rôle `anon` n'a aucun droit ; tout utilisateur **connecté** voit et modifie **les mêmes données** (historique partagé).
- **Connexion courriel + mot de passe.** Deux comptes (moi et mon voisin, avec qui je vais au cinéma), créés à la main dans Supabase ; **inscriptions désactivées**. C'est ce qui garantit que « connecté » veut dire nous deux seulement. Ne jamais réactiver les inscriptions sans revoir la RLS.
- **`<base href>` calculé à l'exécution** dans `index.html` (sous-chemin `/<repo>/` sur github.io). Les fichiers publiés ne sont jamais modifiés après `dotnet publish`, sinon les hash du service worker cassent.
- Service worker : `skipWaiting` + `clients.claim` + rechargement unique sur `controllerchange`.
- Le site du Clap n'a pas d'API (et bloque le CORS) : la liste « à l'affiche » vient de TMDB `now_playing` région CA, plus un lien vers l'horaire.
- La clé **`service_role`** de Supabase ne doit **jamais** apparaître dans le repo.

## Fonctionnalités actuelles

- **Accueil** : carte « Sortie à ajouter · c'est au tour de » (qui paie les entrées / le popcorn la prochaine fois), puis deux listes en cartes « billet de cinéma » :
  - **Prochaines sorties** : aujourd'hui et futures, la plus proche en haut (en grand), étiquette « Aujourd'hui / Demain / Dans N jours » ;
  - **Historique** : sorties passées, la plus récente en haut. Une sortie planifiée passe dans l'historique le lendemain de sa date.
- **Nouvelle / modifier une sortie** : date (passée, aujourd'hui ou **future** : on peut planifier des sorties à l'avance), film (à l'affiche, recherche TMDB, ou titre manuel), payeurs pré-remplis par la rotation, note. Suppression.
- **Personnes** : ajout, renommage, couleur, ordre (= ordre de rotation), suppression ; bouton de déconnexion.

## Comment livrer une modification

1. Modifier les fichiers dans le repo local (`~/avallarino git/popcorn-tracker`, Mac).
2. `git add . && git commit -m "..." && git push` sur `main`.
3. GitHub Actions : tests → publish → GitHub Pages (2–3 min).
4. Si le schéma de base change : ajouter le SQL dans `supabase/setup.sql` (idempotent) et l'exécuter dans le *SQL Editor* de Supabase.

## Idées en attente

- Déplacer la clé TMDB dans une Supabase Edge Function (clé dans les secrets Supabase).
- Écran « Changer mon mot de passe ».
- Rechargement automatique de l'accueil quand l'app revient au premier plan (comme `onVisible` dans CaloriesTracker).
- Statistiques (nombre de sorties, films par mois).
