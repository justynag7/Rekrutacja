import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ProductFormComponent } from './product-form.component';
import { ProductsFacade } from '../../domain/products.facade';
import { of } from 'rxjs';

describe('ProductFormComponent', () => {
  let fixture: ComponentFixture<ProductFormComponent>;
  let component: ProductFormComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ProductsFacade,
          useValue: {
            create: () => of(null)
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('disables submit when form is invalid', () => {
    expect(component.form.invalid).toBeTrue();
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(button.disabled).toBeTrue();
  });

  it('accepts valid values', () => {
    component.form.setValue({ code: 'AU-2OZ', name: 'Gold', price: 10 });
    expect(component.form.valid).toBeTrue();
  });
});
