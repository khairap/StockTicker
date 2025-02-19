using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class StockService
{
    private readonly ConcurrentDictionary<string, decimal> _stockPrices = new()
    {
        ["AAPL"] = 150.0m,
        ["TSLA"] = 700.0m,
        ["MSFT"] = 250.0m
    };

    private readonly Random _random = new();

    public decimal GetStockPrice(string stockSymbol)
    {
        _stockPrices.TryGetValue(stockSymbol, out var price);
        return price;
    }

    public decimal UpdateStockPrice(string stockSymbol)
    {
        if (_stockPrices.TryGetValue(stockSymbol, out var price))
        {
            decimal change = (decimal)_random.NextDouble() * 5 - 2.5m; // Random change between -2.5 and +2.5
            price = Math.Max(0, price + change); // Ensure price doesn't go negative
            _stockPrices[stockSymbol] = price;
            return price;
        }
        return 0;
    }
}
