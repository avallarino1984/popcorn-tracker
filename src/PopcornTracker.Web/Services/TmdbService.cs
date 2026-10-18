using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PopcornTracker.Web.Services;

/// <summary>TMDB settings, read from wwwroot/appsettings.json.</summary>
public sealed class TmdbOptions
{
    /// <summary>
    /// TMDB v3 API key. It is public (shipped to the browser): the only risk is someone
    /// using our quota, and the key can be regenerated on themoviedb.org.
    /// </summary>
    public string ApiKey { get; set; } = "";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}

/// <summary>Movie theater shown on the home screen (name + link to its schedule).</summary>
public sealed class CinemaOptions
{
    public string Name { get; set; } = "";

    public string ScheduleUrl { get; set; } = "";
}

/// <summary>
/// Movie search and "now playing" list from The Movie Database (TMDB).
/// TMDB allows CORS, so the browser calls it directly.
/// </summary>
public sealed class TmdbService(TmdbOptions options)
{
    private const string ApiBase = "https://api.themoviedb.org/3/";
    private const string ImageBase = "https://image.tmdb.org/t/p/";

    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiBase) };

    // The now-playing list rarely changes: cache it for the session.
    private List<TmdbMovie>? _nowPlaying;

    public bool IsConfigured => options.IsConfigured;

    public static string? PosterUrl(string? posterPath, string size = "w342") =>
        string.IsNullOrWhiteSpace(posterPath) ? null : $"{ImageBase}{size}{posterPath}";

    public async Task<List<TmdbMovie>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return await GetResultsAsync(
            $"search/movie?api_key={options.ApiKey}&language=fr-CA&region=CA&include_adult=false"
            + $"&query={Uri.EscapeDataString(query.Trim())}", ct);
    }

    /// <summary>
    /// Movies currently in theaters in Canada. Le Clap has no public API (and its site blocks
    /// browser calls), so this is the closest approximation of what's showing there.
    /// </summary>
    public async Task<List<TmdbMovie>> NowPlayingAsync(CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            return [];
        }

        _nowPlaying ??= await GetResultsAsync(
            $"movie/now_playing?api_key={options.ApiKey}&language=fr-CA&region=CA", ct);
        return _nowPlaying;
    }

    private async Task<List<TmdbMovie>> GetResultsAsync(string url, CancellationToken ct)
    {
        var page = await _http.GetFromJsonAsync<TmdbPage>(url, ct);
        return page?.Results ?? [];
    }

    private sealed class TmdbPage
    {
        [JsonPropertyName("results")] public List<TmdbMovie> Results { get; set; } = [];
    }
}

public sealed class TmdbMovie
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; } = "";

    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }

    [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }

    public string Year => ReleaseDate is { Length: >= 4 } ? ReleaseDate[..4] : "";
}
