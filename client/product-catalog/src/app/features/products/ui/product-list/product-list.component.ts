import { Component, Input } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { Product } from '../../data-access/products.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, CurrencyPipe],
  templateUrl: './product-list.component.html'
})
export class ProductListComponent {
  @Input({ required: true }) products: Product[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
}
