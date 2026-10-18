using PopcornTracker.Models;

namespace PopcornTracker.Services;

/// <summary>Suggested payers for the next outing.</summary>
public sealed record NextPayers(Guid? Tickets, Guid? Popcorn);

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

        var lastTickets = outingsNewestFirst.FirstOrDefault(o => o.TicketsPaidBy is not null)?.TicketsPaidBy;
        var lastPopcorn = outingsNewestFirst.FirstOrDefault(o => o.PopcornPaidBy is not null)?.PopcornPaidBy;

        // With no history, start tickets on the first person and popcorn on the second
        return new NextPayers(
            NextAfter(people, lastTickets, fallbackIndex: 0),
            NextAfter(people, lastPopcorn, fallbackIndex: 1));
    }

    private static Guid NextAfter(IReadOnlyList<Person> people, Guid? previous, int fallbackIndex)
    {
        var index = -1;
        for (var i = 0; i < people.Count; i++)
        {
            if (people[i].Id == previous)
            {
                index = i;
                break;
            }
        }

        return index < 0
            ? people[fallbackIndex % people.Count].Id
            : people[(index + 1) % people.Count].Id;
    }
}
