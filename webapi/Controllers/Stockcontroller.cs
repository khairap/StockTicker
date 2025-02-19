using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/stocks")]
public class StockController : ControllerBase
{
    private readonly StockService _stockService;

    public StockController(StockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>
    /// Get the current stock price for a given symbol.
    /// </summary>
    /// <param name="symbol">Stock ticker symbol (e.g., AAPL, TSLA)</param>
    /// <returns>Current stock price</returns>
    [HttpGet("{symbol}")]
    public IActionResult GetStockPrice(string symbol)
    {
        decimal price = _stockService.GetStockPrice(symbol);
        return Ok(new { Symbol = symbol, Price = price });
    }
}
