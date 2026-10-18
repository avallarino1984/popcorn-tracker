-- Popcorn Tracker: people and movie outings, readable and writable only by their owner.
-- Run once in Supabase: SQL Editor -> New query -> paste -> Run. Safe to run again.

-- People who take turns paying.
create table if not exists public.people (
    id          uuid primary key default gen_random_uuid(),
    user_id     uuid not null default auth.uid() references auth.users (id) on delete cascade,
    name        text not null check (char_length(trim(name)) > 0),
    color       text not null default '#ffc94d',
    sort_order  int  not null default 0,
    created_at  timestamptz not null default now()
);

-- One row per trip to the movies.
create table if not exists public.outings (
    id               uuid primary key default gen_random_uuid(),
    user_id          uuid not null default auth.uid() references auth.users (id) on delete cascade,
    date             date not null default current_date,
    movie_title      text not null check (char_length(trim(movie_title)) > 0),
    poster_url       text,
    tmdb_id          int,
    -- Deleting a person keeps the outing, just without that payer.
    tickets_paid_by  uuid references public.people (id) on delete set null,
    popcorn_paid_by  uuid references public.people (id) on delete set null,
    notes            text,
    created_at       timestamptz not null default now()
);

-- Upgrade from the first draft (tables without owner + open "anon full access" policy).
-- No-op on a fresh install. Assumes those draft tables are still empty.
alter table public.people  add column if not exists user_id uuid default auth.uid() references auth.users (id) on delete cascade;
alter table public.outings add column if not exists user_id uuid default auth.uid() references auth.users (id) on delete cascade;
alter table public.people  alter column user_id set not null;
alter table public.outings alter column user_id set not null;
drop policy if exists "anon full access" on public.people;
drop policy if exists "anon full access" on public.outings;

create index if not exists people_user_idx  on public.people  (user_id, sort_order);
create index if not exists outings_user_idx on public.outings (user_id, date desc, created_at desc);

-- Row Level Security: the public site + public key can only ever reach the signed-in user's own rows.
alter table public.people  enable row level security;
alter table public.outings enable row level security;

drop policy if exists "people_select_own" on public.people;
drop policy if exists "people_insert_own" on public.people;
drop policy if exists "people_update_own" on public.people;
drop policy if exists "people_delete_own" on public.people;

create policy "people_select_own" on public.people
    for select to authenticated using (auth.uid() = user_id);

create policy "people_insert_own" on public.people
    for insert to authenticated with check (auth.uid() = user_id);

create policy "people_update_own" on public.people
    for update to authenticated using (auth.uid() = user_id) with check (auth.uid() = user_id);

create policy "people_delete_own" on public.people
    for delete to authenticated using (auth.uid() = user_id);

drop policy if exists "outings_select_own" on public.outings;
drop policy if exists "outings_insert_own" on public.outings;
drop policy if exists "outings_update_own" on public.outings;
drop policy if exists "outings_delete_own" on public.outings;

create policy "outings_select_own" on public.outings
    for select to authenticated using (auth.uid() = user_id);

-- Payers must be the user's own people, not someone else's ids.
create policy "outings_insert_own" on public.outings
    for insert to authenticated with check (
        auth.uid() = user_id
        and (tickets_paid_by is null or exists (select 1 from public.people p where p.id = tickets_paid_by and p.user_id = auth.uid()))
        and (popcorn_paid_by is null or exists (select 1 from public.people p where p.id = popcorn_paid_by and p.user_id = auth.uid()))
    );

create policy "outings_update_own" on public.outings
    for update to authenticated using (auth.uid() = user_id) with check (
        auth.uid() = user_id
        and (tickets_paid_by is null or exists (select 1 from public.people p where p.id = tickets_paid_by and p.user_id = auth.uid()))
        and (popcorn_paid_by is null or exists (select 1 from public.people p where p.id = popcorn_paid_by and p.user_id = auth.uid()))
    );

create policy "outings_delete_own" on public.outings
    for delete to authenticated using (auth.uid() = user_id);

-- Anonymous visitors get nothing; signed-in users go through the policies above.
revoke all on public.people  from anon;
revoke all on public.outings from anon;
grant select, insert, update, delete on public.people  to authenticated;
grant select, insert, update, delete on public.outings to authenticated;
