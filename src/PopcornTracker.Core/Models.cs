using System.Text.Json.Serialization;

namespace PopcornTracker.Core;

/// <summary>A participant who can pay for tickets or popcorn (a row of the "people" table).</summary>
public sealed class Person
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = "";

    /// <summary>Hex color used for the avatar and chips.</summary>
    [JsonPropertyName("color")] public string Color { get; set; } = "#ffc94d";

    /// <summary>Order used for the payment rotation.</summary>
    [JsonPropertyName("sort_order")] public int SortOrder { get; set; }

    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }

    [JsonIgnore]
    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[..1].ToUpperInvariant();
}

/// <summary>One trip to the movies and who paid for what (a row of the "outings" table).</summary>
public sealed class Outing
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("date")] public DateOnly Date { get; set; }

    [JsonPropertyName("movie_title")] public string MovieTitle { get; set; } = "";

    [JsonPropertyName("poster_url")] public string? PosterUrl { get; set; }

    [JsonPropertyName("tmdb_id")] public int? TmdbId { get; set; }

    [JsonPropertyName("tickets_paid_by")] public Guid? TicketsPaidBy { get; set; }

    [JsonPropertyName("popcorn_paid_by")] public Guid? PopcornPaidBy { get; set; }

    [JsonPropertyName("notes")] public string? Notes { get; set; }

    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>A movie selected in the search UI.</summary>
public sealed record MovieChoice(string Title, string? PosterUrl, int? TmdbId);

/// <summary>Suggested payers for the next outing.</summary>
public sealed record NextPayers(Guid? Tickets, Guid? Popcorn);
