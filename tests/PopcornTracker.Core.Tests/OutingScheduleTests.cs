using PopcornTracker.Core;

namespace PopcornTracker.Core.Tests;

public class OutingScheduleTests
{
    private static readonly DateOnly Today = new(2026, 10, 18);

    private static Outing On(int month, int day, string title) =>
        new() { Id = Guid.NewGuid(), Date = new DateOnly(2026, month, day), MovieTitle = title };

    [Fact]
    public void Upcoming_IsNearestFirst()
    {
        var split = OutingSchedule.Split([On(10, 31, "31"), On(10, 22, "22")], Today);

        Assert.Equal(new[] { "22", "31" }, split.Upcoming.Select(o => o.MovieTitle));
        Assert.Empty(split.Past);
    }

    [Fact]
    public void Past_IsMostRecentFirst()
    {
        var split = OutingSchedule.Split([On(10, 8, "8"), On(10, 14, "14")], Today);

        Assert.Equal(new[] { "14", "8" }, split.Past.Select(o => o.MovieTitle));
        Assert.Empty(split.Upcoming);
    }

    [Fact]
    public void Today_IsUpcoming_YesterdayIsPast()
    {
        var split = OutingSchedule.Split([On(10, 18, "today"), On(10, 17, "yesterday")], Today);

        Assert.Equal("today", Assert.Single(split.Upcoming).MovieTitle);
        Assert.Equal("yesterday", Assert.Single(split.Past).MovieTitle);
    }

    [Fact]
    public void SameDay_KeepsCreationOrder()
    {
        var first = On(10, 22, "first");
        first.CreatedAt = new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);
        var second = On(10, 22, "second");
        second.CreatedAt = new DateTimeOffset(2026, 10, 1, 11, 0, 0, TimeSpan.Zero);

        var split = OutingSchedule.Split([second, first], Today);

        Assert.Equal(new[] { "first", "second" }, split.Upcoming.Select(o => o.MovieTitle));
    }

    [Theory]
    [InlineData(18, "Aujourd'hui")]
    [InlineData(19, "Demain")]
    [InlineData(22, "Dans 4 jours")]
    [InlineData(17, "Passée")]
    public void RelativeLabel_DescribesDistance(int day, string expected)
    {
        Assert.Equal(expected, OutingSchedule.RelativeLabel(new DateOnly(2026, 10, day), Today));
    }
}
