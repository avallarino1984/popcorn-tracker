using PopcornTracker.Core;

namespace PopcornTracker.Web.Services;

/// <summary>
/// Data access for people and outings. Rows are scoped to the signed-in user by RLS;
/// user_id is filled by the database (default auth.uid()), so it is never sent from here.
/// </summary>
public sealed class OutingService(SupabaseClient db)
{
    private const string People = "people";
    private const string Outings = "outings";

    // ---- People ----

    public Task<List<Person>> GetPeopleAsync() =>
        db.SelectAsync<Person>($"{People}?select=*&order=sort_order.asc,created_at.asc");

    public Task<Person> AddPersonAsync(string name, string color, int sortOrder) =>
        db.InsertAsync<Person>(People, new { name, color, sort_order = sortOrder });

    public Task<Person> UpdatePersonAsync(Person person) =>
        db.UpdateAsync<Person>(People, person.Id,
            new { name = person.Name, color = person.Color, sort_order = person.SortOrder });

    public Task DeletePersonAsync(Guid id) => db.DeleteAsync(People, id);

    // ---- Outings ----

    public Task<List<Outing>> GetOutingsAsync() =>
        db.SelectAsync<Outing>($"{Outings}?select=*&order=date.desc,created_at.desc");

    public async Task<Outing?> GetOutingAsync(Guid id) =>
        (await db.SelectAsync<Outing>($"{Outings}?select=*&id=eq.{id}")).FirstOrDefault();

    /// <summary>Creates the outing when its id is empty, otherwise updates it.</summary>
    public Task<Outing> SaveOutingAsync(Outing outing)
    {
        // Nulls are sent on purpose so an update can clear a payer, poster or note.
        var row = new
        {
            date = outing.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            movie_title = outing.MovieTitle,
            poster_url = outing.PosterUrl,
            tmdb_id = outing.TmdbId,
            tickets_paid_by = outing.TicketsPaidBy,
            popcorn_paid_by = outing.PopcornPaidBy,
            notes = outing.Notes,
        };

        return outing.Id == Guid.Empty
            ? db.InsertAsync<Outing>(Outings, row)
            : db.UpdateAsync<Outing>(Outings, outing.Id, row);
    }

    public Task DeleteOutingAsync(Guid id) => db.DeleteAsync(Outings, id);
}
