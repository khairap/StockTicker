import { Component, WritableSignal, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


interface Stock {
    symbol: string;
    price: number;
  }

@Component({
  selector: 'app-stock-ticker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h3>Stock Ticker</h3>
    <input [(ngModel)]="stockSymbol" placeholder="Enter Stock Symbol" />
    <button (click)="subscribeToStock()">Subscribe</button>

    <table *ngIf="getStockKeys().length">
      <thead>
        <tr>
          <th>Symbol</th>
          <th>Price</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        <tr *ngFor="let symbol of getStockKeys()">
          <td>{{ symbol }}</td>
          <td>{{ stocks()[symbol] | currency }}</td>
          <td><button (click)="unsubscribeFromStock(symbol)">Unsubscribe</button></td>
        </tr>
      </tbody>
    </table>
  `
})


export class StockTickerComponent {
  stockSymbol = '';
  stocks: WritableSignal<{ [symbol: string]: number }> = signal({}); // ✅ Explicitly declared

  constructor(private http: HttpClient) {}

  getStockKeys(): string[] {
    return Object.keys(this.stocks());
  }

  subscribeToStock() {
    if (!this.stockSymbol) return;
    this.http.get<Stock>(`http://localhost:5000/api/stocks/${this.stockSymbol}`,  {
        withCredentials: true // ✅ Forces the browser to handle CORS correctly
      })
      .subscribe((response:Stock) => {
        console.log("Stock data:", response);
        const price = response.price;
        this.stocks.update((s) => ({ ...s, [this.stockSymbol]: response.price }));
        this.stockSymbol = '';
      });
  }

  unsubscribeFromStock(symbol: string) {
    this.http.delete(`http://localhost:5000/api/stocks/${symbol}`)
      .subscribe( ()=> {
        this.stocks.update(s => ({ ...s, [symbol]: 0 })); // ✅ Ensure state is updated properly
      },
      error => console.error("Error fetching stock:", error)
    );
  }

  
  
}
