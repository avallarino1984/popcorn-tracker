using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PopcornTracker.Models;

/// <summary>One trip to the movies and who paid for what.</summary>
[Table("outings")]
public sealed class Outing : BaseModel
{
    [PrimaryKey("id", shouldInsert: false)]
    public Guid Id { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("movie_title")]
    public string MovieTitle { get; set; } = string.Empty;

    // Nullable columns are always written (default handling) so they can be cleared on update
    [Column("poster_url")]
    public string? PosterUrl { get; set; }

    [Column("tmdb_id")]
    public int? TmdbId { get; set; }

    [Column("tickets_paid_by")]
    public Guid? TicketsPaidBy { get; set; }

    [Column("popcorn_paid_by")]
    public Guid? PopcornPaidBy { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>A movie selected in the search UI.</summary>
public sealed record MovieChoice(string Title, string? PosterUrl, int? TmdbId);
