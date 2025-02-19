using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class StockUpdateBackgroundService : BackgroundService
{
    private readonly IHubContext<StockHub> _hubContext;
    private readonly StockService _stockService;

    public StockUpdateBackgroundService(IHubContext<StockHub> hubContext, StockService stockService)
    {
        _hubContext = hubContext;
        _stockService = stockService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var symbol in _stockService.GetAllStockSymbols()) // ✅ Update all stocks
            {
                decimal newPrice = _stockService.UpdateStockPrice(symbol);
                await _hubContext.Clients.Group(symbol).SendAsync("ReceiveStockUpdate", symbol, newPrice);
            }
            await Task.Delay(5000, stoppingToken);
        }
    }
}
