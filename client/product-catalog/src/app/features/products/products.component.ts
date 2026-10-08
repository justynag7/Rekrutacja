import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, AsyncPipe } from '@angular/common';
import { ProductsFacade } from './domain/products.facade';
import { ProductFormComponent } from './ui/product-form/product-form.component';
import { ProductListComponent } from './ui/product-list/product-list.component';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, AsyncPipe, ProductFormComponent, ProductListComponent],
  providers: [ProductsFacade],
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  readonly facade = inject(ProductsFacade);
  readonly auth = inject(AuthService);

  ngOnInit(): void {
    this.facade.load();
  }

  logout(): void {
    this.auth.logout();
    location.reload();
  }
}
