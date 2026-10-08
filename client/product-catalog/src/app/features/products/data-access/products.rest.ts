import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { CreateProductRequest, Product, RestResponse } from './products.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProductsRestService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/products`;

  getProducts(): Observable<Product[]> {
    return this.http.get<RestResponse<Product[]>>(this.baseUrl).pipe(
      map((response) => {
        if (!response.isSuccess || !response.data) {
          throw new Error(response.errorMessage || 'Failed to load products.');
        }
        return response.data;
      })
    );
  }

  createProduct(request: CreateProductRequest, idempotencyKey: string): Observable<Product> {
    const headers = new HttpHeaders({ 'Idempotency-Key': idempotencyKey });
    return this.http.post<RestResponse<Product>>(this.baseUrl, request, { headers }).pipe(
      map((response) => {
        if (!response.isSuccess || !response.data) {
          throw new Error(response.errorMessage || 'Failed to create product.');
        }
        return response.data;
      })
    );
  }
}
