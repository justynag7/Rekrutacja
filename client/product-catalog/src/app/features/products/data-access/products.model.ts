export interface Product {
  id: string;
  code: string;
  name: string;
  price: number;
}

export interface CreateProductRequest {
  code: string;
  name: string;
  price: number;
}

export interface RestResponse<T> {
  data: T | null;
  isSuccess: boolean;
  errorMessage: string | null;
}
