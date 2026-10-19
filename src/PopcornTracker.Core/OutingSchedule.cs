namespace PopcornTracker.Core;

/// <summary>Outings split around a reference day.</summary>
/// <param name="Upcoming">Today and later, nearest first.</param>
/// <param name="Past">Before today, most recent first.</param>
public sealed record ScheduledOutings(IReadOnlyList<Outing> Upcoming, IReadOnlyList<Outing> Past);

/// <summary>Splits outings into planned (today and later) and history (before today).</summary>
public static class OutingSchedule
{
    public static ScheduledOutings Split(IEnumerable<Outing> outings, DateOnly today)
    {
        var all = outings.ToList();

        // Today counts as upcoming: an outing stays "planned" until the day after.
        var upcoming = all
            .Where(o => o.Date >= today)
            .OrderBy(o => o.Date)
            .ThenBy(o => o.CreatedAt)
            .ToList();

        var past = all
            .Where(o => o.Date < today)
            .OrderByDescending(o => o.Date)
            .ThenByDescending(o => o.CreatedAt)
            .ToList();

        return new ScheduledOutings(upcoming, past);
    }

    /// <summary>Short French label for how far an upcoming outing is ("Aujourd'hui", "Demain", "Dans 4 jours").</summary>
    public static string RelativeLabel(DateOnly date, DateOnly today)
    {
        int days = date.DayNumber - today.DayNumber;
        return days switch
        {
            < 0 => "Passée",
            0 => "Aujourd'hui",
            1 => "Demain",
            _ => $"Dans {days} jours",
        };
    }
}
