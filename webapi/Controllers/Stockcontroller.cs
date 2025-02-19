using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

[ApiController]
[Route("api/stocks")]
public class StockController : ControllerBase
{
    private readonly StockService _stockService;
    private readonly ILogger<StockController> _logger;

    public StockController(StockService stockService, ILogger<StockController> logger)
    {
        _stockService = stockService;
        _logger = logger;
    }

    [HttpGet("{symbol}")]
    public IActionResult GetStockPrice(string symbol)
    {
        _logger.LogInformation("Received request to get price for {StockSymbol}", symbol);
        decimal price = _stockService.GetStockPrice(symbol);
        if (price == 0) 
        {
            _logger.LogWarning("Stock {StockSymbol} not found.", symbol);
            return NotFound($"Stock '{symbol}' not found.");
        }
        return Ok(new { Symbol = symbol, Price = price });
    }

    [HttpGet]
    public IActionResult GetAllStocks()
    {
        _logger.LogInformation("Received request to get all stocks.");
        var stocks = new List<object>();
        foreach (var symbol in _stockService.GetAllStockSymbols())
        {
            stocks.Add(new { Symbol = symbol, Price = _stockService.GetStockPrice(symbol) });
        }
        return Ok(stocks);
    }

    [HttpPost]
    public IActionResult AddStock([FromBody] string stockSymbol)
    {
        _logger.LogInformation("Received request to add stock: {StockSymbol}", stockSymbol);
        if (string.IsNullOrWhiteSpace(stockSymbol)) return BadRequest("Stock symbol is required.");

        stockSymbol = stockSymbol.ToUpper();
        if (_stockService.AddStock(stockSymbol))
        {
            return CreatedAtAction(nameof(GetStockPrice), new { symbol = stockSymbol }, new { Symbol = stockSymbol, Message = "Stock added successfully." });
        }

        return Conflict($"Stock '{stockSymbol}' already exists.");
    }

    [HttpPut("{symbol}")]
    public IActionResult UpdateStockPrice(string symbol, [FromBody] decimal newPrice)
    {
        _logger.LogInformation("Received request to update price for {StockSymbol} to {Price}", symbol, newPrice);
        if (newPrice <= 0) return BadRequest("Price must be greater than zero.");

        symbol = symbol.ToUpper();
        if (_stockService.UpdateStockPrice(symbol, newPrice))
        {
            return Ok(new { Symbol = symbol, Price = newPrice, Message = "Stock price updated successfully." });
        }

        _logger.LogWarning("Stock {StockSymbol} not found for update.", symbol);
        return NotFound($"Stock '{symbol}' not found.");
    }

    [HttpDelete("{symbol}")]
    public IActionResult DeleteStock(string symbol)
    {
        _logger.LogInformation("Received request to delete stock: {StockSymbol}", symbol);
        symbol = symbol.ToUpper();
        if (_stockService.DeleteStock(symbol))
        {
            return Ok(new { Symbol = symbol, Message = "Stock deleted successfully." });
        }

        _logger.LogWarning("Stock {StockSymbol} not found for deletion.", symbol);
        return NotFound($"Stock '{symbol}' not found.");
    }
}
