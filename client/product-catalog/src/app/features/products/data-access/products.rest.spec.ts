import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ProductsRestService } from './products.rest';

describe('ProductsRestService', () => {
  let service: ProductsRestService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(ProductsRestService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getProducts unwraps RestResponse', () => {
    service.getProducts().subscribe((products) => {
      expect(products[0].code).toBe('A');
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/api/products') && r.method === 'GET');
    req.flush({
      isSuccess: true,
      data: [{ id: '1', code: 'A', name: 'Alpha', price: 10 }],
      errorMessage: null
    });
  });

  it('createProduct sends Idempotency-Key and unwraps RestResponse', () => {
    service.createProduct({ code: 'B', name: 'Beta', price: 2.5 }, 'idempotencykey123').subscribe((p) => {
      expect(p.code).toBe('B');
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/api/products') && r.method === 'POST');
    expect(req.request.headers.get('Idempotency-Key')).toBe('idempotencykey123');
    req.flush({
      isSuccess: true,
      data: { id: '2', code: 'B', name: 'Beta', price: 2.5 },
      errorMessage: null
    });
  });
});
