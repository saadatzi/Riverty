using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestCurrencyApi.Models; // Make sure namespace matches your project
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// API Endpoint 1: Get Exchange Rates (Latest or Historical)
app.MapGet("/rates", (string dateType, string? date) =>
{
    if (dateType != "Latest" && dateType != "Historical")
    {
        return Results.BadRequest("Invalid dateType. Must be 'Latest' or 'Historical'.");
    }

    var random = new Random();
    var rates = new Dictionary<string, decimal>();
    string[] currencies = { "USD", "AUD", "CAD", "PLN", "MXN" };

    foreach (var currency in currencies)
    {
        rates[currency] = 1 + (decimal)random.NextDouble() * 20;
    }

    DateTime rateDate = DateTime.UtcNow.Date;
    if (dateType == "Historical" && DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
    {
        rateDate = parsedDate;
    }

    var response = new ExchangeRateResponse
    {
        BaseCurrency = "EUR",
        Rates = rates
    };

    return Results.Ok(response);
})
.WithName("GetRates")
.WithOpenApi();

// API Endpoint 2: Get Historical Rates for a Currency and Period
app.MapGet("/historical-rates/{currencyCode}", (string currencyCode, string startDate, string endDate) =>
{
    if (currencyCode.Length != 3)
    {
        return Results.BadRequest("Invalid currencyCode. Must be a 3-letter currency code.");
    }

    if (!DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var startDateUtc))
    {
        return Results.BadRequest("Invalid startDate format. Please use yyyy-MM-dd for startDate.");
    }
    startDateUtc = startDateUtc.Date;

    if (!DateTime.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var endDateUtc))
    {
        return Results.BadRequest("Invalid endDate format. Please use yyyy-MM-dd for endDate.");
    }
    endDateUtc = endDateUtc.Date;

    if (startDateUtc > endDateUtc)
    {
        return Results.BadRequest("startDate must be before or equal to endDate.");
    }

    var random = new Random();
    var historicalRates = new List<HistoricalRateData>();
    for (DateTime date = startDateUtc; date <= endDateUtc; date = date.AddDays(1))
    {
        var rates = new Dictionary<string, decimal>();
        string[] currencies = { "USD", "AUD", "CAD", "PLN", "MXN" }; // Example currencies

        foreach (var currency in currencies)
        {
            rates[currency] = 1 + (decimal)random.NextDouble() * 20; // Random rate between 1 and 21
        }

        historicalRates.Add(new HistoricalRateData
        {
            RateDate = date,
            Rates = rates
        });
    }

    return Results.Ok(historicalRates);
})
.WithName("GetHistoricalRates")
.WithOpenApi();


app.Run();