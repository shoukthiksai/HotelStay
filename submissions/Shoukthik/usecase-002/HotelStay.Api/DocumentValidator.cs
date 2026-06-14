namespace HotelStay.Api;

public record DocumentValidationResult(bool IsValid, string? Message)
{
    public static readonly DocumentValidationResult Valid = new(true, null);

    public static DocumentValidationResult Invalid(string message) => new(false, message);
}

/// A passport is accepted anywhere; a national ID only domestically. So an international
/// destination requires a passport — anything else is rejected with 422 upstream.
public sealed class DocumentValidator
{
    public DocumentValidationResult Validate(string destination, DocumentType documentType)
    {
        if (!Destinations.IsKnown(destination))
            return DocumentValidationResult.Invalid($"Unknown destination '{destination}'.");

        if (Destinations.IsInternational(destination) && documentType != DocumentType.Passport)
            return DocumentValidationResult.Invalid($"A passport is required for international destinations ({destination}).");

        return DocumentValidationResult.Valid;
    }
}
