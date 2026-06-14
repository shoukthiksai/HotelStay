using System.Text.Json.Serialization;
using HotelStay.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
builder.Services.AddSingleton<HotelSearchService>();
builder.Services.AddSingleton<DocumentValidator>();
builder.Services.AddSingleton<BookingService>();

const string uiCors = "ui";
builder.Services.AddCors(o => o.AddPolicy(uiCors, p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();
app.UseCors(uiCors);

app.MapGet("/hotels/search", async (
    string? destination, DateOnly? checkIn, DateOnly? checkOut, RoomType? roomType, HotelSearchService search) =>
{
    if (string.IsNullOrWhiteSpace(destination) || checkIn is null || checkOut is null)
        return Results.BadRequest(new { message = "destination, checkIn and checkOut are required." });

    if (checkOut <= checkIn)
        return Results.BadRequest(new { message = "checkOut must be after checkIn." });

    var results = await search.SearchAsync(new SearchCriteria(destination, checkIn.Value, checkOut.Value, roomType));
    return Results.Ok(results);
});

app.MapPost("/hotels/book", async (BookingRequest request, DocumentValidator validator, BookingService bookings) =>
{
    if (request.CheckOut <= request.CheckIn)
        return Results.BadRequest(new { message = "checkOut must be after checkIn." });

    var document = validator.Validate(request.Destination, request.DocumentType);
    if (!document.IsValid)
        return Results.UnprocessableEntity(new { message = document.Message });

    var confirmation = await bookings.BookAsync(request);
    return confirmation is null
        ? Results.BadRequest(new { message = "The selected room is not available with this provider." })
        : Results.Ok(confirmation);
});

app.MapGet("/hotels/booking/{reference}", (string reference, BookingService bookings) =>
{
    var booking = bookings.Find(reference);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});

app.Run();
