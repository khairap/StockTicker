
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class StockService
{
    private readonly ConcurrentDictionary<string, (decimal Price, DateTime LastUpdated)> _stockCache = new();
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockService> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private readonly Random _random = new();
    public StockService(HttpClient httpClient, ILogger<StockService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get stock price, using cached value if within 5 minutes.
    /// </summary>
    public async Task<decimal> GetStockPriceAsync(string stockSymbol)
    {
        stockSymbol = stockSymbol.ToUpper();

        if (_stockCache.TryGetValue(stockSymbol, out var cacheEntry))
        {
            if (DateTime.UtcNow - cacheEntry.LastUpdated < _cacheDuration)
            {
                _logger.LogInformation("Returning cached price for {StockSymbol}: ${Price} (Cached at {LastUpdated})", stockSymbol, cacheEntry.Price, cacheEntry.LastUpdated);
                return cacheEntry.Price;
            }
        }

        decimal freshPrice = await FetchStockPriceFromYahoo(stockSymbol);
        if (freshPrice > 0)
        {
            _stockCache[stockSymbol] = (freshPrice, DateTime.UtcNow);
        }

        return freshPrice;
    }
private async Task<decimal> FetchStockPriceFromTwelveData(string stockSymbol)
{
    try
    {
        string apiKey = "8e85f39999fe4356980f70cb9d6de218"; // 🔥 Replace with your API key
        string url = $"https://api.twelvedata.com/price?symbol={stockSymbol}&apikey={apiKey}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch stock {StockSymbol} from Twelve Data (HTTP {StatusCode})", stockSymbol, response.StatusCode);
            return 0;
        }

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        string priceString = doc.RootElement.GetProperty("price").GetString(); // ✅ Read as string

        if (decimal.TryParse(priceString, out decimal price))
        {
            _logger.LogInformation("Fetched real-time price for {StockSymbol}: ${Price}", stockSymbol, price);
            return price;
        }
        else
        {
            _logger.LogError("Failed to parse stock price for {StockSymbol}: {RawPrice}", stockSymbol, priceString);
            return 0;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching stock {StockSymbol} from Twelve Data", stockSymbol);
        return 0;
    }
}


    /// <summary>
    /// Fetch real-time stock price from Yahoo Finance API.
    /// </summary>
    private async Task<decimal> FetchStockPriceFromYahoo(string stockSymbol)
    {
        try
        {
            var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={stockSymbol}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch stock {StockSymbol} from Yahoo Finance (HTTP {StatusCode})", stockSymbol, response.StatusCode);
                return 0;
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var quote = doc.RootElement
                           .GetProperty("quoteResponse")
                           .GetProperty("result")[0];

            decimal price = quote.GetProperty("regularMarketPrice").GetDecimal();
            _logger.LogInformation("Fetched real-time price for {StockSymbol}: ${Price}", stockSymbol, price);

            return price;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching stock {StockSymbol} from Yahoo Finance", stockSymbol);
            return 0;
        }
    }

    public async Task<bool> AddStock(string stockSymbol)
    {
        if (!_stockCache.ContainsKey(stockSymbol))
        {
            try{
                _stockCache[stockSymbol] = (await FetchStockPriceFromTwelveData(stockSymbol), DateTime.MinValue);
            }
            catch{
             _stockCache[stockSymbol] = (0, DateTime.MinValue); 
            }
        
            return true;
        }
        return false;
    }

    public bool DeleteStock(string stockSymbol) => _stockCache.TryRemove(stockSymbol, out _);

    public IEnumerable<string> GetAllStockSymbols() => _stockCache.Keys;


        public decimal UpdateStockPrice(string stockSymbol)
    {
        if (_stockCache.TryGetValue(stockSymbol, out var price))
        {
            decimal change = (decimal)_random.NextDouble() * 5 - 2.5m; // Random price change
           // price.Price = Math.Max(0, price.Price + change);
            _stockCache[stockSymbol] = price;
           // _logger.LogInformation("Updated stock {StockSymbol} auto", stockSymbol);
            return price.Price;
        }
        return 0;
    }
        
}