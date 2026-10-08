import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, exhaustMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductsFacade } from '../../domain/products.facade';
import { CreateProductRequest, Product } from '../../data-access/products.model';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-form.component.html'
})
export class ProductFormComponent {
  @Input() saving = false;
  @Input() errorMessage: string | null = null;
  @Input() successMessage: string | null = null;
  @Output() readonly created = new EventEmitter<Product>();

  private readonly fb = inject(FormBuilder);
  private readonly facade = inject(ProductsFacade);
  private readonly submit$ = new Subject<CreateProductRequest>();

  readonly form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(32), Validators.pattern(/^[A-Za-z0-9\-_.]+$/)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    price: [null as number | null, [Validators.required, Validators.min(0.01), Validators.max(1_000_000)]]
  });

  constructor() {
    this.submit$
      .pipe(
        exhaustMap((payload) => this.facade.create(payload)),
        takeUntilDestroyed()
      )
      .subscribe((product) => {
        if (product) {
          this.form.reset();
          this.created.emit(product);
        }
      });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving) {
      return;
    }

    const value = this.form.getRawValue();
    this.submit$.next({
      code: value.code.trim(),
      name: value.name.trim(),
      price: Number(value.price)
    });
  }
}
