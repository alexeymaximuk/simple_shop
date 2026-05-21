# Shop Microservices System

## Overview

`Shop` is a microservices-based e-commerce system built with **ASP.NET Core (.NET 8)**.

The system consists of three independent services, one shared library, and two test projects:

| Project | Type | Description |
|---|---|---|
| `Shop.Users` | Web API | Authentication, authorization, and user management |
| `Shop.Products` | Web API | Product catalog and product management |
| `Shop.Frontend` | ASP.NET Core MVC | Server-rendered web UI |
| `Shop.Shared` | Class Library | Shared middleware, exceptions, extensions, and settings |
| `Shop.Users.Tests` | xUnit Test Project | Unit + integration tests for Shop.Users |
| `Shop.Products.Tests` | xUnit Test Project | Unit + integration tests for Shop.Products |

Services communicate over HTTP for user-facing calls. User lifecycle events (deactivate / reactivate / delete) are propagated from `Shop.Users` to `Shop.Products` asynchronously via RabbitMQ using MassTransit.

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
| Async messaging | MassTransit 8 + RabbitMQ |
| API documentation | Swagger / Swashbuckle (Development only) |
| Containerisation | Docker + Docker Compose |
| Testing | xUnit, NSubstitute, WebApplicationFactory, EF Core InMemory |

---

## Project Structure

```text
Shop/
├── Shop.Users/                         # User management service
│   ├── Application/
│   │   ├── DTOs/
│   │   │   ├── Auth/                   # RegisterUserDto, LoginUserDto, ResetPasswordDto, etc.
│   │   │   └── Users/                  # UpdateUsernameDto, UserResponseDto
│   │   ├── Interfaces/                 # IUserRepository, ILoginService, IRegistrationService,
│   │   │                               # IPasswordService, IEmailVerificationService,
│   │   │                               # IUserService, IUserEventPublisher, IEmailSendingService
│   │   ├── Services/                   # LoginService, RegistrationService, PasswordService,
│   │   │                               # EmailVerificationService, UserService
│   │   └── Validators/                 # FluentValidation validators for all DTOs
│   ├── Domain/
│   │   ├── Models/User.cs
│   │   └── Settings/                   # AppSettings, EmailSettings
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── UsersDbContext.cs
│   │   │   ├── UserRepository.cs
│   │   │   ├── Seed/AdminSeeder.cs
│   │   │   └── Migrations/
│   │   ├── Email/
│   │   │   ├── EmailSendingService.cs
│   │   │   └── Constants/EmailTemplates.cs
│   │   └── Publishers/
│   │       └── UserEventPublisher.cs   # Publishes user lifecycle events via MassTransit
│   ├── Presentation/
│   │   └── Controllers/
│   │       ├── AuthController.cs           # Register, Login, ResetPassword flows
│   │       ├── EmailConfirmationController.cs  # Email confirm + change flows
│   │       └── UserManagementController.cs # CRUD, activate/deactivate (admin)
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── appsettings.Testing.json
│   └── Program.cs
│
├── Shop.Products/                      # Product management service
│   ├── Application/
│   │   ├── DTOs/                       # ProductInfoDto, ProductResponseDto, ProductFilterDto
│   │   ├── Interfaces/                 # IProductRepository, IProductCommandService,
│   │   │                               # IProductQueryService, IProductExternalServices
│   │   ├── Services/                   # ProductCommandService, ProductQueryService,
│   │   │                               # ProductExternalServices
│   │   └── Validators/                 # ProductInfoDtoValidator, ProductFilterDtoValidator
│   ├── Domain/
│   │   └── Models/Product.cs
│   ├── Infrastructure/
│   │   └── Data/
│   │       ├── ProductsDbContext.cs
│   │       ├── ProductRepository.cs
│   │       └── Migrations/
│   ├── Presentation/
│   │   ├── Controllers/
│   │   │   └── ProductsController.cs       # Public + authenticated product endpoints
│   │   └── Consumers/
│   │       ├── UserDeactivatedConsumer.cs  # Soft-deletes user's products
│   │       ├── UserActivatedConsumer.cs    # Restores user's products
│   │       └── UserDeletedConsumer.cs      # Hard-deletes user's products
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── appsettings.Testing.json
│   └── Program.cs
│
├── Shop.Frontend/                      # MVC web application
│   ├── Controllers/
│   │   ├── Auth/                       # LoginController, RegisterController, AccountController,
│   │   │                               # EmailController, PasswordController
│   │   ├── ProductsController.cs
│   │   ├── ProfileController.cs
│   │   └── HomeController.cs
│   ├── Models/                         # View models for all pages
│   ├── Views/                          # Razor views
│   └── Constants/ApiClients.cs
│
├── Shop.Shared/                        # Shared class library
│   ├── Constants/                      # AuthConstants
│   ├── Controllers/                    # ApiBaseController, BaseController
│   ├── Exceptions/                     # AppException hierarchy
│   ├── Extensions/ConfiguratorExtensions.cs
│   ├── Helpers/TokenGenerator.cs
│   ├── Messages/                       # MassTransit event contracts
│   │   ├── UserDeactivatedEvent.cs
│   │   ├── UserActivatedEvent.cs
│   │   └── UserDeletedEvent.cs
│   ├── Middleware/ExceptionMiddleware.cs
│   └── Settings/JwtSettings.cs
│
├── Shop.Users.Tests/                   # Tests for Shop.Users
│   ├── Integration/
│   │   ├── UsersApiFactory.cs
│   │   ├── AuthControllerTests.cs
│   │   ├── EmailConfirmationControllerTests.cs
│   │   └── UserManagementControllerTests.cs
│   └── Services/
│       ├── RegistrationServiceTest.cs
│       ├── LoginServiceTests.cs
│       ├── PasswordServiceTest.cs
│       ├── EmailVerificationServiceTest.cs
│       └── UserServiceTests.cs
│
├── Shop.Products.Tests/                # Tests for Shop.Products
│   ├── Integration/
│   │   ├── ProductsApiFactory.cs
│   │   ├── TestJwt.cs
│   │   └── ProductsControllerTests.cs
│   └── Services/
│       ├── ProductCommandServiceTest.cs
│       ├── ProductQueryServiceTest.cs
│       └── ProductExternalServiceTest.cs
│
├── compose.yaml
└── global.json
```

---

## Architecture

Both `Shop.Users` and `Shop.Products` follow **Clean Architecture** with four layers:

```
Presentation  →  Application  →  Domain
                     ↓
               Infrastructure
```

| Layer | Responsibility |
|---|---|
| **Presentation** | Controllers and message consumers — receive HTTP requests or MQ events, delegate to services, return responses |
| **Application** | Service interfaces + implementations, DTOs, validators — business logic |
| **Domain** | Models and settings — pure data structures, no dependencies |
| **Infrastructure** | DbContext, repositories, email sending, event publishing — EF Core, external I/O |

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
- When a user is deactivated, a `UserDeactivatedEvent` is published — Products service soft-deletes their products
- When a user is reactivated, a `UserActivatedEvent` is published — Products service restores their products
- When a user account is permanently deleted, a `UserDeletedEvent` is published — Products service hard-deletes their products
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
- User lifecycle operations (deactivate / reactivate / delete) are received as MassTransit events consumed by `UserDeactivatedConsumer`, `UserActivatedConsumer`, `UserDeletedConsumer`

**Validation (`ProductInfoDto`):**
- `Name`: 2–200 characters, required
- `Description`: 20–2000 characters, required
- `Price`: > 0, ≤ 1,000,000

---

## Async Messaging (RabbitMQ / MassTransit)

User lifecycle operations are decoupled from the Products service via RabbitMQ. MassTransit is used as the messaging abstraction.

### Event contracts (`Shop.Shared/Messages/`)

| Event | Published when | Consumed by |
|---|---|---|
| `UserDeactivatedEvent` | Admin deactivates a user | `UserDeactivatedConsumer` → `SoftDeleteUserProducts` |
| `UserActivatedEvent` | Admin reactivates a user | `UserActivatedConsumer` → `RestoreUserProducts` |
| `UserDeletedEvent` | User account is deleted | `UserDeletedConsumer` → `DeleteAllProductsForUser` |

### Publisher (`Shop.Users`)

`IUserEventPublisher` (defined in `Application/Interfaces/`) is the domain-facing interface. Its implementation, `UserEventPublisher` (`Infrastructure/Publishers/`), uses MassTransit's `IPublishEndpoint` to dispatch events to RabbitMQ. `UserService` depends only on the interface.

### Consumers (`Shop.Products`)

Three consumers in `Presentation/Consumers/` implement `IConsumer<TEvent>`. Each receives a message and calls the corresponding `IProductExternalServices` method.

### Transport configuration

| Environment | Transport |
|---|---|
| `Development` / `Production` | RabbitMQ — configured via `RabbitMq:Host`, `RabbitMq:Username`, `RabbitMq:Password` |
| `Testing` | MassTransit InMemory — no broker required, consumers still registered |

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

Shared code referenced by all services:

| File | Purpose |
|---|---|
| `Middleware/ExceptionMiddleware.cs` | Global exception handler — maps `AppException` subclasses to HTTP status codes, logs unhandled exceptions |
| `Exceptions/AppException.cs` | Base exception with `StatusCode` property |
| `Exceptions/NotFoundException.cs` | → 404 |
| `Exceptions/AuthorisationException.cs` | → 401 |
| `Exceptions/ForbiddenException.cs` | → 403 |
| `Exceptions/InvalidRequestException.cs` | → 422 |
| `Exceptions/TokenExpiredException.cs` | → 400 |
| `Exceptions/DuplicateMailException.cs` | → 409 |
| `Exceptions/AccountDeactivatedException.cs` | → 403 |
| `Extensions/ConfiguratorExtensions.cs` | `GetRequiredSettings<T>()` — throws on missing config section |
| `Helpers/TokenGenerator.cs` | Cryptographically secure token generation via `RandomNumberGenerator` |
| `Settings/JwtSettings.cs` | Shared JWT configuration model |
| `Constants/AuthConstants.cs` | Token expiry durations |
| `Controllers/ApiBaseController.cs` | `GetCurrentUserId()` helper for authenticated API controllers |
| `Controllers/BaseController.cs` | `AddApiErrors()` helper for parsing API error responses into `ModelState` |
| `Messages/UserDeactivatedEvent.cs` | MassTransit event contract — user deactivated |
| `Messages/UserActivatedEvent.cs` | MassTransit event contract — user reactivated |
| `Messages/UserDeletedEvent.cs` | MassTransit event contract — user deleted |

**Error response format:**

```json
{ "error": "Human-readable message" }
```

---

## Testing

Both `Shop.Users` and `Shop.Products` have dedicated test projects with **unit tests** and **integration tests**.

### Test stack

| Tool | Role |
|---|---|
| xUnit | Test framework |
| NSubstitute | Mocking library for unit tests |
| `Microsoft.AspNetCore.Mvc.Testing` | In-process integration test host (`WebApplicationFactory`) |
| `Microsoft.EntityFrameworkCore.InMemory` | In-memory database for integration tests |

---

### Shop.Users.Tests — 101 tests

#### Unit tests (`Services/`)

| File | Service under test | Cases covered |
|---|---|---|
| `RegistrationServiceTest.cs` | `RegistrationService` | Duplicate email, adds user to repo, hashes password, sends confirmation email |
| `LoginServiceTests.cs` | `LoginService` | User not found, inactive account, unconfirmed email, wrong password, valid login returns token |
| `PasswordServiceTest.cs` | `PasswordService` | Request not found / deactivated / valid; validate token expired / null / valid; change password hashes + notifies + clears token |
| `EmailVerificationServiceTest.cs` | `EmailVerificationService` | Confirm email (not found, expired, valid); resend (not found, already confirmed, valid); change email request (not found, valid); confirm change (not found, no pending email, expired, valid) |
| `UserServiceTests.cs` | `UserService` | Update name, delete self (null id, valid), delete by id (not found, valid), deactivate (not found, valid), activate (not found, valid), get by id (null / found), get all (empty / populated) |

#### Integration tests (`Integration/`)

Tests use `UsersApiFactory` — a `WebApplicationFactory<Program>` with:
- EF Core InMemory database (unique per test class)
- `IEmailSendingService` replaced with an NSubstitute mock (no real SMTP)
- `IUserEventPublisher` replaced with an NSubstitute mock (no real message publishing)
- Environment set to `Testing` (loads `appsettings.Testing.json`)

| File | Controller | Scenarios |
|---|---|---|
| `AuthControllerTests.cs` | `AuthController` | Register (valid, invalid email, duplicate); Login (valid, unconfirmed, inactive, wrong password, not found); ResetPasswordRequest (valid, not found, invalid email); ValidateResetToken (valid, invalid, expired); ResetPassword (valid, invalid token, expired) |
| `EmailConfirmationControllerTests.cs` | `EmailConfirmationController` | ConfirmEmail (valid, invalid, expired); ResendConfirmation (valid, not registered, already confirmed); ChangeEmail (valid, unauthenticated, invalid format); ConfirmEmailChange (valid, invalid token, expired) |
| `UserManagementControllerTests.cs` | `UserManagementController` | DeleteCurrentUser (authenticated, unauthenticated); DeleteUser (admin, regular user, unauthenticated, not found); DeactivateUser (admin, regular user, unauthenticated, not found); ActivateUser (admin, regular user, unauthenticated, not found); ChangeName (valid, unauthenticated, too short); GetById (found, not found); GetAll (admin, regular user, unauthenticated) |

---

### Shop.Products.Tests — 43 tests

#### Unit tests (`Services/`)

| File | Service under test | Cases covered |
|---|---|---|
| `ProductCommandServiceTest.cs` | `ProductCommandService` | CreateProduct (valid); EditProduct (not found, wrong user, valid); ActivateProduct (not found, wrong user, already active, valid); DeactivateProduct (not found, wrong user, already deactivated, valid) |
| `ProductQueryServiceTest.cs` | `ProductQueryService` | GetProductById (not found, found + mapping verified); GetAllProducts (empty, multiple results mapped, filter + showUnavailable forwarded to repository) |
| `ProductExternalServiceTest.cs` | `ProductExternalServices` | SoftDeleteUserProducts, RestoreUserProducts, DeleteAllProductsForUser — each verifies correct repository method is called |

#### Integration tests (`Integration/`)

Tests use `ProductsApiFactory` — a `WebApplicationFactory<Program>` with:
- EF Core InMemory database (unique per test class)
- MassTransit configured with InMemory transport (no RabbitMQ broker needed)
- Environment set to `Testing` (loads `appsettings.Testing.json`)

`TestJwt.cs` generates signed JWT tokens from the same `JwtSettings` that the app uses, ensuring tokens are accepted by the real authentication middleware.

| File | Controller | Scenarios |
|---|---|---|
| `ProductsControllerTests.cs` | `ProductsController` | AddNewProduct (valid 201, unauthenticated, invalid data); EditProduct (valid, unauthenticated, wrong user, not found, invalid data); Deactivate (valid, unauthenticated, wrong user, already deactivated, not found); Activate (valid, unauthenticated, wrong user, already active, not found); GetProduct (found, not found); GetAllProducts; GetMyProducts (authenticated, unauthenticated) |

---

### Running tests

```bash
# Run all tests
dotnet test

# Run a single project
dotnet test Shop.Users.Tests
dotnet test Shop.Products.Tests
```

Tests do **not** require a running database, SMTP server, RabbitMQ broker, or any other external dependency — everything is mocked or replaced with in-memory equivalents.

---


## Authentication & Security

- **JWT Bearer tokens** — issued by `Shop.Users`, validated by both `Shop.Users` and `Shop.Products`
- **Cookie auth** — used by `Shop.Frontend` for session management
- **JWT stored in session** — `Shop.Frontend` stores the raw JWT in server-side session and attaches it to outgoing API requests
- **Async events** — user lifecycle changes (deactivate / reactivate / delete) travel via RabbitMQ; no direct HTTP call from Users to Products
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
| `rabbitmq` (AMQP) | 5672 |
| `rabbitmq` (Management UI) | 15672 |
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

RABBITMQ_USER=
RABBITMQ_PASSWORD=

ADMIN_EMAIL=
ADMIN_PASSWORD=
ADMIN_ROLE=Admin
ADMIN_NAME=
```

Migrations are applied automatically on startup via `db.Database.Migrate()` (skipped when `ASPNETCORE_ENVIRONMENT=Testing`).
The admin account is seeded on every startup (only created if it does not yet exist).

---

## Running Locally (without Docker)

1. Start a PostgreSQL instance locally
2. Start a RabbitMQ instance locally (default `guest`/`guest` on `localhost:5672`, or configure `RabbitMq:Host` / `RabbitMq:Username` / `RabbitMq:Password`)
3. Configure `appsettings.Development.json` in `Shop.Users` and `Shop.Products` with connection strings and JWT settings (or use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets))
4. Start Mailpit (or any SMTP server) and point `Email:Host` / `Email:Port` at it
5. Run all three services

Swagger UI is available in Development at `/swagger`.

