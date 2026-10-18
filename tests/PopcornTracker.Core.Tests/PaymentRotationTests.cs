using PopcornTracker.Core;

namespace PopcornTracker.Core.Tests;

public class PaymentRotationTests
{
    private static readonly Person Andres = new() { Id = Guid.NewGuid(), Name = "Andres", SortOrder = 0 };
    private static readonly Person Alexandra = new() { Id = Guid.NewGuid(), Name = "Alexandra", SortOrder = 1 };
    private static readonly Person Guest = new() { Id = Guid.NewGuid(), Name = "Invité", SortOrder = 2 };

    private static Outing Trip(Person? tickets, Person? popcorn) =>
        new() { Id = Guid.NewGuid(), MovieTitle = "Film", TicketsPaidBy = tickets?.Id, PopcornPaidBy = popcorn?.Id };

    [Fact]
    public void NoPeople_SuggestsNobody()
    {
        var next = PaymentRotation.Suggest([], [Trip(Andres, Alexandra)]);

        Assert.Null(next.Tickets);
        Assert.Null(next.Popcorn);
    }

    [Fact]
    public void NoHistory_TicketsOnFirstPerson_PopcornOnSecond()
    {
        var next = PaymentRotation.Suggest([Andres, Alexandra], []);

        Assert.Equal(Andres.Id, next.Tickets);
        Assert.Equal(Alexandra.Id, next.Popcorn);
    }

    [Fact]
    public void SinglePerson_PaysEverything()
    {
        var next = PaymentRotation.Suggest([Andres], [Trip(Andres, Andres)]);

        Assert.Equal(Andres.Id, next.Tickets);
        Assert.Equal(Andres.Id, next.Popcorn);
    }

    [Fact]
    public void TwoPeople_SwapEachItemAfterLastOuting()
    {
        var next = PaymentRotation.Suggest([Andres, Alexandra], [Trip(Andres, Alexandra)]);

        Assert.Equal(Alexandra.Id, next.Tickets);
        Assert.Equal(Andres.Id, next.Popcorn);
    }

    [Fact]
    public void UsesNewestOutingOnly()
    {
        // Newest first: the latest outing had Alexandra on tickets.
        var history = new[] { Trip(Alexandra, Andres), Trip(Andres, Alexandra) };

        var next = PaymentRotation.Suggest([Andres, Alexandra], history);

        Assert.Equal(Andres.Id, next.Tickets);
        Assert.Equal(Alexandra.Id, next.Popcorn);
    }

    [Fact]
    public void ItemNobodyPaid_LooksFurtherBack()
    {
        // Latest outing had no popcorn; the one before had Alexandra on popcorn.
        var history = new[] { Trip(Alexandra, null), Trip(Andres, Alexandra) };

        var next = PaymentRotation.Suggest([Andres, Alexandra], history);

        Assert.Equal(Andres.Id, next.Tickets);
        Assert.Equal(Andres.Id, next.Popcorn);
    }

    [Fact]
    public void ThreePeople_WrapAroundToFirst()
    {
        var next = PaymentRotation.Suggest([Andres, Alexandra, Guest], [Trip(Guest, Alexandra)]);

        Assert.Equal(Andres.Id, next.Tickets);
        Assert.Equal(Guest.Id, next.Popcorn);
    }

    [Fact]
    public void DeletedPreviousPayer_FallsBackToDefaultStart()
    {
        var removed = new Person { Id = Guid.NewGuid(), Name = "Ancien" };

        var next = PaymentRotation.Suggest([Andres, Alexandra], [Trip(removed, removed)]);

        Assert.Equal(Andres.Id, next.Tickets);
        Assert.Equal(Alexandra.Id, next.Popcorn);
    }
}
