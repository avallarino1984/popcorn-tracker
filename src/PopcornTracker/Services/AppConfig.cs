namespace PopcornTracker.Services;

/// <summary>
/// Runtime configuration loaded from wwwroot/appsettings.json.
/// All values here end up in the browser, so only public keys belong here.
/// </summary>
public sealed class AppConfig
{
    public string SupabaseUrl { get; init; } = string.Empty;

    /// <summary>Supabase "anon" (publishable) key. Access is controlled by RLS policies.</summary>
    public string SupabaseAnonKey { get; init; } = string.Empty;

    /// <summary>TMDB v3 API key, used for movie search and posters.</summary>
    public string TmdbApiKey { get; init; } = string.Empty;

    public string CinemaName { get; init; } = string.Empty;

    public string CinemaScheduleUrl { get; init; } = string.Empty;
}
