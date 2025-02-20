using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BlazorStockApp.Services
{
    public class StockService
    {
        private readonly HttpClient _httpClient;
        private readonly HubConnection _hubConnection;
        private readonly Dictionary<string, decimal> _stockPrices = new();
        public event Action<string, decimal> OnStockUpdated;

        public StockService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/stockHub") // Ensure this matches your API URL
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string, decimal>("ReceiveStockUpdate", (symbol, price) =>
            {
                _stockPrices[symbol] = price;
                OnStockUpdated?.Invoke(symbol, price);
            });

            _hubConnection.StartAsync();
        }

        public async Task<List<(string Symbol, decimal Price)>> GetAllStocksAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<(string, decimal)>>("api/stocks");
        }

        public async Task SubscribeToStock(string symbol)
        {
            await _hubConnection.SendAsync("Subscribe", symbol);
        }

        public async Task UnsubscribeFromStock(string symbol)
        {
            await _hubConnection.SendAsync("Unsubscribe", symbol);
        }

        public decimal GetStockPrice(string symbol) => _stockPrices.GetValueOrDefault(symbol, 0);
    }
}
