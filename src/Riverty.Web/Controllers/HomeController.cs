using Microsoft.AspNetCore.Mvc;
using Riverty.Web.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Riverty.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }
    public IActionResult HistoricalRates()
    {
        var viewModel = new HistoricalRatesViewModel
        {
            HistoricalRates = new List<HistoricalRateData>()
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> HistoricalRates(HistoricalRatesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            using var client = _clientFactory.CreateClient();
            var startDateFormatted = model.StartDate?.ToString("yyyy-MM-dd");
            var endDateFormatted = model.EndDate?.ToString("yyyy-MM-dd");

            string apiUrl = $"http://localhost:5000/historical-rates/{model.CurrencyCode}?startDate={startDateFormatted}&endDate={endDateFormatted}";

            var httpResponse = await client.GetAsync(apiUrl);
            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();
            var historicalRates = JsonSerializer.Deserialize<List<HistoricalRateData>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            model.HistoricalRates = historicalRates;

            return View(model);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling API");
            ModelState.AddModelError("", "Error retrieving data from the API.");
            return View(model);
        }
    }

    public IActionResult Index()
    {
        var viewModel = new RateViewModel
        {
            Request = new RateRequestModel { DateType = "Latest" }
        };
        return View(viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> Index(RateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        using var client = _clientFactory.CreateClient();

        RateResponseModel? rateResponse = null;
        RateRequestModel model = viewModel.Request;

        if (model.DateType == "Latest" || string.IsNullOrEmpty(model.Date?.ToString()))
        {
            string apiUrl = $"http://localhost:5000/rates?dateType={model.DateType}&date=";

            var httpResponse = await client.GetAsync(apiUrl);
            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();

            // Parse the JSON using System.Text.Json
            var jsonDocument = JsonDocument.Parse(content);

            if (jsonDocument.RootElement.TryGetProperty("rates", out JsonElement ratesElement))
            {
                decimal sourceRateToEur, targetRateToEur;

                string sourceCurrency = model.SourceCurrency ?? "EUR";
                string targetCurrency = model.TargetCurrency ?? "USD";
                if (sourceCurrency == "EUR")
                {
                    sourceRateToEur = 1.0m;
                }
                else if (ratesElement.TryGetProperty(sourceCurrency, out JsonElement sourceRateElement) && sourceRateElement.TryGetDecimal(out sourceRateToEur))
                {
                    sourceRateToEur = 1.0m / sourceRateToEur;
                }
                else
                {
                    ModelState.AddModelError("Request.SourceCurrency", "Could not find exchange rate for source currency");
                    return View(viewModel);
                }

                if (targetCurrency == "EUR")
                {
                    targetRateToEur = 1.0m;
                }
                else if (ratesElement.TryGetProperty(targetCurrency, out JsonElement targetRateElement) && targetRateElement.TryGetDecimal(out targetRateToEur))
                {
                    targetRateToEur = targetRateElement.GetDecimal();
                }
                else
                {
                    ModelState.AddModelError("Request.TargetCurrency", "Could not find exchange rate for target currency");
                    return View(viewModel);
                }

                decimal convertedAmount = model.Amount * targetRateToEur * sourceRateToEur;
                rateResponse = new RateResponseModel
                {
                    SourceCurrency = model.SourceCurrency,
                    TargetCurrency = model.TargetCurrency,
                    DateType = model.DateType,
                    ConvertedAmount = convertedAmount
                };
            }
            else
            {
                ModelState.AddModelError("Api Error", "Problem in response");
                return View(viewModel);
            }
        }
        else if (model.DateType == "Historical" && model.Date != null)
        {
            string apiUrl = $"http://localhost:5000/rates?dateType=Historical&date={model.Date?.ToString("yyyy-MM-dd")}";

            var httpResponse = await client.GetAsync(apiUrl);
            httpResponse.EnsureSuccessStatusCode();
            var content = await httpResponse.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);

            if (jsonDocument.RootElement.TryGetProperty("rates", out JsonElement ratesElement))
            {
                decimal sourceRateToEur, targetRateToEur;

                string sourceCurrency = model.SourceCurrency ?? "EUR";
                string targetCurrency = model.TargetCurrency ?? "USD";
                if (sourceCurrency == "EUR")
                {
                    sourceRateToEur = 1.0m;
                }
                else if (ratesElement.TryGetProperty(sourceCurrency, out JsonElement sourceRateElement) && sourceRateElement.TryGetDecimal(out sourceRateToEur))
                {
                    sourceRateToEur = 1.0m / sourceRateToEur;
                }
                else
                {
                    ModelState.AddModelError("Request.SourceCurrency", "Could not find exchange rate for source currency");
                    return View(viewModel);
                }

                if (targetCurrency == "EUR")
                {
                    targetRateToEur = 1.0m;
                }
                else if (ratesElement.TryGetProperty(targetCurrency, out JsonElement targetRateElement) && targetRateElement.TryGetDecimal(out targetRateToEur))
                {
                    targetRateToEur = targetRateElement.GetDecimal();
                }
                else
                {
                    ModelState.AddModelError("Request.TargetCurrency", "Could not find exchange rate for target currency");
                    return View(viewModel);
                }

                decimal convertedAmount = model.Amount * targetRateToEur * sourceRateToEur;
                rateResponse = new RateResponseModel
                {
                    SourceCurrency = model.SourceCurrency,
                    TargetCurrency = model.TargetCurrency,
                    DateType = model.DateType,
                    Date = model.Date?.ToString("yyyy-MM-dd"),
                    ConvertedAmount = convertedAmount
                };
            }
            else
            {
                ModelState.AddModelError("Api Error", "Problem in response");
                return View(viewModel);
            }
        }

        if (rateResponse == null)
        {
            ModelState.AddModelError("Api Error", "Response could not be created");
            return View(viewModel);
        }

        viewModel.Response = rateResponse;

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}