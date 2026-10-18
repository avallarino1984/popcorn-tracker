-- Popcorn Tracker: people and movie outings, shared by every signed-in user.
-- Sign-ups are disabled, so "signed in" means only the accounts created by hand in the dashboard.
-- Run once in Supabase: SQL Editor -> New query -> paste -> Run. Safe to run again.

-- People who take turns paying.
create table if not exists public.people (
    id          uuid primary key default gen_random_uuid(),
    -- Who created the row (kept for history; not used for access).
    user_id     uuid default auth.uid() references auth.users (id) on delete set null,
    name        text not null check (char_length(trim(name)) > 0),
    color       text not null default '#ffc94d',
    sort_order  int  not null default 0,
    created_at  timestamptz not null default now()
);

-- One row per trip to the movies.
create table if not exists public.outings (
    id               uuid primary key default gen_random_uuid(),
    user_id          uuid default auth.uid() references auth.users (id) on delete set null,
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

-- Upgrade from earlier versions (open draft policy, then per-user data).
-- user_id becomes a plain "created by": nullable, and deleting an account must not delete shared data.
alter table public.people  add column if not exists user_id uuid default auth.uid();
alter table public.outings add column if not exists user_id uuid default auth.uid();
alter table public.people  alter column user_id drop not null;
alter table public.outings alter column user_id drop not null;
alter table public.people  drop constraint if exists people_user_id_fkey;
alter table public.outings drop constraint if exists outings_user_id_fkey;
alter table public.people  add constraint people_user_id_fkey
    foreign key (user_id) references auth.users (id) on delete set null;
alter table public.outings add constraint outings_user_id_fkey
    foreign key (user_id) references auth.users (id) on delete set null;

drop policy if exists "anon full access" on public.people;
drop policy if exists "anon full access" on public.outings;
drop policy if exists "people_select_own" on public.people;
drop policy if exists "people_insert_own" on public.people;
drop policy if exists "people_update_own" on public.people;
drop policy if exists "people_delete_own" on public.people;
drop policy if exists "outings_select_own" on public.outings;
drop policy if exists "outings_insert_own" on public.outings;
drop policy if exists "outings_update_own" on public.outings;
drop policy if exists "outings_delete_own" on public.outings;
drop index if exists public.people_user_idx;
drop index if exists public.outings_user_idx;

create index if not exists people_order_idx on public.people  (sort_order, created_at);
create index if not exists outings_date_idx on public.outings (date desc, created_at desc);

-- Row Level Security: anonymous visitors get nothing; every signed-in user shares the same data.
alter table public.people  enable row level security;
alter table public.outings enable row level security;

drop policy if exists "people_shared" on public.people;
create policy "people_shared" on public.people
    for all to authenticated using (true) with check (true);

drop policy if exists "outings_shared" on public.outings;
create policy "outings_shared" on public.outings
    for all to authenticated using (true) with check (true);

revoke all on public.people  from anon;
revoke all on public.outings from anon;
grant select, insert, update, delete on public.people  to authenticated;
grant select, insert, update, delete on public.outings to authenticated;
