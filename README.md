# 🍿 Popcorn Tracker

Qui a payé les entrées et le popcorn à la dernière sortie au cinéma — Blazor WebAssembly (PWA) + Supabase, hébergé sur GitHub Pages.

## Fonctionnalités

- **Prochaine sortie** : suggère automatiquement qui paie les entrées et le popcorn (rotation selon l'ordre des personnes).
- **Nouvelle sortie** : date, film (recherche TMDB avec affiche, ou « à l'affiche au Canada »), qui a payé quoi — pré-rempli avec la suggestion.
- **Historique** en cartes « billet de cinéma », modification et suppression.
- **Personnes** configurables : nom, couleur, ordre de rotation.
- Installable sur le téléphone (PWA), lien vers l'horaire du Clap Loretteville.

## Mise en place

### 1. Supabase
1. Crée un projet sur supabase.com.
2. *SQL Editor* → colle et exécute `supabase/schema.sql`.
3. *Project Settings → API* : note l'**URL** et la clé **anon / publishable**.

### 2. TMDB (affiches)
Crée un compte sur themoviedb.org → *Paramètres → API* → demande une clé **v3** (gratuite).

### 3. GitHub
1. Crée le repo `popcorn-tracker` et pousse ce dossier sur `main`.
2. *Settings → Secrets and variables → Actions* : ajoute `SUPABASE_URL`, `SUPABASE_ANON_KEY`, `TMDB_API_KEY`.
3. *Settings → Pages → Source* : **GitHub Actions**.
4. Le workflow publie sur `https://<ton-user>.github.io/popcorn-tracker/`.

## Dev local

Requiert le SDK .NET 10.

```bash
# Put your keys in src/PopcornTracker/wwwroot/appsettings.json (do not commit them)
dotnet watch --project src/PopcornTracker
```

## Notes

- Le Clap n'a pas d'API publique et son site bloque les appels navigateur (CORS) ; la liste « à l'affiche » vient donc de TMDB (`now_playing`, région CA), ce qui couvre ce qui joue au Clap.
- Pas de login : quiconque a l'URL peut modifier les données. Pour verrouiller, on pourra ajouter Supabase Auth (lien magique) et restreindre les politiques RLS à `authenticated`.
