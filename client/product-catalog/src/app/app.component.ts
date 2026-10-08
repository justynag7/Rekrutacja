import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/auth/auth.service';
import { LoginComponent } from './features/login/login.component';
import { ProductsComponent } from './features/products/products.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, LoginComponent, ProductsComponent],
  template: `
    @if (loggedIn) {
      <app-products />
    } @else {
      <app-login (loggedIn)="onLoggedIn()" />
    }
  `
})
export class AppComponent {
  private readonly auth = inject(AuthService);
  loggedIn = this.auth.isLoggedIn();

  onLoggedIn(): void {
    this.loggedIn = true;
  }
}
