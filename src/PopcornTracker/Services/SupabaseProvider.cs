namespace PopcornTracker.Services;

/// <summary>
/// Holds the initialized Supabase client, or null when the app has no Supabase settings.
/// Lets the UI show a friendly banner instead of crashing at startup.
/// </summary>
public sealed class SupabaseProvider(Supabase.Client? client)
{
    public Supabase.Client? Client { get; } = client;
}
