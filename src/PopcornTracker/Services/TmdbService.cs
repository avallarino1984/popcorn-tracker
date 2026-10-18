using System.Net.Http.Json;
using System.Text.Json;

namespace PopcornTracker.Services;

/// <summary>
/// Movie search and "now playing" list from The Movie Database (TMDB).
/// TMDB allows CORS, so the browser can call it directly.
/// </summary>
public sealed class TmdbService(AppConfig config)
{
    private const string ApiBase = "https://api.themoviedb.org/3/";
    private const string ImageBase = "https://image.tmdb.org/t/p/";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiBase) };

    // Cache the now-playing list for the session; it rarely changes
    private List<TmdbMovie>? _nowPlaying;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(config.TmdbApiKey);

    public static string? PosterUrl(string? posterPath, string size = "w342") =>
        string.IsNullOrWhiteSpace(posterPath) ? null : $"{ImageBase}{size}{posterPath}";

    public async Task<List<TmdbMovie>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var url = $"search/movie?api_key={config.TmdbApiKey}&language=fr-CA&region=CA&include_adult=false"
                  + $"&query={Uri.EscapeDataString(query.Trim())}";
        return await GetResultsAsync(url, ct);
    }

    /// <summary>
    /// Movies currently in theaters in Canada. Le Clap does not expose a public API,
    /// so this is the closest approximation of what's showing there.
    /// </summary>
    public async Task<List<TmdbMovie>> NowPlayingAsync(CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            return [];
        }

        _nowPlaying ??= await GetResultsAsync(
            $"movie/now_playing?api_key={config.TmdbApiKey}&language=fr-CA&region=CA", ct);
        return _nowPlaying;
    }

    private async Task<List<TmdbMovie>> GetResultsAsync(string url, CancellationToken ct)
    {
        var page = await _http.GetFromJsonAsync<TmdbPage>(url, JsonOptions, ct);
        return page?.Results ?? [];
    }

    private sealed class TmdbPage
    {
        public List<TmdbMovie> Results { get; set; } = [];
    }
}

public sealed class TmdbMovie
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? PosterPath { get; set; }

    public string? ReleaseDate { get; set; }

    public string Year => ReleaseDate is { Length: >= 4 } ? ReleaseDate[..4] : string.Empty;
}
