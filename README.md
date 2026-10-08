# Product Catalog

Zadanie rekrutacyjne: katalog produktów — REST API + frontend (formularz dodawania + lista).

## Technologie

### Backend
| Technologia | Zastosowanie |
|-------------|--------------|
| **.NET 8** / ASP.NET Core Web API | REST API |
| **C#** | logika aplikacji |
| **JWT Bearer** (`Microsoft.AspNetCore.Authentication.JwtBearer`) | logowanie i ochrona endpointów |
| **Swashbuckle** | Swagger / OpenAPI |
| **xUnit** | testy API |
| Repozytorium **in-memory** | przechowywanie produktów bez bazy danych |

Warstwy: Controller → Service → Domain/DTO → Repository  
Dodatkowo: idempotencja (`Idempotency-Key`), security headers, walidacja DataAnnotations, `RestResponse<T>`.

### Frontend
| Technologia | Zastosowanie |
|-------------|--------------|
| **Angular 18** (standalone components) | UI |
| **TypeScript** | kod aplikacji |
| **RxJS** | strumienie HTTP / `exhaustMap` przy zapisie |
| **Reactive Forms** | formularz logowania i produktu |
| **SCSS** (`src/styles/`) | style globalne (theme, layout, forms, components) |
| **Karma / Jasmine** | testy jednostkowe |
| Angular **dev proxy** | `/api` → backend |

Wymagania środowiskowe:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (np. 18.20.x / 20.x)
- Visual Studio 2022+ lub VS Code

## Jak uruchomić

Uruchom **najpierw API**, potem frontend (w dwóch terminalach).

### 1. Backend

```bash
cd src/ProductCatalog.Api
dotnet run --launch-profile http
```

| Adres | Opis |
|-------|------|
| http://localhost:5080 | API |
| http://localhost:5080/swagger | Swagger UI |

### 2. Frontend

```bash
cd client/product-catalog
npm install
npm start
```

UI: **http://localhost:4200**  
Proxy przekierowuje `/api` na `http://localhost:5080`.

### Logowanie (demo)

| Email | Hasło |
|-------|-------|
| `admin@catalog.local` | `Admin123!` |

## Testy

```bash
# Backend
dotnet test tests/ProductCatalog.Api.Tests

# Frontend
cd client/product-catalog
npx ng test --watch=false --browsers=ChromeHeadless
```

## API (skrót)

| Metoda | Ścieżka | Auth | Opis |
|--------|---------|------|------|
| `POST` | `/api/auth/login` | nie | logowanie → JWT |
| `GET` | `/api/products` | Bearer | lista produktów |
| `POST` | `/api/products` | Bearer + nagłówek `Idempotency-Key` | dodanie produktu |

Model produktu: `id`, `code` (Kod), `name` (Nazwa), `price` (Cena).

Odpowiedzi w envelope `RestResponse<T>` (`isSuccess`, `data`, `errorMessage`).

### Przykład — dodanie produktu

```http
POST /api/products
Authorization: Bearer <token>
Content-Type: application/json
Idempotency-Key: 9f1c2a7b0e4d8c6a1b2e3f4a5c6d7e8f

{
  "code": "AU-2OZ",
  "name": "Sztabka złota 2 oz",
  "price": 19600.00
}
```

## Struktura rozwiązania

```
src/ProductCatalog.Api/     # Web API (.NET 8)
client/product-catalog/     # Angular 18
tests/ProductCatalog.Api.Tests/
```

## Bezpieczeństwo (skrót)

- JWT: issuer/audience/signature/lifetime validation
- hasła hashowane (`PasswordHasher`)
- stały komunikat błędu logowania (brak enumeracji użytkowników)
- `Idempotency-Key` wymagany przy POST produktów (ochrona przed duplikatami / replay UI)
- rate limit: 10 loginów/min/IP, 60 req/min użytkownika
- nagłówki: `X-Content-Type-Options`, `X-Frame-Options`, CSP, HSTS (HTTPS)
- frontend: escape HTML przy renderowaniu listy (XSS)
- token w `sessionStorage` (nie localStorage)

## Uwaga

`Jwt:SigningKey` w `appsettings.json` jest kluczem deweloperskim — w produkcji użyj sekretu ze zmiennych środowiskowych / Key Vault.
