using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Riverty.ExchangeRateCalculator.Services;
using ScheduledRateUpdater.Data;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddSingleton<CurrencyService>();

builder.Services.AddDbContext<RateDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Exchange rates API",
        Version = "v1",
        Description = "This exchange rates API shows the latest (reflect it from fixer ;) - api provisioning using others api) and periodic result from the DB",
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exchange rates API v1");
        });
}

app.UseHttpsRedirection();

// API Endpoint 1: Get Exchange Rates (Latest or Historical)
app.MapGet("/rates", async (string dateType, string? date, CurrencyService currencyService) =>
{
    if (dateType != "Latest" && dateType != "Historical")
    {
        return Results.BadRequest("Invalid dateType. Must be 'Latest' or 'Historical'.");
    }

    var ratesResponse = await currencyService.GetExchangeRatesAsync(dateType, date);
    if (ratesResponse == null)
    {
        return Results.Problem(statusCode: 500, title: "Failed to retrieve exchange rates.");
    }

    return Results.Ok(ratesResponse);
})
.WithName("GetExchangeRates")
.WithOpenApi();

// API Endpoint 2: Get Historical Rates for a Currency and Period
app.MapGet("/historical-rates/{currencyCode}", async (string currencyCode, string startDate, string endDate, RateDbContext dbContext) =>
{
    if (currencyCode.Length != 3)
    {
        return Results.BadRequest("Invalid currencyCode. Must be a 3-letter currency code.");
    }

    // Parse startDate and endDate as DateTime (UTC)
    if (!DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var startDateUtc))
    {
        return Results.BadRequest("Invalid startDate format. Please use yyyy-MM-dd.");
    }
    startDateUtc = startDateUtc.Date;

    if (!DateTime.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var endDateUtc))
    {
        return Results.BadRequest("Invalid endDate format. Please use yyyy-MM-dd.");
    }
    endDateUtc = endDateUtc.Date;

    if (startDateUtc > endDateUtc)
    {
        return Results.BadRequest("startDate must be before or equal to endDate.");
    }

    // Query the database using startDateUtc and endDateUtc
    var historicalRates = await dbContext.ExchangeRates
        .Where(r => r.RateDate >= startDateUtc && r.RateDate <= endDateUtc)
        .Select(r => new
        {
            r.RateDate,
            Rates = new Dictionary<string, decimal>()
            {
                { "USD", r.USD },
                { "AUD", r.AUD },
                { "CAD", r.CAD },
                { "PLN", r.PLN },
                { "MXN", r.MXN }
            }
        })
        .ToListAsync();

    return Results.Ok(historicalRates);
})
.WithName("GetHistoricalRates")
.WithOpenApi();


app.Run();