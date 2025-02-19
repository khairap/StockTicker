import React, { useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";

const API_BASE_URL = "http://localhost:5000"; // Update with your API URL

const StockTicker = () => {
  const [stockSymbol, setStockSymbol] = useState("AAPL");
  const [price, setPrice] = useState(null);
  const [connection, setConnection] = useState(null);

  useEffect(() => {
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/stockHub`) // Connect to .NET SignalR Hub
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

    connection.on("ReceiveStockUpdate", (symbol, newPrice) => {
      if (symbol === stockSymbol) {
        setPrice(newPrice);
      }
    });

    return () => {
      connection.off("ReceiveStockUpdate");
    };
  }, [connection, stockSymbol]);

  const subscribeToStock = async () => {
    if (!connection) return;
    await connection.invoke("Subscribe", stockSymbol);
    console.log(`Subscribed to ${stockSymbol}`);
  };

  const unsubscribeFromStock = async () => {
    if (!connection) return;
    await connection.invoke("Unsubscribe", stockSymbol);
    console.log(`Unsubscribed from ${stockSymbol}`);
  };

  const fetchStockPrice = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/stocks/${stockSymbol}`);
      const data = await response.json();
      setPrice(data.price);
    } catch (error) {
      console.error("Error fetching stock price:", error);
    }
  };

  return (
    <div style={styles.container}>
      <h2>Stock Ticker</h2>
      <input
        type="text"
        value={stockSymbol}
        onChange={(e) => setStockSymbol(e.target.value.toUpperCase())}
        placeholder="Enter Stock Symbol (e.g., AAPL)"
        style={styles.input}
      />
      <div>
        <button onClick={subscribeToStock} style={styles.button}>Subscribe</button>
        <button onClick={unsubscribeFromStock} style={styles.button}>Unsubscribe</button>
        <button onClick={fetchStockPrice} style={styles.button}>Get Price</button>
      </div>
      {price !== null && <h3>Current Price: ${price.toFixed(2)}</h3>}
    </div>
  );
};

const styles = {
  container: { textAlign: "center", padding: "20px", fontFamily: "Arial, sans-serif" },
  input: { padding: "10px", marginBottom: "10px", fontSize: "16px", width: "200px" },
  button: { margin: "5px", padding: "10px", fontSize: "14px", cursor: "pointer" },
};

export default StockTicker;
