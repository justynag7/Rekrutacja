import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { LoginRequest, RestResponse, TokenInfo } from './auth.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthRestService {
  private readonly http = inject(HttpClient);

  login(request: LoginRequest): Observable<TokenInfo> {
    return this.http
      .post<RestResponse<TokenInfo>>(`${environment.apiBaseUrl}/api/auth/login`, request)
      .pipe(
        map((response) => {
          if (!response.isSuccess || !response.data) {
            throw new Error(response.errorMessage || 'Login failed.');
          }
          return response.data;
        })
      );
  }
}
