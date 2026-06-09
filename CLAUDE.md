# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

.NET 10 microservice backend + React 19 SPA, orchestrated locally with **.NET Aspire**. An HTTP API publishes domain events over **RabbitMQ (MassTransit)**; a worker consumes them; **MongoDB** is the store; **OpenTelemetry** traces/metrics/logs go to a Grafana LGTM container. Aspire's AppHost wires all of it — including the frontend — so it is the normal entry point for running locally.

Topology: `Web (Vite/React) → Api → {MongoDB, RabbitMQ} → ProjectSubscriber (worker) → MongoDB`.

## Commands

Run everything (DB, broker, OTel, API, worker, frontend) via Aspire — this is the default way to develop:
```bash
dotnet run --project src/_aspire/Aspire.AppHost
```

Backend build / test:
```bash
dotnet build dotnet-microservices.slnx
dotnet test                                              # all tests
dotnet test test/unit/Api.Features.Test.Unit            # one project (xUnit)
dotnet test --filter "FullyQualifiedName~SomeTestName"  # a single test
```

Frontend (`src/web/ui`) — Aspire runs `npm run dev` for you; run directly only when iterating on the UI alone:
```bash
npm run dev      # vite (see Dev gotchas re: ports)
npm run build    # tsc -b && vite build
npm run lint     # eslint
```

Note: a running Aspire/API process locks the output DLLs, so `dotnet build` reports `MSB3027` copy errors while the app is up — these are not compile errors. To verify a backend change without stopping the app, build and check only for `error CS` lines.

## Backend architecture

**Vertical-slice endpoints.** Features live under `src/web/Api.Features/<Area>/<Feature>/v1/` (e.g. `Auth/Login/v1`, `Projects/ProjectsGet/v1`). Each slice is self-contained: `Handler.cs` (a class implementing `IEndpointModule` that maps the route in `MapEndpoints` and holds a static `Handle` delegate), plus `Request`/`Response`/`Validator`/`Mapper` as needed. There is no controller layer — these are Minimal API endpoints.

**Endpoint auto-discovery.** `EndpointDiscoveryExtensions.MapDiscoveredEndpoints` reflects over the `Api.Features` assembly and instantiates every `IEndpointModule`. To add an endpoint, just create a class implementing `IEndpointModule`; no manual registration.

**API versioning** is URL-segment based (`Asp.Versioning`): routes are `/v{version:apiVersion}/...`, default v1, with `AssumeDefaultVersionWhenUnspecified`. Map a slice to its version with `.MapToApiVersion(1, 0)`.

**Validation** uses FluentValidation via SharpGrip auto-validation: add a `Validator : AbstractValidator<Request>` in the slice and chain `.AddFluentValidationAutoValidation()` on the endpoint. Validators are registered by assembly scan in `Program.cs`.

**Persistence** is abstracted behind `IRepository<T> where T : BaseEntity` (`src/Infrastructure`), implemented by `MongoDbRepository`. Entities live in `src/Models` (`Models.Entity`), events in `Models.Events`. Repository registration: `AddMongoRepository(...)`.

**Messaging.** The API publishes events (`Models.Events.Project.*`) through MassTransit; the worker `src/worker/Product.Consumer` (assembly/namespace `ProjectSubscriber`) hosts `IConsumer<T>` consumers. Both configure RabbitMQ from `RabbitMqOptions`.

**Observability.** Both services use `Aspire.ServiceDefaults` (`AddServiceDefaults`) and OpenTelemetry, exporting OTLP to the `grafana/otel-lgtm` container started by the AppHost.

## Authentication (the most intricate part — read before touching auth)

Central service: `IAuthFacade` / `AuthFacade` (`src/web/Api.Features/Services/Auth`). The model is **access token in memory + rotating refresh token in an httpOnly cookie**:

- **Two auth schemes** behind a `DynamicAuth` policy scheme that forwards by `Authorization` prefix: `Bearer ` → JWT, `Ldx ` → `HmacAuthenticationHandler` (scheme name = `HmacAuthenticationHandler.SchemeName`). A fallback policy requires an authenticated user globally; role policies (`RequireAdmin`, `RequireReader`) use `RequireRole`.
- **Refresh tokens** are read from the `refresh_token` cookie (never the body — responses never return the refresh token). On `/v1/refresh` the token is rotated; presenting a revoked token triggers reuse detection that revokes the whole family. Cookie paths differ by purpose: `refresh_token` → `/v1/refresh`, `fp` → `/`.
- **Fingerprint binding** (`FingerprintValidationMiddleware`): a random per-session value; the **raw** value is in the `fp` cookie and its **hash** is the JWT `fp` claim. The middleware rejects when `claim != GenerateHash(cookie)`, and skips validation for HMAC callers. When changing fingerprint logic, keep the cookie=raw / claim+DB=hash split, and generate the value once per login/refresh (the login `Mapper` must receive the hash, not regenerate it).
- **WIP/stubbed:** there is no user store — login is hardcoded in `Auth/Login/v1/Handler.cs`; `HmacAuthenticationHandler.ValidateHmac` returns `true`. Treat these as placeholders.

Frontend side (`src/web/ui/src`): `context/AuthProvider` holds the access token in memory only (never localStorage) and runs a silent refresh on load; `api/authClient.ts` exposes a **single-flight** `refreshAccessToken()` shared by all callers (concurrent 401s must not each rotate the token, or reuse detection logs the user out); `hooks/useFetch.ts` attaches the bearer, retries once on 401 with the freshly refreshed token, and sends `credentials: 'include'`.

## Dev gotchas (hard-won, non-obvious)

- **Vite stays on port 5174.** Aspire's frontend endpoint is `port: 5173, targetPort: 5174` (`Aspire.AppHost/AppHost.cs`) — the browser hits 5173 (Aspire), Vite serves 5174. Setting Vite to 5173 collides with Aspire's proxy and breaks the UI.
- **`VITE_API_URL=http://localhost:5173`** (HTTP, the Aspire origin) in `src/web/ui/.env.development`. Vite proxies `/v1` to the API so cookies are same-origin/first-party. Pointing it at the API directly makes the auth cookies cross-origin (they won't be sent → 400 on refresh); pointing it at `https://localhost:5173` causes `ERR_SSL_PROTOCOL_ERROR` (5173 is plain HTTP).
- **HTTPS redirection is disabled in Development** (`Program.cs`) so the HTTP proxy hop to the API works and cookies aren't forced `Secure` (an http page can't store Secure cookies). `UseForwardedHeaders` is enabled so the real scheme is honored behind a TLS-terminating proxy in prod.
- **CORS allowed origins** come from config (`Cors:AllowedOrigins`), not hardcoded. Prod target is same root domain, different subdomains (e.g. `app.*`/`api.*`) — that is same-site, so `SameSite=Lax` cookies work without `SameSite=None`.

## Conventions

- Code, identifiers, and comments are English (some legacy comments are Spanish). Endpoint slices mirror the `<Area>/<Feature>/v1` folder shape; project file names are lowercase (`api.csproj`) while assemblies/namespaces are PascalCase.
- Commit style: Conventional Commits, no AI attribution.
