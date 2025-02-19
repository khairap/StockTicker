using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class StockUpdateBackgroundService : BackgroundService
{
    private readonly IHubContext<StockHub> _hubContext;
    private readonly StockService _stockService;
    private readonly string[] _stockSymbols = { "AAPL", "TSLA", "MSFT" };

    public StockUpdateBackgroundService(IHubContext<StockHub> hubContext, StockService stockService)
    {
        _hubContext = hubContext;
        _stockService = stockService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var symbol in _stockSymbols)
            {
                decimal newPrice = _stockService.UpdateStockPrice(symbol);
                await _hubContext.Clients.Group(symbol).SendAsync("ReceiveStockUpdate", symbol, newPrice);
            }
            await Task.Delay(5000, stoppingToken); // Update every 5 seconds
        }
    }
}
