import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, finalize, map, of, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ProductsRestService } from '../data-access/products.rest';
import { CreateProductRequest, Product } from '../data-access/products.model';

export interface ProductsState {
  products: Product[];
  loading: boolean;
  error: string | null;
  saving: boolean;
  saveError: string | null;
  saveSuccess: string | null;
}

const initialState: ProductsState = {
  products: [],
  loading: false,
  error: null,
  saving: false,
  saveError: null,
  saveSuccess: null
};

@Injectable()
export class ProductsFacade {
  private readonly rest = inject(ProductsRestService);
  private readonly stateSubject = new BehaviorSubject<ProductsState>(initialState);

  readonly state$: Observable<ProductsState> = this.stateSubject.asObservable();

  get snapshot(): ProductsState {
    return this.stateSubject.value;
  }

  load(): void {
    this.patch({ loading: true, error: null });

    this.rest.getProducts().subscribe({
      next: (products) => this.patch({ products, loading: false }),
      error: (err: unknown) =>
        this.patch({
          loading: false,
          error: this.readError(err, 'Failed to load products.')
        })
    });
  }

  create(request: CreateProductRequest): Observable<Product | null> {
    if (this.snapshot.saving) {
      return of(null);
    }

    this.patch({ saving: true, saveError: null, saveSuccess: null });
    const key = crypto.randomUUID().replaceAll('-', '');

    return this.rest.createProduct(request, key).pipe(
      tap((product) => {
        const products = [...this.snapshot.products.filter((p) => p.id !== product.id), product].sort(
          (a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' })
        );
        this.patch({
          products,
          saving: false,
          saveSuccess: `Dodano ${product.code}.`
        });
        this.rest.getProducts().subscribe({
          next: (fresh) => this.patch({ products: fresh }),
          error: () => undefined
        });
      }),
      catchError((err: unknown) => {
        this.patch({
          saving: false,
          saveError: this.readError(err, 'Nie udało się dodać produktu.')
        });
        return of(null);
      }),
      finalize(() => {
        if (this.snapshot.saving) {
          this.patch({ saving: false });
        }
      }),
      map((value) => value)
    );
  }

  private patch(partial: Partial<ProductsState>): void {
    this.stateSubject.next({ ...this.stateSubject.value, ...partial });
  }

  private readError(err: unknown, fallback: string): string {
    if (err instanceof Error && err.message) {
      return err.message;
    }
    if (err instanceof HttpErrorResponse) {
      if (err.status === 0) {
        return 'Cannot reach API. Start the backend on http://localhost:5080.';
      }
      return `${fallback} (${err.status}).`;
    }
    return fallback;
  }
}
