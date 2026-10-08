import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthRestService } from './auth.rest';
import { LoginRequest, TokenInfo } from './auth.model';

const TOKEN_KEY = 'pc_access_token';
const USER_KEY = 'pc_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly rest = inject(AuthRestService);
  private readonly loggedInSubject = new BehaviorSubject<boolean>(!!sessionStorage.getItem(TOKEN_KEY));

  readonly isLoggedIn$ = this.loggedInSubject.asObservable();

  get token(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  get displayName(): string {
    try {
      const raw = sessionStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as { displayName: string }).displayName : '';
    } catch {
      return '';
    }
  }

  get email(): string {
    try {
      const raw = sessionStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as { email: string }).email : '';
    } catch {
      return '';
    }
  }

  isLoggedIn(): boolean {
    return !!this.token;
  }

  login(request: LoginRequest): Observable<TokenInfo> {
    return this.rest.login(request).pipe(tap((token) => this.persist(token)));
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    this.loggedInSubject.next(false);
  }

  private persist(token: TokenInfo): void {
    sessionStorage.setItem(TOKEN_KEY, token.accessToken);
    sessionStorage.setItem(
      USER_KEY,
      JSON.stringify({ email: token.email, displayName: token.displayName })
    );
    this.loggedInSubject.next(true);
  }
}
