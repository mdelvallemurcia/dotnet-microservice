# Tasks — "make it read as professional" backlog

Prioritized, incremental checklist. **P1** = highest effort/impact ratio (do first), **P3** = nice-to-have.
Feature-level ideas live in the [readme Roadmap](./readme.md#roadmap--todo); this file is about quality/finish.

## P1 — Security & repo hygiene (biggest credibility wins)

- [ ] **Get secrets out of git.** `appsettings.Development.json` and `.env`/`.env.development` are currently tracked with a real JWT `SecretKey` and Mongo credentials.
  - [ ] Add `.gitignore` rules for `**/appsettings.Development.json` and `**/.env*`.
  - [ ] `git rm --cached` those files (keep them locally).
  - [ ] Rotate the leaked JWT `SecretKey`.
  - [ ] Move dev secrets to **User Secrets** (`dotnet user-secrets`) / environment variables; commit a `*.example` template instead.
- [ ] **Replace hardcoded login.** Credentials are hardcoded in `Auth/Login/v1/Handler.cs`. Add a minimal user store (even in Mongo) or clearly fence it as a dev-only seeded user.
- [ ] **Implement real HMAC validation.** `HmacAuthenticationHandler.ValidateHmac` always returns `true` — implement signature + timestamp checks or remove the scheme until it's real.

## P1 — Frontend hygiene

- [x] **Fix `afterLogoutActions`** in `AuthProvider.tsx` — it calls `useFetch` inside a function, violating the Rules of Hooks. Use the single-flight client / a plain `fetch` instead.
- [x] Remove dead commented-out blocks (`AuthProvider.tsx`, `useFetch.ts`) and leftover `console.log`/`console.info`.

## P2 — Backend cleanup

- [x] Drive analyzer warnings to **0** — curated `.editorconfig` ruleset (fix real signal: CS86xx nullability, CA2016/CA2000, CA13xx culture, CA2254 structured logging, ASPDEPR005; suppress library-author noise with justification: CA1062/1515/1812/1819/1308/1724/1716/1711/1707/1848/1031) + proper `TreatWarningsAsErrors` in `Directory.Build.props`. api/worker/tests/apphost all build clean.
- [x] `GenerateRefreshToken(string userId)` ignores its `userId` argument — dropped the param (interface + impl + 2 call sites).
- [x] Unify refresh-token expiration (login set `AddHours(1)`, refresh set `AddDays(1)`) and make it **config-driven** — single `IAuthFacade.GetRefreshTokenExpiresAt()` reads `BearerToken:RefreshTokenExpirationInDays` (default 7).
- [x] Replace `Console.WriteLine` in the JwtBearer `OnAuthenticationFailed` event with proper `ILogger`.
- [x] Add **MongoDB indexes** for `RefreshToken` (Hash unique, UserName, ExpiresAt as TTL) — `RefreshTokenIndexInitializer` hosted service.

## P2 — Tests

- [ ] Add meaningful coverage for the auth flow: login, refresh **rotation**, **reuse detection** (revoked token → family revoked), and **fingerprint** validation (claim == hash(cookie); HMAC exempt). Test projects exist but the critical paths aren't covered.

## P3 — Auth & polish

- [ ] Constant-time fingerprint comparison in `FingerprintValidationMiddleware` (`CryptographicOperations.FixedTimeEquals`).
- [ ] Extract a shared `AuthSchemes` constants holder (the `"DynamicAuth"` literal still appears twice; `"HMAC"` is already unified).
- [ ] Consider `HybridCache` for refresh-token lookups (noted in the readme Roadmap).
- [ ] Harden `ConfigureForwardedHeaders` for real deploys (restrict `KnownProxies`/`KnownNetworks` to the ingress).

## Done this session (for reference)

- [x] Cookie-based refresh (read from cookie, never from body).
- [x] Reuse detection on `IsActive`; whole-family revoke on replay.
- [x] Refresh token removed from login/refresh response bodies.
- [x] Client single-flight refresh (`api/authClient.ts`) — kills the stale-closure retry and concurrent-rotation logout.
- [x] Random OWASP fingerprint (raw in cookie / hash in claim+DB); HMAC exempt from fingerprint check.
- [x] Dev same-origin Vite proxy; HTTPS redirect off in Development; `UseForwardedHeaders`.
- [x] CORS origins from config; `"HMAC"` scheme name unified into `HmacAuthenticationHandler.SchemeName`.
- [x] `CLAUDE.md` + README added.
