using HotelStay.Api;

namespace HotelStay.Tests;

public class NormalisationTests
{
    private static readonly SearchCriteria AnyStay =
        new("London", new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 4), null);

    [Fact]
    public async Task PremierStays_maps_pascalcase_response_to_unified_model()
    {
        var results = await new PremierStaysProvider().SearchAsync(AnyStay);

        var suite = results.Single(r => r.RoomType == RoomType.Suite);
        Assert.Equal("PremierStays", suite.Provider);
        Assert.Equal(420m, suite.NightlyRate);
        Assert.Equal(CancellationPolicy.NonRefundable, suite.CancellationPolicy);
        Assert.Equal(5, suite.StarRating);
        Assert.NotNull(suite.Amenities);
        Assert.True(suite.Available);
    }

    [Fact]
    public async Task BudgetNests_maps_snakecase_response_and_omits_premium_fields()
    {
        var results = await new BudgetNestsProvider().SearchAsync(AnyStay);

        var deluxe = results.Single(r => r.RoomType == RoomType.Deluxe);
        Assert.Equal("BudgetNests", deluxe.Provider);
        Assert.Equal(140m, deluxe.NightlyRate);
        Assert.Equal(CancellationPolicy.NonRefundable, deluxe.CancellationPolicy);
        Assert.Null(deluxe.Amenities);
        Assert.Null(deluxe.StarRating);
        Assert.False(deluxe.Available);
    }
}
