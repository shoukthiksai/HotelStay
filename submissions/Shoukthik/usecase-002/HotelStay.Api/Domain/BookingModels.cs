namespace HotelStay.Api;

public record BookingRequest(
    string Provider,
    RoomType RoomType,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string Destination,
    string PassengerName,
    DocumentType DocumentType,
    string DocumentNumber);

public record BookingConfirmation(
    string ReferenceNumber,
    string Provider,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy);

public record Booking(
    string Reference,
    string Provider,
    string Destination,
    RoomType RoomType,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string PassengerName,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy,
    string Status);
