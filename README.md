# Shop Microservices System

## Overview

`Shop` is a microservices-based e-commerce system built with **ASP.NET Core (.NET 8)**.

The system consists of three independent services and one shared library:

| Project | Type | Description |
|---|---|---|
| `Shop.Users` | Web API | Authentication, authorization, and user management |
| `Shop.Products` | Web API | Product catalog and product management |
| `Shop.Frontend` | ASP.NET Core MVC | Server-rendered web UI |
| `Shop.Shared` | Class Library | Shared middleware, exceptions, extensions, and settings |

Services communicate over HTTP. Service-to-service calls (Users → Products) are secured with an internal API key.

---

## Project Structure

```text
Shop/
├── Shop.Users/
│   ├── Controllers/
│   │   ├── UserController.cs
│   │   ├── UserController.Auth.cs
│   │   ├── UserController.EmailConfirmation.cs
│   │   └── UserController.Management.cs
│   ├── Data/
│   │   ├── UsersDbContext.cs
│   │   └── Seed/AdminSeeder.cs
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Users/
│   │   └── Validators/
│   ├── Migrations/
│   ├── Models/User.cs
│   ├── Services/
│   │   ├── AuthService.cs (+ partial files)
│   │   ├── UserService.cs (+ partial files)
│   │   ├── EmailService.cs (+ partial files)
│   │   ├── ProductServiceClient.cs
│   │   └── Interfaces/
│   └── Settings/
│
├── Shop.Products/
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   └── ProductsController.HelperMethods.cs
│   ├── Data/ProductsDbContext.cs
│   ├── DTOs/
│   │   └── Validators/
│   ├── Migrations/
│   ├── Models/Product.cs
│   └── Services/
│       ├── ProductService.cs
│       ├── ProductService.Commands.cs
│       ├── ProductService.Queries.cs
│       ├── ProductService.HelperMethods.cs
│       └── Interfaces/
│
├── Shop.Frontend/
│   ├── Controllers/
│   │   ├── Auth/
│   │   │   ├── LoginController.cs
│   │   │   ├── RegisterController.cs
│   │   │   ├── AccountController.cs
│   │   │   ├── EmailController.cs
│   │   │   └── PasswordController.cs
│   │   ├── ProductsController.cs
│   │   ├── ProfileController.cs
│   │   └── HomeController.cs
│   ├── Models/
│   └── Views/
│
├── Shop.Shared/
│   ├── Controllers/BaseController.cs
│   ├── Exceptions/
│   ├── Extensions/ConfiguratorExtensions.cs
│   ├── Middleware/ExceptionMiddleware.cs
│   └── Settings/JwtSettings.cs
│
├── compose.yaml
└── global.json
```

---

## Architecture

Services are organized with **partial classes split by responsibility** (Commands, Queries, HelperMethods). Each service is self-contained with its own database.

```
Controller → Service Interface → Service (partial classes) → DbContext
```

Shared concerns (exception types, middleware, settings, base controller) live in `Shop.Shared`.

---

## Microservices

### Shop.Users — User Management Service (`port 8080`)

Handles registration, login, email flows, and user administration.

**Endpoints:**

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/users/register` | Public | Register new user |
| POST | `/api/users/login` | Public | Login, returns JWT |
| GET | `/api/users/confirm-email` | Public | Confirm email via token |
| POST | `/api/users/resend-confirmation` | Public | Resend confirmation email |
| POST | `/api/users/reset-password-request` | Public | Request password reset |
| GET | `/api/users/reset-password` | Public | Validate reset token |
| POST | `/api/users/reset-password` | Public | Apply new password |
| POST | `/api/users/me/change-email` | JWT | Request email change |
| GET | `/api/users/confirm-email-change` | Public | Confirm email change |
| PATCH | `/api/users/me/change-name` | JWT | Update own username |
| DELETE | `/api/users/me` | JWT | Delete own account |
| GET | `/api/users/{id}` | Public | Get user by ID |
| GET | `/api/users` | Admin | Get all users |
| DELETE | `/api/users/{id}` | Admin | Delete user by ID |
| POST | `/api/users/{id}/deactivate` | Admin | Deactivate user |
| POST | `/api/users/{id}/activate` | Admin | Activate user |

**User model:**

```json
{
  "id": "guid",
  "name": "string",
  "email": "string",
  "role": "User | Admin",
  "isActive": true,
  "isEmailConfirmed": true,
  "createdAt": "datetime"
}
```

**Business rules:**
- Passwords are hashed using `Microsoft.AspNetCore.Identity.PasswordHasher`
- Users must confirm their email before they can log in
- Deactivated users cannot log in
- When a user is deactivated, all their products are soft-deleted (hidden)
- When a user is reactivated, their products are restored
- When a user account is permanently deleted, all their products are hard-deleted
- An Admin account is seeded at startup from environment configuration
- Roles: `User` (default), `Admin`

---

### Shop.Products — Product Management Service (`port 8081`)

Handles product creation, editing, filtering, and visibility management.

**Endpoints:**

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/products` | JWT | Create product |
| PUT | `/api/products/{productId}` | JWT | Edit own product |
| POST | `/api/products/{productId}/deactivate` | JWT | Hide own product |
| POST | `/api/products/{productId}/activate` | JWT | Show own product |
| GET | `/api/products/{productId}` | Public | Get product by ID |
| GET | `/api/products/all` | Public | Get all products (filterable) |
| GET | `/api/products/my-products` | JWT | Get caller's own products |
| POST | `/api/products/users/{id}/deactivate` | Internal API Key | Soft-delete user's products |
| POST | `/api/products/users/{id}/reactivate` | Internal API Key | Restore user's products |
| DELETE | `/api/products/users/{id}/delete` | Internal API Key | Hard-delete user's products |

**Product model:**

```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "price": 0.00,
  "isActive": true,
  "isDeleted": false,
  "userId": "guid",
  "createTime": "datetime"
}
```

**Filter parameters (`GET /api/products/all`):**

| Parameter | Type | Description |
|---|---|---|
| `name` | string | Case-insensitive partial match |
| `minPrice` | decimal | Minimum price |
| `maxPrice` | decimal | Maximum price |

**Business rules:**
- Only authenticated users can create products
- Users can only edit/activate/deactivate their own products
- Ownership is enforced at the service layer (`ForbiddenException` on mismatch)
- Public listing shows only active, non-deleted products
- Products from deactivated users are soft-deleted (hidden) until the user is reactivated

**Validation (`ProductInfoDto`):**
- `Name`: 2–200 characters, required
- `Description`: 20–2000 characters, required
- `Price`: > 0, ≤ 1,000,000

---

### Shop.Frontend — MVC Web Application (`port 8082`)

Server-rendered UI built with ASP.NET Core MVC and Razor views.

**Authentication flow:**
1. User logs in via `POST /api/users/login` on the Users service
2. The returned JWT is stored in the server-side session (`JwtToken` key)
3. JWT claims are parsed and signed into a **Cookie** authentication principal
4. Subsequent API calls from the frontend attach the stored JWT as a `Bearer` token

Backend URLs are read from configuration (`ApiSettings:UsersAPI`, `ApiSettings:ProductsAPI`).

---

## Shop.Shared — Shared Library

Shared code referenced by all three services:

| File | Purpose |
|---|---|
| `Middleware/ExceptionMiddleware.cs` | Global exception handler — maps `AppException` subclasses to HTTP status codes, logs unhandled exceptions |
| `Exceptions/AppException.cs` | Base exception with `StatusCode` property |
| `Exceptions/NotFoundException.cs` | → 404 |
| `Exceptions/AuthorisationException.cs` | → 401 |
| `Exceptions/ForbiddenException.cs` | → 403 |
| `Exceptions/InvalidRequestException.cs` | → 400 |
| `Exceptions/TokenExpiredException.cs` | → 400 |
| `Exceptions/DuplicateMailException.cs` | → 409 |
| `Exceptions/AccountDeactivatedException.cs` | → 403 |
| `Extensions/ConfiguratorExtensions.cs` | `GetRequiredSettings<T>()` — throws on missing config section |
| `Settings/JwtSettings.cs` | Shared JWT configuration model |
| `Constants/AuthConstants.cs` | Token expiry durations, confirmation/reset URL routes |
| `Constants/ProductsServiceRoutes.cs` | Route templates for Products internal endpoints |
| `Controllers/BaseController.cs` | `AddApiErrors()` helper for parsing API error responses into `ModelState` |

**Error response format:**

```json
{ "error": "Human-readable message" }
```

---

## Technical Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API / MVC |
| ORM | Entity Framework Core 8 (Code First) |
| Database | PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`) |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Frontend auth | Cookie Authentication |
| Validation | FluentValidation 12 |
| Password hashing | `Microsoft.AspNetCore.Identity.PasswordHasher<T>` |
| Email | MailKit (SMTP) |
| API documentation | Swagger / Swashbuckle (Development only) |
| Containerisation | Docker + Docker Compose |

---

## Authentication & Security

- **JWT Bearer tokens** — issued by `Shop.Users`, validated by both `Shop.Users` and `Shop.Products`
- **Cookie auth** — used by `Shop.Frontend` for session management
- **JWT stored in session** — `Shop.Frontend` stores the raw JWT in server-side session and attaches it to outgoing API requests
- **Internal API key** — service-to-service calls (Users → Products) use an `X-Internal-Key` header validated by `[InternalApiKey]` attribute
- **Role-based authorization** — `Admin` role required for user management admin endpoints
- All tokens (email confirmation, email change, password reset) are generated using `RandomNumberGenerator` (cryptographically secure)

---

## Email Flows

All emails are sent via SMTP using MailKit. In development, [Mailpit](https://github.com/axllent/mailpit) is used as a local mail catcher.

| Flow | Trigger | Token expiry |
|---|---|---|
| Email confirmation | Registration or resend request | 24 hours |
| Email change confirmation | Email change request | 24 hours |
| Password reset | Reset password request | 5 hours |
| Password changed notification | Successful password reset | — |

---

## Running with Docker Compose

```bash
# Copy and fill in environment variables
cp .env.example .env

docker compose up --build
```

**Services started:**

| Service | Port |
|---|---|
| `shop.frontend` | 8082 |
| `shop.users` | 8080 |
| `shop.products` | 8081 |
| `postgres` | 5432 |
| `mailpit` (UI) | 8025 |
| `mailpit` (SMTP) | 1026 → 1025 |

**Required `.env` variables:**

```env
POSTGRES_USER=
POSTGRES_PASSWORD=
USERS_DB=
PRODUCTS_DB=

JWT_KEY=
JWT_ISSUER=
JWT_AUDIENCE=
JWT_TOKEN_DURATION=

APP_BASE_URL=
APP_EMAIL_FROM=
PRODUCTS_API_URL=
USERS_API_URL=

INTERNAL_API_KEY=

ADMIN_EMAIL=
ADMIN_PASSWORD=
ADMIN_ROLE=Admin
ADMIN_NAME=
```

Migrations are applied automatically on startup via `db.Database.Migrate()`.
The admin account is seeded on every startup (only created if it does not yet exist).

---

## Running Locally (without Docker)

1. Start a PostgreSQL instance locally
2. Configure `appsettings.Development.json` in `Shop.Users` and `Shop.Products` with connection strings and JWT settings (or use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets))
3. Start Mailpit (or any SMTP server) and point `Email:Host` / `Email:Port` at it
4. Run all three services

Swagger UI is available in Development at `/swagger`.

