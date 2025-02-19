import React, { useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";

const API_BASE_URL = "http://localhost:5000"; // Update with your API URL

const StockTicker = () => {
  const [stockSymbol, setStockSymbol] = useState("");
  const [subscribedStocks, setSubscribedStocks] = useState({});
  const [connection, setConnection] = useState(null);

  useEffect(() => {
    console.log("Initializing SignalR connection...");
  
    const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/stockHub", {
        withCredentials: true, // ✅ Required for CORS
        transport: signalR.HttpTransportType.WebSockets // ✅ Force WebSockets
    })
    .configureLogging(signalR.LogLevel.Information) // ✅ Enable Debug Logs
    .withAutomaticReconnect()
    .build();
  
    hubConnection
      .start()
      .then(() => {
        console.log("Connected to SignalR!");
        setConnection(hubConnection);
      })
      .catch((err) => console.error("Error connecting to SignalR:", err));
  
    return () => {
      if (hubConnection) {
        console.log("Stopping SignalR connection...");
        hubConnection.stop();
      }
    };
  }, []);
  

  useEffect(() => {
    console.log("StockUpdate...");
    if (!connection) return;

    connection.on("ReceiveStockUpdate", (symbol, price) => {
      setSubscribedStocks((prev) => ({
        ...prev,
        [symbol]: price.toFixed(2),
      }));
    });

    return () => {
      connection.off("ReceiveStockUpdate");
    };
  }, [connection]);

  const subscribeToStock = async () => {
    if (!connection || !stockSymbol.trim()) return;

    const symbol = stockSymbol.toUpperCase();

    if (!subscribedStocks[symbol]) {
      await connection.invoke("Subscribe", symbol);
      setSubscribedStocks((prev) => ({
        ...prev,
        [symbol]: "Fetching...",
      }));
    }
    setStockSymbol(""); // Clear input
  };

  const unsubscribeFromStock = async (symbol) => {
    if (!connection) return;

    await connection.invoke("Unsubscribe", symbol);
    setSubscribedStocks((prev) => {
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
  container: {
    textAlign: "center",
    padding: "20px",
    fontFamily: "Arial, sans-serif",
  },
  input: {
    padding: "10px",
    fontSize: "16px",
    width: "200px",
    marginRight: "10px",
  },
  button: {
    padding: "10px",
    fontSize: "14px",
    cursor: "pointer",
    margin: "5px",
  },
  table: {
    width: "100%",
    marginTop: "20px",
    borderCollapse: "collapse",
  },
  th: {
    borderBottom: "1px solid black",
    padding: "10px",
  },
  td: {
    borderBottom: "1px solid #ddd",
    padding: "10px",
    textAlign: "center",
  },
};

export default StockTicker;
