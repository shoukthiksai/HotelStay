using System.Text.Json.Serialization;

namespace HotelStay.Api;

/// Boutique brand. Offers Deluxe and Suite only (Standard is never available) and adds a
/// flat per-night fee to its base rate. snake_case wire format, no amenities or star rating.
public sealed class BoutiqueCollectionProvider : IHotelProvider
{
    private const decimal BoutiqueFee = 15m;

    public string Name => "BoutiqueCollection";

    private sealed record Room(
        [property: JsonPropertyName("room_type")] string RoomType,
        [property: JsonPropertyName("base_rate")] decimal BaseRate,
        [property: JsonPropertyName("available")] bool Available);

    private static readonly Room[] Catalog =
    [
        new("standard", 280m, false),
        new("deluxe", 300m, true),
        new("suite", 500m, true),
    ];

    public Task<IReadOnlyList<HotelResult>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<HotelResult>>([.. Catalog.Select(Normalise)]);

    public Task<HotelResult?> BookAsync(SearchCriteria criteria, CancellationToken ct = default)
        => Task.FromResult(Catalog.Select(Normalise).FirstOrDefault(r => r.RoomType == criteria.RoomType));

    private static HotelResult Normalise(Room r) => new(
        Provider: "BoutiqueCollection",
        RoomType: r.RoomType switch
        {
            "standard" => RoomType.Standard,
            "deluxe" => RoomType.Deluxe,
            "suite" => RoomType.Suite,
            _ => throw new ArgumentOutOfRangeException(nameof(r), r.RoomType, "Unknown BoutiqueCollection room type."),
        },
        NightlyRate: r.BaseRate + BoutiqueFee,
        CancellationPolicy: CancellationPolicy.FreeCancellation,
        Amenities: null,
        StarRating: null,
        Available: r.Available);
}
