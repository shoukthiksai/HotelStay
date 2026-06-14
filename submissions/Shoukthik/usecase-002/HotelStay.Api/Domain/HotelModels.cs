namespace HotelStay.Api;

public record SearchCriteria(string Destination, DateOnly CheckIn, DateOnly CheckOut, RoomType? RoomType);

/// Unified room offer. Amenities and StarRating are null for providers that don't supply them.
public record HotelResult(
    string Provider,
    RoomType RoomType,
    decimal NightlyRate,
    CancellationPolicy CancellationPolicy,
    string[]? Amenities,
    int? StarRating,
    bool Available);
