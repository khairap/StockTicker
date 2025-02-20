import { bootstrapApplication } from '@angular/platform-browser';
import { StockTickerComponent } from './app/stock-ticker.component';
import { provideHttpClient } from '@angular/common/http';

bootstrapApplication(StockTickerComponent, {
  providers: [provideHttpClient()]
}).catch(err => console.error(err));
