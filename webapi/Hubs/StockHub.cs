using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class StockHub : Hub
{
    public async Task Subscribe(string stockSymbol)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, stockSymbol);
    }

    public async Task Unsubscribe(string stockSymbol)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, stockSymbol);
    }
}
