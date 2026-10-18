# 🍿 Popcorn Tracker

Qui a payé les entrées et le popcorn à la dernière sortie au cinéma. Blazor WebAssembly (PWA) + Supabase, hébergé sur GitHub Pages.

## Fonctionnalités

- **Prochaine sortie** : suggère qui paie les entrées et le popcorn (rotation selon l'ordre des personnes).
- **Nouvelle sortie** : date, film (recherche TMDB avec affiche, ou « à l'affiche au Canada »), qui a payé quoi, pré-rempli avec la suggestion.
- **Historique** en cartes « billet de cinéma », modification et suppression.
- **Personnes** configurables : nom, couleur, ordre de rotation.
- Installable sur le téléphone (PWA), lien vers l'horaire du Clap Loretteville.

## Structure

```
src/PopcornTracker.Core          Pure logic (models, payment rotation)
src/PopcornTracker.Web           Blazor WebAssembly PWA
tests/PopcornTracker.Core.Tests  xUnit tests, run before every deployment
supabase/setup.sql               Tables + RLS
```

## Mise en place

### 1. Supabase
1. *SQL Editor → New query* : colle `supabase/setup.sql`, puis *Run*.
2. *Authentication → Sign In / Providers* : désactive **Allow new users to sign up**.
3. *Authentication → Users → Add user → Create new user* : courriel + mot de passe, avec **Auto Confirm User** coché.
4. *Project Settings → API* : copie l'**URL** et la clé **publishable / anon** dans `src/PopcornTracker.Web/wwwroot/appsettings.json`.

### 2. TMDB
Sur themoviedb.org → *Paramètres → API*, crée une clé **v3** et colle-la dans `appsettings.json` (`Tmdb:ApiKey`).

### 3. GitHub Pages
*Settings → Pages → Source* : **GitHub Actions**. Chaque push sur `main` lance les tests, puis publie sur `https://<user>.github.io/popcorn-tracker/`.

## Pourquoi les clés sont dans le repo

Dans une app Blazor WASM, tout ce qui est fourni au build finit dans le navigateur, où on peut le lire. Un secret GitHub ne cacherait donc rien. La clé Supabase *publishable* est faite pour être publique : la sécurité repose sur la RLS, qui limite chaque utilisateur connecté à ses propres lignes. La clé TMDB est publique aussi ; le seul risque est qu'on consomme le quota, et on peut la régénérer. Ne mets **jamais** la clé `service_role` / secrète dans ce fichier.

## Dev local

```bash
dotnet test tests/PopcornTracker.Core.Tests
dotnet watch --project src/PopcornTracker.Web
```
