using HotelStay.Api;

namespace HotelStay.Tests;

public class DocumentValidatorTests
{
    private readonly DocumentValidator _validator = new();

    [Theory]
    [InlineData("Paris", DocumentType.Passport, true)]
    [InlineData("Paris", DocumentType.NationalId, false)]
    [InlineData("London", DocumentType.NationalId, true)]
    [InlineData("London", DocumentType.Passport, true)]
    public void Validates_document_against_destination(string destination, DocumentType document, bool expected)
        => Assert.Equal(expected, _validator.Validate(destination, document).IsValid);

    [Fact]
    public void International_with_national_id_explains_the_requirement()
    {
        var result = _validator.Validate("Tokyo", DocumentType.NationalId);

        Assert.False(result.IsValid);
        Assert.Contains("passport", result.Message!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Unknown_destination_is_rejected()
        => Assert.False(_validator.Validate("Atlantis", DocumentType.Passport).IsValid);
}
