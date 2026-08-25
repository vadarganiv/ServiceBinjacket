# Architecture

## System context

```text
Browser (Albanian / English)
        |
        v
Nginx reverse proxy
   |                 |
   v                 v
Next.js 16       ASP.NET Core 10 API
                      |          |
                      v          v
                 PostgreSQL   upload volume
```

Nginx presents one origin. `/api/*` is routed to ASP.NET Core, framework/static routes are routed to Next.js, and `/uploads/*` is served from the upload volume.

## Backend projects

| Project | Responsibility | Dependencies |
|---|---|---|
| `ServisBinjaket.Domain` | Entities and enums | None |
| `ServisBinjaket.Application` | DTOs, validation, use cases, repository contracts | Domain |
| `ServisBinjaket.Infrastructure` | EF Core, PostgreSQL mappings/migrations, repositories, auth services | Application, Domain |
| `ServisBinjaket.Api` | HTTP controllers, middleware, auth, configuration, upload boundary | Application, Infrastructure |
| `ServisBinjaket.Tests` | Unit and integration-style tests | All projects |

The intended dependency direction is `Api -> Infrastructure -> Application -> Domain`. Controllers bind/validate HTTP input and delegate business/data work to use cases and repositories. Persistence entities are not exposed as public request contracts.

`AppDbContext` is request-scoped. EF Core migrations run during non-test application startup. The test project replaces PostgreSQL with a unique in-memory database where an HTTP test host is required.

## Frontend

```text
frontend/
  app/          App Router routes and layouts
  components/   shared shell components
  features/     catalog, cart, checkout, repair, and admin slices
  i18n/         routing and request configuration
  lib/          API clients and shared types
  messages/     sq/en translations
```

Server components fetch initial data. Interactive forms, cart state, filtering, and admin controls use client components. The root layout owns the single `<html>/<body>` document shell; nested locale layouts provide translations and feature providers without creating invalid nested document elements.

## Data and localization

PostgreSQL stores Albanian and English content fields (`*Sq`, `*En`). Application-layer mapping selects the requested locale and falls back from missing English content to Albanian. Enum values are stored as strings for readable data and migration-friendly additions.

## Deployment model

The production Compose topology exposes only Nginx. PostgreSQL and application containers communicate on the internal Docker network. Containers drop capabilities where practical and apply process/memory limits. See [DEPLOYMENT.md](DEPLOYMENT.md) for required production configuration.

## Deliberate boundaries

- Payments are cash-only; the `Payment` model is preparation for future work.
- Upload storage is local-volume based; private object storage is a future production improvement.
- The application uses repository/use-case boundaries but does not introduce messaging or distributed-service complexity that the current workflow does not need.
