-- Popcorn Tracker schema. Run once in Supabase > SQL Editor.

create extension if not exists pgcrypto;

-- People who take turns paying
create table if not exists public.people (
    id          uuid primary key default gen_random_uuid(),
    name        text not null check (char_length(trim(name)) > 0),
    color       text not null default '#ffc94d',
    sort_order  int  not null default 0,
    created_at  timestamptz not null default now()
);

-- One row per trip to the movies
create table if not exists public.outings (
    id               uuid primary key default gen_random_uuid(),
    date             date not null default current_date,
    movie_title      text not null check (char_length(trim(movie_title)) > 0),
    poster_url       text,
    tmdb_id          int,
    -- Keep history if a person is deleted
    tickets_paid_by  uuid references public.people(id) on delete set null,
    popcorn_paid_by  uuid references public.people(id) on delete set null,
    notes            text,
    created_at       timestamptz not null default now()
);

create index if not exists outings_date_idx on public.outings (date desc, created_at desc);

-- Row Level Security.
-- The app has no login: the anon key can read and write.
-- Anyone who has the site URL can therefore edit the data (fine for a family app).
alter table public.people  enable row level security;
alter table public.outings enable row level security;

drop policy if exists "anon full access" on public.people;
create policy "anon full access" on public.people
    for all to anon using (true) with check (true);

drop policy if exists "anon full access" on public.outings;
create policy "anon full access" on public.outings
    for all to anon using (true) with check (true);
