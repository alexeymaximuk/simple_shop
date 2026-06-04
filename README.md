# Shop Microservices System

## Overview

`Shop` is a microservices-based e-commerce system built with **ASP.NET Core (.NET 8)**.

The system consists of three independent services, one shared library, and two test projects:

| Project               | Type               | Description                                             |
|-----------------------|--------------------|---------------------------------------------------------|
| `Shop.Users`          | Web API            | Authentication, authorization, and user management      |
| `Shop.Products`       | Web API            | Product catalog and product management                  |
| `Shop.Frontend`       | ASP.NET Core MVC   | Server-rendered web UI                                  |
| `Shop.Shared`         | Class Library      | Shared middleware, exceptions, extensions, and settings |
| `Shop.Users.Tests`    | xUnit Test Project | Unit + integration tests for Shop.Users                 |
| `Shop.Products.Tests` | xUnit Test Project | Unit + integration tests for Shop.Products              |

Services communicate over HTTP for user-facing calls. User lifecycle events (deactivate / reactivate / delete) are propagated from `Shop.Users` to `Shop.Products` asynchronously via RabbitMQ using MassTransit.

---

## Technical Stack

| Concern           | Technology                                                   |
|-------------------|--------------------------------------------------------------|
| Framework         | ASP.NET Core 8 Web API / MVC                                 |
| ORM               | Entity Framework Core 8 (Code First)                         |
| Database          | PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)         |
| Authentication    | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Frontend auth     | Cookie Authentication                                        |
| Validation        | FluentValidation 12                                          |
| Password hashing  | `Microsoft.AspNetCore.Identity.PasswordHasher<T>`            |
| Email             | MailKit (SMTP)                                               |
| Session store     | Redis (`StackExchange.Redis`) — JTI-keyed session tracking   |
| Async messaging   | MassTransit 8 + RabbitMQ                                     |
| API documentation | Swagger / Swashbuckle (Development only)                     |
| Containerisation  | Docker + Docker Compose                                      |
| Testing           | xUnit, NSubstitute, WebApplicationFactory, EF Core InMemory  |

---

## Architecture

Both `Shop.Users` and `Shop.Products` follow **Clean Architecture** with four layers:

| Layer              | Responsibility                                                                                                 |
|--------------------|----------------------------------------------------------------------------------------------------------------|
| **Presentation**   | Controllers and message consumers — receive HTTP requests or MQ events, delegate to services, return responses |
| **Application**    | Service interfaces + implementations, DTOs, validators — business logic                                        |
| **Domain**         | Models and settings — pure data structures, no dependencies                                                    |
| **Infrastructure** | DbContext, repositories, email sending, event publishing — EF Core, external I/O                               |

---

## Microservices

### Shop.Users — User Management Service (`port 8080`)

Handles registration, login, email flows, and user administration.

**Endpoints:**

| Method | Route                               | Auth   | Description               |
|--------|-------------------------------------|--------|---------------------------|
| POST   | `/api/users/register`               | Public | Register new user         |
| POST   | `/api/users/login`                  | Public | Login, returns JWT        |
| GET    | `/api/users/confirm-email`          | Public | Confirm email via token   |
| POST   | `/api/users/resend-confirmation`    | Public | Resend confirmation email |
| POST   | `/api/users/reset-password-request` | Public | Request password reset    |
| GET    | `/api/users/reset-password`         | Public | Validate reset token      |
| POST   | `/api/users/reset-password`         | Public | Apply new password        |
| POST   | `/api/users/me/change-email`        | JWT    | Request email change      |
| GET    | `/api/users/confirm-email-change`   | Public | Confirm email change      |
| PATCH  | `/api/users/me/change-name`         | JWT    | Update own username       |
| DELETE | `/api/users/me`                     | JWT    | Delete own account        |
| GET    | `/api/users/{id}`                   | Public | Get user by ID            |
| GET    | `/api/users`                        | Admin  | Get all users             |
| DELETE | `/api/users/{id}`                   | Admin  | Delete user by ID         |
| POST   | `/api/users/{id}/deactivate`        | Admin  | Deactivate user           |
| POST   | `/api/users/{id}/activate`          | Admin  | Activate user             |

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

| Method | Route                                  | Auth   | Description                   |
|--------|----------------------------------------|--------|-------------------------------|
| POST   | `/api/products`                        | JWT    | Create product                |
| PUT    | `/api/products/{productId}`            | JWT    | Edit own product              |
| POST   | `/api/products/{productId}/deactivate` | JWT    | Hide own product              |
| POST   | `/api/products/{productId}/activate`   | JWT    | Show own product              |
| GET    | `/api/products/{productId}`            | Public | Get product by ID             |
| GET    | `/api/products/all`                    | Public | Get all products (filterable) |
| GET    | `/api/products/my-products`            | JWT    | Get caller's own products     |

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

| Parameter  | Type    | Description                    |
|------------|---------|--------------------------------|
| `name`     | string  | Case-insensitive partial match |
| `minPrice` | decimal | Minimum price                  |
| `maxPrice` | decimal | Maximum price                  |

**Business rules:**
- Only authenticated users can create products
- Users can only edit/activate/deactivate their own products
- Ownership is enforced at the service layer (`ForbiddenException` on mismatch)
- Public listing shows only active, non-deleted products
- Products from deactivated users are soft-deleted (hidden) until the user is reactivated
- User lifecycle operations (deactivate / reactivate / delete) are received as MassTransit events consumed by `UserDeactivatedConsumer`, `UserActivatedConsumer`, `UserDeletedConsumer`

---

## Async Messaging (RabbitMQ / MassTransit)

User lifecycle operations are decoupled from the Products service via RabbitMQ. MassTransit is used as the messaging abstraction.

### Event contracts (`Shop.Shared/Messages/`)

| Event                  | Published when           | Consumed by                                          |
|------------------------|--------------------------|------------------------------------------------------|
| `UserDeactivatedEvent` | Admin deactivates a user | `UserDeactivatedConsumer` → `SoftDeleteUserProducts` |
| `UserActivatedEvent`   | Admin reactivates a user | `UserActivatedConsumer` → `RestoreUserProducts`      |
| `UserDeletedEvent`     | User account is deleted  | `UserDeletedConsumer` → `DeleteAllProductsForUser`   |

### Transport configuration

| Environment                  | Transport                                                                           |
|------------------------------|-------------------------------------------------------------------------------------|
| `Development` / `Production` | RabbitMQ — configured via `RabbitMq:Host`, `RabbitMq:Username`, `RabbitMq:Password` |
| `Testing`                    | MassTransit InMemory — no broker required, consumers still registered               |

---

### Shop.Frontend — MVC Web Application (`port 8082`)

Server-rendered UI built with ASP.NET Core MVC and Razor views.

Backend URLs are read from configuration (`ApiSettings:UsersAPI`, `ApiSettings:ProductsAPI`).

---

## Shop.Shared — Shared Library

Contains shared middleware, exception types, Redis session service, JWT/Redis settings, and MassTransit event contracts, used by all services

**Error response format:**

```json
{ "error": "Human-readable message" }
```

---

## Testing

Both `Shop.Users` and `Shop.Products` have dedicated test projects with **unit tests** and **integration tests**.

### Test stack

| Tool                                     | Role                                                       |
|------------------------------------------|------------------------------------------------------------|
| xUnit                                    | Test framework                                             |
| NSubstitute                              | Mocking library for unit tests                             |
| `Microsoft.AspNetCore.Mvc.Testing`       | In-process integration test host (`WebApplicationFactory`) |
| `Microsoft.EntityFrameworkCore.InMemory` | In-memory database for integration tests                   |

---

### Shop.Users.Tests — 101 tests

#### Unit tests (`Services/`)

| File                              | Service under test         |
|-----------------------------------|----------------------------|
| `RegistrationServiceTest.cs`      | `RegistrationService`      |
| `LoginServiceTests.cs`            | `LoginService`             |
| `PasswordServiceTest.cs`          | `PasswordService`          |
| `EmailVerificationServiceTest.cs` | `EmailVerificationService` |
| `UserServiceTests.cs`             | `UserService`              |

#### Integration tests (`Integration/`)

Tests use `UsersApiFactory` — a `WebApplicationFactory<Program>` with:
- EF Core InMemory database (unique per test class)
- `IEmailSendingService` replaced with an NSubstitute mock
- `IUserEventPublisher` replaced with an NSubstitute mock
- `IRedisSessionService` replaced with an NSubstitute mock
- Environment set to `Testing` (loads `appsettings.Testing.json`)

| File                                  | Controller                    |
|---------------------------------------|-------------------------------|
| `AuthControllerTests.cs`              | `AuthController`              |
| `EmailConfirmationControllerTests.cs` | `EmailConfirmationController` |
| `UserManagementControllerTests.cs`    | `UserManagementController`    |

---

### Shop.Products.Tests — 43 tests

#### Unit tests (`Services/`)

| File                            | Service under test        |
|---------------------------------|---------------------------|
| `ProductCommandServiceTest.cs`  | `ProductCommandService`   |
| `ProductQueryServiceTest.cs`    | `ProductQueryService`     |
| `ProductExternalServiceTest.cs` | `ProductExternalServices` |

#### Integration tests (`Integration/`)

Tests use `ProductsApiFactory` — a `WebApplicationFactory<Program>` with:
- EF Core InMemory database (unique per test class)
- MassTransit configured with InMemory transport (no RabbitMQ broker needed)
- Environment set to `Testing` (loads `appsettings.Testing.json`)

`TestJwt.cs` generates signed JWT tokens from the same `JwtSettings` that the app uses, ensuring tokens are accepted by the real authentication middleware.

| File                         | Controller           |
|------------------------------|----------------------|
| `ProductsControllerTests.cs` | `ProductsController` |

---

### Running tests

```bash
# Run all tests
dotnet test

# Run a single project
dotnet test Shop.Users.Tests
dotnet test Shop.Products.Tests
```

Tests do *not* require a running database, SMTP server, RabbitMQ broker, or any other external dependency — everything is mocked or replaced with in-memory equivalents.

---


## Authentication & Security

- **JWT Bearer tokens** — issued by `Shop.Users`, validated by both `Shop.Users` and `Shop.Products`
- **Cookie auth** — used by `Shop.Frontend` for session management
- **JWT stored in session** — `Shop.Frontend` stores the raw JWT in server-side session and attaches it to outgoing API requests
- **Redis session validation** — on login a `jti` (JWT ID) claim is embedded in the token and stored in Redis with a configurable TTL. `SessionValidationMiddleware` checks the JTI on every authenticated request and refreshes the TTL on success. Removing the Redis key immediately invalidates the session regardless of token expiry.
- **Automatic sign-out** — `UnauthorizedHandler` on the Frontend's HTTP clients intercepts 401 responses, clears session, signs out the cookie, and redirects to `/login`
- **Async events** — user lifecycle changes (deactivate / reactivate / delete) travel via RabbitMQ; no direct HTTP call from Users to Products
- **Role-based authorization** — `Admin` role required for user management admin endpoints
- All tokens (email confirmation, email change, password reset) are generated using `RandomNumberGenerator` (cryptographically secure)

---

## Email Flows

All emails are sent via SMTP using MailKit. In development, [Mailpit](https://github.com/axllent/mailpit) is used as a local mail catcher.

| Flow                          | Trigger                        | Token expiry |
|-------------------------------|--------------------------------|--------------|
| Email confirmation            | Registration or resend request | 24 hours     |
| Email change confirmation     | Email change request           | 24 hours     |
| Password reset                | Reset password request         | 5 hours      |
| Password changed notification | Successful password reset      | —            |

---

## Running with Docker Compose

```bash
# Copy and fill in environment variables
cp .env.example .env

docker compose up --build
```

**Services started:**

| Service                    | Port        |
|----------------------------|-------------|
| `shop.frontend`            | 8082        |
| `shop.users`               | 8080        |
| `shop.products`            | 8081        |
| `postgres`                 | 5432        |
| `redis`                    | 6379        |
| `rabbitmq` (AMQP)          | 5672        |
| `rabbitmq` (Management UI) | 15672       |
| `mailpit` (UI)             | 8025        |
| `mailpit` (SMTP)           | 1026 → 1025 |


Migrations are applied automatically on startup via `db.Database.MigrateAsync()` (skipped when `ASPNETCORE_ENVIRONMENT=Testing`).
The admin account is seeded on every startup (only created if it does not yet exist).

---

## Running Locally (without Docker)

To run without Docker, start Postgres, RabbitMQ, Redis, and Mailpit locally and configure appsettings.Development.json accordingly. Swagger is available at /swagger in Development.

