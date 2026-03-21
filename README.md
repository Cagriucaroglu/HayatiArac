# HayatiArac

HayatiArac is a backend API for a second-hand vehicle marketplace.

## Project Overview

![HayatiArac](HayatiArac/assets/HayatiArac.png)
 Users can register, verify their identity, and publish vehicle listings. The platform enforces strict publishing rules — one active listing per user, a 30-day cooldown between listings, and automatic expiry — to keep the marketplace fair and up-to-date.

---

## Technologies

| Category | Technology |
|----------|-----------|
| Language | C# / .NET 9 |
| Architecture | Modular Monolith (User module, Advert module, SharedKernel) |
| API | ASP.NET Core Minimal APIs |
| ORM | Entity Framework Core 9 |
| Database | SQL Server (Docker) |
| Cache | Redis (Docker) via StackExchange.Redis |
| Messaging | MassTransit 8 (In-Memory transport) |
| Auth | JWT Bearer + Refresh Tokens |
| Password Hashing | BCrypt.Net |
| Validation | FluentValidation |
| Mediator | MediatR (CQRS) |
| SMS Provider | Netgsm |
| Email Provider | SMTP (e.g. Gmail) |
| OTP Storage | Redis (5-minute TTL) |
| Containerization | Docker / Docker Compose |

---

## Features

### Authentication & Authorization
- JWT access tokens + refresh tokens
- Token blacklist on logout (Redis-backed, JTI-based)
- Account lockout after 5 failed login attempts (15-minute lock)
- Role-based authorization (`User`, `Admin`)

### Registration & Identity Verification
- Users register with email, password, first name, last name, and phone number
- **Email and phone number must each be unique across the platform** — no two accounts can share the same email or phone number
- After registration the account is inactive until both verifications are completed:
  - A 6-digit OTP is sent to the user's **email address** via SMTP
  - A 6-digit OTP is sent to the user's **phone number** via **Netgsm** (SMS)
  - OTPs are stored in **Redis** with a 5-minute TTL (`RedisOtpService`)
  - Once both are verified the account becomes active and login is permitted
- OTP resend is rate-limited to once per minute per user to prevent abuse
- Login is blocked until both email and phone are verified

### Listing (Advert) Management
- Users can create, update, deactivate, and delete vehicle listings
- Each listing has a **30-day publication window** — it automatically expires after 30 days
- **One active listing per user** — a user cannot publish a second listing while one is active
- **30-day cooldown** — even if a listing is deleted early, the user must wait 30 days from their last publish date before creating a new one. This rule is enforced at both the application level and the database level (partial unique index)
- Listing owner can choose **phone visibility**:
  - `ShowPhoneNumber: true` → phone number is visible on the listing
  - `ShowPhoneNumber: false` → phone number is hidden; buyers can only contact via in-app messaging (messaging module planned)
- Listings support categories (with subcategories), images, price (multi-currency), and location (city/district)
- Search with pagination, filtering by keyword, category, city, price range, and status

### Background Services
- `AdvertExpiryBackgroundService` runs daily at midnight, finds all active listings past their `ExpiresAt` date, and transitions them to `Expired` status in parallel batches

---

## The Process

### `Initial commit`
Project structure established. Modular monolith skeleton with `User` and `Advert` modules, `SharedKernel`, and a single API host.

### `Add Advert module application and persistence` + `Add Advert module DI and registration` + `Add Advert endpoints and EF/Npgsql integration`
Advert module built out: domain entities (`Advert`, `Category`, `AdvertImage`, `AdvertOwnerInfo`), value objects (`Money`, `Location`), CQRS commands/queries via MediatR, EF Core persistence with PostgreSQL, and Minimal API endpoints for create/update/delete/search.

### `Switch DB provider to SQLite and add migrations`
Swapped PostgreSQL for SQLite for local development convenience. Added EF Core migrations and design-time `DbContextFactory` classes for both modules.

### `Add JWT auth, refresh tokens and SQL Server`
Major infrastructure milestone:
- Migrated both modules from SQLite to **SQL Server** (Docker-based)
- Implemented **JWT authentication** with access and refresh tokens
- Added `JwtTokenService`, `RefreshToken` entity, `RefreshTokenRepository`
- Introduced **BCrypt password hashing**
- Added `EnsureAdminExists` hosted service to seed an admin user on startup
- Added `docker-compose.yml` for local SQL Server container

### `Add login flow and refactor result/error types`
- Implemented complete **login flow**: user lookup, password verification, account lockout, JWT generation
- Added `LoginCommand`, `LoginCommandHandler`, `LoginRequest`, `LoginResponse`, `LoginEndpoint`
- Refactored shared result/error types into strongly-typed `Result<T>`, `Error`, and `ErrorType` in SharedKernel

### `Standardize errors, auth changes and migrations`
- Standardized error handling across all command handlers (`Unauthorized`, `NotFound`, `Forbidden`, `Conflict`)
- Added `RegisterEndpoint` and `RegisterRequest` DTO
- Logout now revokes all refresh tokens for the user via `RefreshTokenRepository`
- Regenerated SQL Server–compatible EF migrations for both modules

### `Add OTP verification, Redis & token blacklist`
- Added **email and phone OTP verification** to the registration flow
- New interfaces: `ISmsService`, `IEmailService`, `IOtpService`, `ICacheService`, `ITokenBlacklistService`
- Implementations: `NetgsmSmsService` (SMS via Netgsm API), `SmtpEmailService`, `RedisOtpService`, `RedisCacheService`, `TokenBlacklistService`
- Registration now creates **inactive** users and sends OTPs to both email and phone
- Login blocks accounts that haven't completed both verifications
- JWT `OnTokenValidated` hook checks the token blacklist on every authenticated request
- Logout revokes the access token (JTI stored in Redis) and clears the refresh cookie
- Login endpoint split for **mobile vs. web** clients (`X-Client-Type` header): mobile gets full response body, web gets an HttpOnly cookie for the refresh token
- OTP resend rate-limiting via Redis
- Added `docker-compose.yml` Redis service, Netgsm/SMTP settings in `appsettings.json`
- Downgraded MassTransit from 9.x to 8.3.6 (v9 requires a commercial license)

### `Add advert expiry, cooldown and phone visibility`
- `ExpiresAt` added to `Advert` — set to `CreatedAt + 30 days` on creation
- `Expire()` domain method raises `AdvertExpiredDomainEvent` and transitions status to `Expired`
- `ShowPhoneNumber` field on `Advert` — controls whether the owner's phone is returned in the listing DTO
- `LastAdvertPublishedAt` on `AdvertOwnerInfo` — tracks when the user last published, enforcing the 30-day cooldown
- `CreateAdvertCommandHandler` enforces: one active listing per user, 30-day cooldown, sets expiry and updates publish timestamp
- Partial unique index on `(OwnerUserId) WHERE Status = 'Active'` prevents race conditions at the database level
- New repository methods: `GetActiveAdvertByUserAsync`, `GetExpiredAdvertsBatchAsync`

---

## What I Learned

- **Modular monolith architecture** — how to split a single deployable application into cohesive, loosely-coupled modules that communicate via integration events (MassTransit) rather than direct references, making future extraction into microservices straightforward
- **CQRS with MediatR** — separating reads and writes at the handler level keeps each piece of business logic small, testable, and easy to reason about; validation pipelines via `FluentValidation` plug in cleanly as MediatR behaviors
- **JWT lifecycle management** — stateless tokens create a revocation problem; solving it with a Redis-backed blacklist indexed by JTI teaches you that "stateless" and "revocable" are in tension and requires careful design
- **OTP verification flow** — designing a multi-step verification process (register → send OTPs → verify email → verify phone → activate) taught me about TTLs, replay attacks (consume the OTP on first use), and rate-limiting resend requests to prevent abuse
- **Redis as a versatile tool** — using the same Redis instance for three distinct purposes (OTP storage, token blacklist, and general caching) through a single `ICacheService` abstraction shows how a cache layer can replace several one-off solutions
- **Partial unique indexes** — a filtered unique index on `(OwnerUserId) WHERE Status = 'Active'` elegantly enforces a business rule at the database level, acting as a last line of defense against race conditions that application-level checks can't fully prevent
- **Domain-driven design primitives** — value objects (`Money`, `Location`), domain events (`AdvertCreatedDomainEvent`, `AdvertExpiredDomainEvent`), and factory methods (`Advert.Create()`, `ApplicationUser.Register()`) make the domain model expressive and protect invariants without leaking logic into handlers
- **Background services in .NET** — `IHostedService` / `BackgroundService` for scheduled work (nightly advert expiry) with batched parallel processing using `Parallel.ForEachAsync` and scoped service lifetimes

---

## How Can It Be Improved

- **In-app messaging** — when a listing owner hides their phone number, buyers currently have no way to contact them; a `Message` module with real-time support (SignalR or WebSockets) would complete this feature
- **Outbox pattern** — integration events are published directly after saving to the database; if the publish step fails the modules go out of sync. An outbox table would make event delivery reliable and idempotent
- **Distributed background jobs (Hangfire)** — the current `BackgroundService` for advert expiry runs in-process and is tied to a single instance; Hangfire would provide a dashboard, retry logic, and multi-instance safety
- **Image upload** — listing images are currently stored as URLs; integrating Azure Blob Storage or AWS S3 with a dedicated upload endpoint would be more production-ready
- **Search with Elasticsearch** — EF Core `Contains` queries do not scale well for full-text search on large datasets; Elasticsearch or Azure Cognitive Search would provide fast, relevant results
- **Rate limiting** — only OTP resend is rate-limited; adding ASP.NET Core's built-in `RateLimiter` middleware globally (or per endpoint) would protect against brute-force and DDoS attacks
- **Architecture tests** — `HayatiArac.Architecture.Tests` project exists but is empty; adding NetArchTest rules would enforce layer boundaries (e.g., domain must not reference infrastructure) automatically on every build
- **Integration tests** — testing the full request/response cycle against a real database (Testcontainers for SQL Server and Redis) would catch issues that unit tests with mocks miss

---

## Running The Project

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Start infrastructure (SQL Server + Redis)

```bash
cd HayatiArac  # the folder containing docker-compose.yml
docker compose up -d
```

### 2. Configure secrets

Open `HayatiArac.Api/appsettings.json` and fill in the placeholder values:

```json
"Netgsm": {
  "ApiKey": "YOUR_NETGSM_API_KEY",
  "Sender": "HAYATIARAC"
},
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@gmail.com",
  "Password": "YOUR_APP_PASSWORD",
  "FromEmail": "your-email@gmail.com",
  "EnableSsl": true
}
```

> For Gmail, generate an **App Password** (not your regular password) under Google Account → Security → 2-Step Verification → App passwords.

### 3. Apply database migrations

```bash
# User module
dotnet ef database update \
  --project HayatiArac.Modules.User.Infrastructure \
  --startup-project HayatiArac.Api

# Advert module
dotnet ef database update \
  --project HayatiArac.Modules.Advert.Infrastructure \
  --startup-project HayatiArac.Api
```

### 4. Run the API

```bash
cd HayatiArac.Api
dotnet run
```

Swagger UI is available at `https://localhost:{port}/swagger`.

### Default admin account

An admin user is seeded automatically on first startup by `EnsureAdminExistsCommandHandler`. Check `DatabaseInitializerService` for the default credentials and change them immediately in a real environment.
