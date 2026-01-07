# AppKernel Specification

## Overview
- **Tech stack:** .NET 9 solution following a clean architecture split across Domain, Application, Infrastructure, SharedKernel, and Web.Api projects. The API is implemented with ASP.NET Core minimal APIs plus MediatR for CQRS-style command/query handling.
- **Primary capabilities:** user management (registration/login + role/permission model), JWT-based authentication/authorization, Redis-backed caching, PostgreSQL persistence with EF Core + Dapper, outbox/event processing with Hangfire, OpenTelemetry/Serilog observability, and health checks.

## Solution layout
- **SharedKernel (`src/SharedKernel`)** – Cross-cutting primitives such as `Result`, `Error`, domain events, and the `Entity` base class that captures domain events for the outbox. Also includes the `RootUser` identity helper.
- **Domain (`src/Domain`)** – Core model:
  - Users: `User` aggregate with value objects `Email` and `Name`, a `HasPublicProfile` flag, password hash, roles, and `UserCreatedDomainEvent`.
  - Security: `Role`, `Permission` (predefined `users:*` and granular user permissions), and `RolePermission`.
  - Nodes: hierarchical `Node`/`NodeRelation` entities with soft-delete and `NodeDeletedDomainEvent`.
- **Application (`src/Application`)** – Use cases expressed as MediatR commands/queries with pipeline behaviors for exception handling, logging, validation, transactions (stubbed), and query caching. Abstractions define contracts for persistence, authentication (JWT, password hashing, user context), caching, events, and data seeding.
- **Infrastructure (`src/Infrastructure`)** – Implementations for Application abstractions: EF Core `ApplicationDbContext` with PostgreSQL, Dapper-based read models, Redis caching, Hangfire background jobs, JWT auth, authorization policy/handler, root user seeding, outbox pattern, in-memory integration event bus, and OpenTelemetry wiring.
- **Web.Api (`src/Web.Api`)** – Minimal API host with endpoint discovery, API versioning, Swagger, middleware (logging + global exception handling), authentication/authorization setup, Hangfire dashboard (dev), health checks, and startup seeding.
- **Tests (`tests`)** – Architecture tests enforcing layer boundaries, domain/application unit tests, application integration tests, and API functional tests.

## Domain model essentials
- **User** – Created via `User.Create(email, name, passwordHash, hasPublicProfile)` which raises a `UserCreatedDomainEvent`. Tracks assigned roles and basic profile flags; password hashing/verification is handled in Infrastructure. Value objects enforce non-empty name and email format.
- **Security** – `Role` and `Permission` capture role/permission relationships; permission constants cover the user module (wildcard + CRUD + reset password). `RootUser` is recognized by email to bypass permission checks.
- **Nodes** – Optional hierarchical resource with ancestor/descendant links and soft-delete (`IsDeleted` + query filters). Deletion raises `NodeDeletedDomainEvent`.

## Authorization conventions
### ResourceNode 來源
- **route**：資源識別來自路由參數（例如 `/nodes/{nodeId}`），以路由值解析資源節點。
- **body**：資源識別來自 request body（例如 `nodeId` 欄位），由 payload 解析資源節點。
- **externalKey**：資源識別來自外部鍵值（例如第三方系統對應碼），透過對應表或查詢轉換成資源節點。

### PermissionCode 命名規範
- 格式：`{domain}:{action}`（全部小寫，使用底線 `_` 分隔複合詞）。
- `domain` 表示資源或模組，例如 `users`、`members`、`member_points`。
- `action` 表示權限動作，例如 `read`、`create`、`update`、`delete`、`suspend`。
- 需要通配時使用 `*`，例如 `users:*` 代表 users 模組全權限。

### Deny/Allow 規則與繼承策略
- **明確 Deny 優先於 Allow**：同一資源與動作上，若存在 Deny，最終結果為拒絕。
- **繼承策略**：子節點未設定規則時，沿用最近的父節點規則；若一路皆未設定，預設拒絕。
- **多角色合併**：同一使用者多角色合併時，先彙總所有規則，再套用 Deny 優先於 Allow 的決策。

## Application layer patterns & use cases
- **Pipeline behaviors:** 
  - `ExceptionHandlingPipelineBehavior` logs unhandled exceptions.
  - `RequestLoggingPipelineBehavior` logs start/end plus errors.
  - `ValidationPipelineBehavior` invokes FluentValidation validators and returns `Result` failures.
  - `TransactionalPipelineBehavior` is currently a stub (logs and commits a default transaction placeholder).
  - `QueryCachingPipelineBehavior` short-circuits cached queries implementing `ICachedQuery`.
- **Abstractions:** `IUnitOfWork` and `IDbConnectionFactory` for data access, `ICacheService`/`ICachedQuery` for caching, `IPasswordHasher`/`IJwtService`/`IUserContext` for auth, `IRootUserService` and `IDataSeeder`, plus MediatR command/query interfaces.
- **User flows:**
  - **Register:** `CreateUserCommand` validates email uniqueness, hashes password, persists via `IUserRepository`, and returns the new ID.
  - **Login:** `LoginCommand` validates credentials and issues a JWT with roles/node/permission claims.
  - **Read models:** `GetUserByIdQuery` (Dapper) returns a DTO; `GetUserByEmailQuery` exists but currently filters by ID instead of email (bug to note).

## Infrastructure highlights
- **Data access:** PostgreSQL via EF Core (`ApplicationDbContext`) with snake_case and schema `public`; Dapper used for query handlers. Configurations map value objects and apply soft-delete filters for nodes/relations.
- **Outbox pattern:** `InsertOutboxMessagesInterceptor` captures domain events from tracked entities into `outbox_messages`; `ProcessOutboxMessagesJob` (Hangfire + Dapper) publishes them via MediatR and records processing metadata.
- **Messaging:** `EventBus` writes integration events to an in-memory channel; `IntegrationEventProcessorJob` bridges queued integration events to MediatR notifications.
- **Caching:** `CacheService` wraps `IDistributedCache` (Redis) with JSON serialization; queries implement `ICachedQuery` to leverage `QueryCachingPipelineBehavior`.
- **Authentication/Authorization:** JWT bearer auth configured from `JwtSettings`; `JwtService` issues/validates tokens; `UserContext` exposes the current user ID; `PermissionAuthorizationHandler` checks user permissions with a wildcard fallback and root-user bypass.
- **Background jobs & telemetry:** Hangfire server configured for recurring outbox processing (CRON from config). OpenTelemetry traces/metrics plus Serilog sinks (console/Otel/Seq).
- **Health/observability:** Health checks for Postgres and Redis; request context middleware adds correlation/trace/user IDs to logs.

## Web API surface
- **Endpoint discovery:** All `IEndpoint` implementations are registered automatically and mapped through `MapEndpoints`, optionally within versioned route groups.
- **Current endpoints:**
  - `POST /auth/login` (v1, anonymous) – issues JWT on successful credential check.
  - `GET /account/me` (frontend-v1 group) – returns the authenticated user ID from claims.
  - `Users` group (`/users`, admin-v1, API v2) – GET by ID and POST create user wired via `UseCaseInvoker`; older v1 user endpoints exist but are commented out.
- **Startup pipeline:** applies migrations in Development, enables Swagger + Hangfire dashboard, hooks background jobs, adds health checks, authentication/authorization, Serilog request logging, and seeds all registered `IDataSeeder` implementations (root user).

## Configuration & runtime
- **App settings:** `appsettings.Development.json` supplies Postgres/Redis connection strings, Serilog sinks (console + Seq), Hangfire outbox schedule (`BackgroundJobs:Outbox:Schedule`), JWT settings (secret/issuer/audience/expiry), and root user credentials. Default `appsettings.json` only defines `AllowedHosts`.
- **Migrations:** Initial schema creates `users` and `outbox_messages`; later migrations add password hash plus `node`, `node_relation`, `role`, `permission`, and join tables. The latest migration adds a `description` column to `permission`.
- **Docker:** Solution includes Docker artifacts (compose + Web.Api Dockerfile); Web.Api project sets `DockerDefaultTargetOS` and compose context.

## Testing strategy
- **Architecture tests** ensure layer independence (Domain ⟂ Application/Infrastructure/Presentation; Application ⟂ Infrastructure/Presentation; Infrastructure ⟂ Presentation).
- **Domain/Application unit tests** cover user creation invariants and command behavior; **integration tests** spin up Web.Api with Infrastructure; **functional tests** exercise API endpoints through HTTP.

## Notable gaps/risks
- `TransactionalPipelineBehavior` currently creates a default transaction placeholder instead of using a real unit of work transaction.
- `GetUserByEmailQueryHandler` filters on `id = @Email`, so email lookups will fail until corrected.
- `GetUserById` endpoint handler is structured as a method wrapper without returning a delegate, and several v1 user endpoints are commented out, indicating unfinished routing. 
