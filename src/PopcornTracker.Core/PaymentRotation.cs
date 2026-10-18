namespace PopcornTracker.Core;

/// <summary>
/// Computes who should pay next. Payments alternate: each item moves
/// to the next person in the list order after whoever paid it last.
/// </summary>
public static class PaymentRotation
{
    public static NextPayers Suggest(IReadOnlyList<Person> people, IReadOnlyList<Outing> outingsNewestFirst)
    {
        if (people.Count == 0)
        {
            return new NextPayers(null, null);
        }

        // Outings where nobody paid an item (e.g. free popcorn) do not move that item's rotation.
        var lastTickets = outingsNewestFirst.FirstOrDefault(o => o.TicketsPaidBy is not null)?.TicketsPaidBy;
        var lastPopcorn = outingsNewestFirst.FirstOrDefault(o => o.PopcornPaidBy is not null)?.PopcornPaidBy;

        // With no history, tickets start on the first person and popcorn on the second.
        return new NextPayers(
            NextAfter(people, lastTickets, fallbackIndex: 0),
            NextAfter(people, lastPopcorn, fallbackIndex: 1));
    }

    private static Guid NextAfter(IReadOnlyList<Person> people, Guid? previous, int fallbackIndex)
    {
        int index = -1;
        for (int i = 0; i < people.Count; i++)
        {
            if (people[i].Id == previous)
            {
                index = i;
                break;
            }
        }

        // Unknown previous payer (never paid, or deleted since): fall back to the default start.
        return index < 0
            ? people[fallbackIndex % people.Count].Id
            : people[(index + 1) % people.Count].Id;
    }
}
