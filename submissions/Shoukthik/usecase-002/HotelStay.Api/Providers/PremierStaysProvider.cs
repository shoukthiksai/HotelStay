namespace HotelStay.Api;

/// Premium provider. Wire format is PascalCase and includes amenities + star rating.
/// All requested room types are always available.
public sealed class PremierStaysProvider : IHotelProvider
{
    public string Name => "PremierStays";

    private sealed record Room(string RoomType, decimal Rate, string CancellationPolicy, string[] Amenities, int StarRating);

    private static readonly Room[] Catalog =
    [
        new("Standard", 180m, "FreeCancellation", ["WiFi", "Breakfast"], 4),
        new("Deluxe", 260m, "FreeCancellation", ["WiFi", "Breakfast", "Minibar"], 5),
        new("Suite", 420m, "NonRefundable", ["WiFi", "Breakfast", "Minibar", "Lounge Access"], 5),
    ];

    public Task<IReadOnlyList<HotelResult>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<HotelResult>>([.. Catalog.Select(Normalise)]);

    public Task<HotelResult?> BookAsync(SearchCriteria criteria, CancellationToken ct = default)
        => Task.FromResult(Catalog.Select(Normalise).FirstOrDefault(r => r.RoomType == criteria.RoomType));

    private static HotelResult Normalise(Room r) => new(
        Provider: "PremierStays",
        RoomType: ParseRoomType(r.RoomType),
        NightlyRate: r.Rate,
        CancellationPolicy: r.CancellationPolicy switch
        {
            "FreeCancellation" => CancellationPolicy.FreeCancellation,
            "NonRefundable" => CancellationPolicy.NonRefundable,
            _ => throw new ArgumentOutOfRangeException(nameof(r), r.CancellationPolicy, "Unknown PremierStays cancellation policy."),
        },
        Amenities: r.Amenities,
        StarRating: r.StarRating,
        Available: true);

    private static RoomType ParseRoomType(string value) => value switch
    {
        "Standard" => RoomType.Standard,
        "Deluxe" => RoomType.Deluxe,
        "Suite" => RoomType.Suite,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown PremierStays room type."),
    };
}
