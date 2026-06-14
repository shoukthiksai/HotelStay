using System.Collections.Concurrent;

namespace HotelStay.Api;

/// Routes a booking to the selected provider, prices the stay, and stores the confirmation
/// in memory. Provider lookup is by name, so new providers need no change here.
public sealed class BookingService(IEnumerable<IHotelProvider> providers)
{
    private readonly ConcurrentDictionary<string, Booking> _bookings = new();

    public async Task<BookingConfirmation?> BookAsync(BookingRequest request, CancellationToken ct = default)
    {
        var provider = providers.FirstOrDefault(p => p.Name == request.Provider);
        if (provider is null)
            return null;

        var criteria = new SearchCriteria(request.Destination, request.CheckIn, request.CheckOut, request.RoomType);
        var room = await provider.BookAsync(criteria, ct);
        if (room is null || !room.Available)
            return null;

        var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
        var total = nights * room.NightlyRate;
        var reference = $"HS-{Guid.NewGuid():N}"[..11].ToUpperInvariant();

        _bookings[reference] = new Booking(
            reference, provider.Name, request.Destination, room.RoomType,
            request.CheckIn, request.CheckOut, request.PassengerName, total, room.CancellationPolicy, "Confirmed");

        return new BookingConfirmation(reference, provider.Name, total, room.CancellationPolicy);
    }

    public Booking? Find(string reference) => _bookings.GetValueOrDefault(reference);
}
