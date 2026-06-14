namespace HotelStay.Api;

/// The single seam for adding providers. A new provider is a new implementation plus one
/// DI registration — aggregation and booking orchestration never change (Open/Closed).
public interface IHotelProvider
{
    string Name { get; }

    Task<IReadOnlyList<HotelResult>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default);

    /// Returns the offer for the requested room type, or null if the provider can't fulfil it.
    Task<HotelResult?> BookAsync(SearchCriteria criteria, CancellationToken ct = default);
}
