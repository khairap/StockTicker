using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

public class StockService
{
    private readonly ConcurrentDictionary<string, decimal> _stockPrices = new();
    private readonly Random _random = new();
    private readonly ILogger<StockService> _logger;

    public StockService(ILogger<StockService> logger)
    {
        _logger = logger;
    }

    public decimal GetStockPrice(string stockSymbol)
    {
        _stockPrices.TryGetValue(stockSymbol, out var price);
        _logger.LogInformation("Fetched price for {StockSymbol}: {Price}", stockSymbol, price);
        return price;
    }

    public bool AddStock(string stockSymbol)
    {
        if (!_stockPrices.ContainsKey(stockSymbol))
        {
            decimal initialPrice = 100.0m + (decimal)_random.NextDouble() * 50;
            _stockPrices[stockSymbol] = initialPrice;
            _logger.LogInformation("Added new stock: {StockSymbol} with initial price {Price}", stockSymbol, initialPrice);
            return true;
        }

        _logger.LogWarning("Stock {StockSymbol} already exists.", stockSymbol);
        return false;
    }

    public bool DeleteStock(string stockSymbol)
    {
        if (_stockPrices.TryRemove(stockSymbol, out _))
        {
            _logger.LogInformation("Deleted stock: {StockSymbol}", stockSymbol);
            return true;
        }

        _logger.LogWarning("Attempted to delete non-existing stock: {StockSymbol}", stockSymbol);
        return false;
    }

    public bool UpdateStockPrice(string stockSymbol, decimal newPrice)
    {
        if (_stockPrices.ContainsKey(stockSymbol))
        {
            _stockPrices[stockSymbol] = newPrice;
            _logger.LogInformation("Updated stock {StockSymbol} to new price {Price}", stockSymbol, newPrice);
            return true;
        }

        _logger.LogWarning("Attempted to update price of non-existing stock: {StockSymbol}", stockSymbol);
        return false;
    }


        public decimal UpdateStockPrice(string stockSymbol)
    {
        if (_stockPrices.TryGetValue(stockSymbol, out var price))
        {
            decimal change = (decimal)_random.NextDouble() * 5 - 2.5m; // Random price change
            price = Math.Max(0, price + change);
            _stockPrices[stockSymbol] = price;
            _logger.LogInformation("Updated stock {StockSymbol} auto", stockSymbol);
            return price;
        }
        return 0;
    }

    public IEnumerable<string> GetAllStockSymbols()
    {
        return _stockPrices.Keys;
    }
}





