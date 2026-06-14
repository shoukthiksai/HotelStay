namespace HotelStay.Api;

/// Queries every registered provider in parallel and merges the results. A single provider
/// failing yields partial results rather than failing the whole search. Unavailable rooms
/// (e.g. BudgetNests "available": false) are filtered here, not in the providers.
public sealed class HotelSearchService(IEnumerable<IHotelProvider> providers)
{
    public async Task<IReadOnlyList<HotelResult>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default)
    {
        var batches = await Task.WhenAll(providers.Select(p => SafeSearch(p, criteria, ct)));

        return [.. batches
            .SelectMany(b => b)
            .Where(r => r.Available && (criteria.RoomType is null || r.RoomType == criteria.RoomType))];
    }

    private static async Task<IReadOnlyList<HotelResult>> SafeSearch(IHotelProvider provider, SearchCriteria criteria, CancellationToken ct)
    {
        try
        {
            return await provider.SearchAsync(criteria, ct);
        }
        catch
        {
            return [];
        }
    }
}
