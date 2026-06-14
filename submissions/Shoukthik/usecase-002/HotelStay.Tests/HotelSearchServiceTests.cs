using HotelStay.Api;

namespace HotelStay.Tests;

public class HotelSearchServiceTests
{
    private static readonly SearchCriteria AnyStay =
        new("London", new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 4), null);

    [Fact]
    public async Task Excludes_unavailable_rooms()
    {
        var service = new HotelSearchService([new BudgetNestsProvider()]);

        var results = await service.SearchAsync(AnyStay);

        Assert.DoesNotContain(results, r => r.RoomType == RoomType.Deluxe);
        Assert.All(results, r => Assert.True(r.Available));
    }

    [Fact]
    public async Task One_provider_failing_still_returns_the_others()
    {
        var service = new HotelSearchService([new ThrowingProvider(), new PremierStaysProvider()]);

        var results = await service.SearchAsync(AnyStay);

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal("PremierStays", r.Provider));
    }

    private sealed class ThrowingProvider : IHotelProvider
    {
        public string Name => "Broken";

        public Task<IReadOnlyList<HotelResult>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default)
            => throw new InvalidOperationException("provider down");

        public Task<HotelResult?> BookAsync(SearchCriteria criteria, CancellationToken ct = default)
            => throw new InvalidOperationException("provider down");
    }
}
