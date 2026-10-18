using PopcornTracker.Models;
using static Supabase.Postgrest.Constants;

namespace PopcornTracker.Services;

/// <summary>Data access for people and outings, through the Supabase client.</summary>
public sealed class OutingService(SupabaseProvider provider)
{
    public bool IsConfigured => provider.Client is not null;

    private Supabase.Client Db =>
        provider.Client ?? throw new InvalidOperationException("Supabase n'est pas configuré (appsettings.json).");

    // ---- People ----

    public async Task<List<Person>> GetPeopleAsync()
    {
        var response = await Db.From<Person>()
            .Order("sort_order", Ordering.Ascending)
            .Order("created_at", Ordering.Ascending)
            .Get();
        return response.Models;
    }

    public async Task<Person> AddPersonAsync(Person person)
    {
        var response = await Db.From<Person>().Insert(person);
        return response.Model ?? throw NoRowReturned();
    }

    public async Task UpdatePersonAsync(Person person) =>
        await Db.From<Person>().Update(person);

    public async Task DeletePersonAsync(Guid id) =>
        await Db.From<Person>().Filter("id", Operator.Equals, id.ToString()).Delete();

    // ---- Outings ----

    public async Task<List<Outing>> GetOutingsAsync()
    {
        var response = await Db.From<Outing>()
            .Order("date", Ordering.Descending)
            .Order("created_at", Ordering.Descending)
            .Get();
        return response.Models;
    }

    public async Task<Outing?> GetOutingAsync(Guid id)
    {
        var response = await Db.From<Outing>()
            .Filter("id", Operator.Equals, id.ToString())
            .Get();
        return response.Model;
    }

    public async Task<Outing> SaveOutingAsync(Outing outing)
    {
        // An empty Guid means the row does not exist yet
        var response = outing.Id == Guid.Empty
            ? await Db.From<Outing>().Insert(outing)
            : await Db.From<Outing>().Update(outing);
        return response.Model ?? throw NoRowReturned();
    }

    public async Task DeleteOutingAsync(Guid id) =>
        await Db.From<Outing>().Filter("id", Operator.Equals, id.ToString()).Delete();

    private static InvalidOperationException NoRowReturned() =>
        new("Supabase n'a retourné aucune ligne (vérifie les politiques RLS).");
}
