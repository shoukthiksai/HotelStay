namespace HotelStay.Api;

/// Home country is the UK. The Angular form mirrors this list; the booking guard is the
/// authoritative copy used for document validation.
public static class Destinations
{
    private static readonly HashSet<string> Domestic = new(StringComparer.OrdinalIgnoreCase) { "London", "Manchester" };

    private static readonly HashSet<string> International =
        new(StringComparer.OrdinalIgnoreCase) { "Paris", "New York", "Tokyo", "Dubai", "Sydney" };

    public static bool IsKnown(string destination) => Domestic.Contains(destination) || International.Contains(destination);

    public static bool IsInternational(string destination) => International.Contains(destination);
}
