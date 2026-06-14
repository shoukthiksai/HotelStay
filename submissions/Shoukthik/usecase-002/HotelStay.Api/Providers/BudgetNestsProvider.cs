using System.Text.Json.Serialization;

namespace HotelStay.Api;

/// Budget provider. Wire format is snake_case with no amenities or star rating, and may
/// mark rooms unavailable — the aggregation layer is responsible for filtering those out.
public sealed class BudgetNestsProvider : IHotelProvider
{
    public string Name => "BudgetNests";

    private sealed record Room(
        [property: JsonPropertyName("room_type")] string RoomType,
        [property: JsonPropertyName("rate")] decimal Rate,
        [property: JsonPropertyName("cancellation_policy")] string CancellationPolicy,
        [property: JsonPropertyName("available")] bool Available);

    private static readonly Room[] Catalog =
    [
        new("standard", 95m, "flexible", true),
        new("deluxe", 140m, "non_refundable", false),
        new("suite", 210m, "flexible", true),
    ];

    public Task<IReadOnlyList<HotelResult>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<HotelResult>>([.. Catalog.Select(Normalise)]);

    public Task<HotelResult?> BookAsync(SearchCriteria criteria, CancellationToken ct = default)
        => Task.FromResult(Catalog.Select(Normalise).FirstOrDefault(r => r.RoomType == criteria.RoomType));

    private static HotelResult Normalise(Room r) => new(
        Provider: "BudgetNests",
        RoomType: r.RoomType switch
        {
            "standard" => RoomType.Standard,
            "deluxe" => RoomType.Deluxe,
            "suite" => RoomType.Suite,
            _ => throw new ArgumentOutOfRangeException(nameof(r), r.RoomType, "Unknown BudgetNests room type."),
        },
        NightlyRate: r.Rate,
        CancellationPolicy: r.CancellationPolicy switch
        {
            "flexible" => CancellationPolicy.Flexible,
            "non_refundable" => CancellationPolicy.NonRefundable,
            _ => throw new ArgumentOutOfRangeException(nameof(r), r.CancellationPolicy, "Unknown BudgetNests cancellation policy."),
        },
        Amenities: null,
        StarRating: null,
        Available: r.Available);
}
