using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// Get the current real-time stock price from Yahoo Finance.
    /// </summary>
    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetStockPrice(string symbol)
    {
        _logger.LogInformation("Fetching real-time price for {StockSymbol}", symbol);
        decimal price = await _stockService.GetStockPriceAsync(symbol);
        if (price == 0) return NotFound($"Stock '{symbol}' not found or failed to fetch.");
        return Ok(new { Symbol = symbol, Price = price });
    }

    /// <summary>
    /// Get all subscribed stocks.
    /// </summary>
    [HttpGet]
    public IActionResult GetAllStocks()
    {
        _logger.LogInformation("Fetching all stocks");
        var stocks = new List<object>();
        foreach (var symbol in _stockService.GetAllStockSymbols())
        {
            stocks.Add(new { Symbol = symbol });
        }
        return Ok(stocks);
    }

    /// <summary>
    /// Add a new stock symbol.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddStock([FromBody] string stockSymbol)
    {
        if (string.IsNullOrWhiteSpace(stockSymbol)) return BadRequest("Stock symbol required.");

        stockSymbol = stockSymbol.ToUpper();
        if (await _stockService.AddStock(stockSymbol ))
        {
            return CreatedAtAction(nameof(GetStockPrice), new { symbol = stockSymbol }, new { Symbol = stockSymbol, Message = "Stock added successfully." });
        }

        return Conflict($"Stock '{stockSymbol}' already exists.");
    }

    /// <summary>
    /// Delete a stock symbol.
    /// </summary>
    [HttpDelete("{symbol}")]
    public IActionResult DeleteStock(string symbol)
    {
        symbol = symbol.ToUpper();
        if (_stockService.DeleteStock(symbol))
        {
            return Ok(new { Symbol = symbol, Message = "Stock deleted successfully." });
        }
        return NotFound($"Stock '{symbol}' not found.");
    }
}
