using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class StockService
{
    private readonly ConcurrentDictionary<string, decimal> _stockPrices = new();
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
            decimal change = (decimal)_random.NextDouble() * 5 - 2.5m; // Random price change
            price = Math.Max(0, price + change);
            _stockPrices[stockSymbol] = price;
            return price;
        }
        return 0;
    }

    public bool AddStock(string stockSymbol)
    {
        if (!_stockPrices.ContainsKey(stockSymbol))
        {
            _stockPrices[stockSymbol] = 100.0m + (decimal)_random.NextDouble() * 50; // Initial price
            return true;
        }
        return false; // Stock already exists
    }

    // ✅ Add this method to return all stock symbols dynamically
    public IEnumerable<string> GetAllStockSymbols()
    {
        return _stockPrices.Keys;
    }
}
