using HotelStay.Api;

namespace HotelStay.Tests;

public class BoutiqueCollectionTests
{
    private static readonly SearchCriteria AnyStay =
        new("Paris", new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 4), null);

    [Fact]
    public async Task Adds_the_flat_boutique_fee_to_the_base_rate()
    {
        var results = await new BoutiqueCollectionProvider().SearchAsync(AnyStay);

        var deluxe = results.Single(r => r.RoomType == RoomType.Deluxe);
        Assert.Equal(315m, deluxe.NightlyRate);
        Assert.Equal(CancellationPolicy.FreeCancellation, deluxe.CancellationPolicy);
    }

    [Fact]
    public async Task Standard_is_never_available()
    {
        var results = await new BoutiqueCollectionProvider().SearchAsync(AnyStay);

        Assert.False(results.Single(r => r.RoomType == RoomType.Standard).Available);
    }
}
