using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class StockHub : Hub
{
    private readonly StockService _stockService;

    public StockHub(StockService stockService)
    {
        _stockService = stockService;
    }

    public async Task Subscribe(string stockSymbol)
    {
        stockSymbol = stockSymbol.ToUpper();

        if (await _stockService.AddStock(stockSymbol)) // ✅ Dynamically add stock if not exists
        {
            await Clients.All.SendAsync("NewStockAdded", stockSymbol);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, stockSymbol);
    }

    public async Task Unsubscribe(string stockSymbol)
    {
        stockSymbol = stockSymbol.ToUpper();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, stockSymbol);
    }
}
