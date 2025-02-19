import React, { useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";

const API_BASE_URL = "http://localhost:5000"; 

const StockTicker = () => {
  const [stockSymbol, setStockSymbol] = useState("");
  const [subscribedStocks, setSubscribedStocks] = useState({});
  const [connection, setConnection] = useState(null);

  useEffect(() => {
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/stockHub`, {
        withCredentials: true, 
        transport: signalR.HttpTransportType.WebSockets 
      })
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    hubConnection.start()
      .then(() => {
        console.log("Connected to SignalR");
        setConnection(hubConnection);
      })
      .catch(err => console.error("Error connecting to SignalR:", err));

    return () => {
      if (hubConnection) {
        hubConnection.stop();
      }
    };
  }, []);

  useEffect(() => {
    if (!connection) return;

    connection.on("ReceiveStockUpdate", (symbol, price) => {
      setSubscribedStocks(prev => ({
        ...prev,
        [symbol]: price.toFixed(2),
      }));
    });

    connection.on("NewStockAdded", (symbol) => {
      console.log(`New Stock Added: ${symbol}`);
    });

    return () => {
      connection.off("ReceiveStockUpdate");
      connection.off("NewStockAdded");
    };
  }, [connection]);

  const subscribeToStock = async () => {
    if (!connection || !stockSymbol.trim()) return;

    const symbol = stockSymbol.toUpperCase();

    if (!subscribedStocks[symbol]) {
      await connection.invoke("Subscribe", symbol);
      setSubscribedStocks(prev => ({
        ...prev,
        [symbol]: "Fetching...",
      }));
    }
    setStockSymbol(""); 
  };

  const unsubscribeFromStock = async (symbol) => {
    if (!connection) return;

    await connection.invoke("Unsubscribe", symbol);
    setSubscribedStocks(prev => {
      const updatedStocks = { ...prev };
      delete updatedStocks[symbol];
      return updatedStocks;
    });
  };

  return (
    <div style={styles.container}>
      <h2>Live Stock Ticker</h2>
      <input
        type="text"
        value={stockSymbol}
        onChange={(e) => setStockSymbol(e.target.value.toUpperCase())}
        placeholder="Enter Stock Symbol (e.g., AAPL)"
        style={styles.input}
      />
      <button onClick={subscribeToStock} style={styles.button}>
        Subscribe
      </button>

      <h3>Subscribed Stocks</h3>
      <table style={styles.table}>
        <thead>
          <tr>
            <th>Stock</th>
            <th>Price</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {Object.entries(subscribedStocks).map(([symbol, price]) => (
            <tr key={symbol}>
              <td>{symbol}</td>
              <td>${price}</td>
              <td>
                <button
                  onClick={() => unsubscribeFromStock(symbol)}
                  style={styles.button}
                >
                  Unsubscribe
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

const styles = {
  container: { textAlign: "center", padding: "20px", fontFamily: "Arial, sans-serif" },
  input: { padding: "10px", fontSize: "16px", width: "200px", marginRight: "10px" },
  button: { padding: "10px", fontSize: "14px", cursor: "pointer", margin: "5px" },
  table: { width: "100%", marginTop: "20px", borderCollapse: "collapse" },
};

export default StockTicker;
