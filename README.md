# Servis Binjaket

Servis Binjaket is a full-stack storefront and repair-request system for an Albanian electronics service business. It is a portfolio project focused on a practical business workflow rather than a generic CRUD demo: a bilingual public catalog, checkout and repair intake are paired with an authenticated administration area.

The Albanian brand spelling, **Servis Binjaket**, is intentional and is used consistently in the solution and namespaces.

> A public demo is not currently advertised. The previous deployment used an HTTP-only IP address and placeholder data; publish a new link only after HTTPS and representative demo data are ready.

## What the project demonstrates

- Cleanly separated ASP.NET Core API, application, domain, and infrastructure projects
- PostgreSQL persistence through EF Core migrations and repository abstractions
- Next.js App Router UI with Albanian (`sq`) and English (`en`) localization
- Product catalog filtering, details, cart, cash checkout, and repair intake
- JWT administrator authentication in an HttpOnly cookie, logout invalidation, and login rate limiting
- Defensive file uploads with per-file/request limits, extension/MIME/signature agreement, generated storage names, and upload rate limiting
- Docker Compose deployment behind a hardened Nginx reverse proxy
- Automated backend tests, frontend lint/type/build checks, dependency auditing, and CI

## Technology

| Area | Stack |
|---|---|
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS, next-intl |
| Backend | .NET 10, ASP.NET Core Web API, EF Core, FluentValidation, Serilog |
| Data | PostgreSQL 16 |
| Delivery | Docker, Docker Compose, Nginx |
| Quality | xUnit, FluentAssertions, ESLint, GitHub Actions, Dependabot |

## Architecture

```text
Browser (sq/en)
    |
    v
Nginx reverse proxy
    |-- Next.js application
    |-- ASP.NET Core API
            |-- Application use cases
            |-- Domain model
            `-- EF Core / PostgreSQL
```

The backend follows the dependency direction `Api -> Infrastructure -> Application -> Domain`. HTTP request and response DTOs are kept separate from persistence entities. The frontend uses App Router route segments and feature folders; server components perform server-side reads while interactive forms and admin views remain client components.

## Run with Docker Compose

Prerequisite: Docker with the Compose plugin.

```bash
cp .env.example .env
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 (redirects to `/sq`) |
| API | http://localhost:5000/api/v1 |
| Swagger UI | http://localhost:5000/swagger |
| PostgreSQL | localhost:5432 |

The values marked `change_me` are development placeholders. The backend deliberately refuses to start in `Production` when the JWT secret or default administrator password is missing, weak, or still a placeholder.

## Run the applications separately

Prerequisites: .NET SDK 10, Node.js 24, npm, and PostgreSQL 16 (the database can run through Docker).

```bash
docker compose up -d postgres

cd backend
dotnet restore
dotnet run --project src/ServisBinjaket.Api
```

In another terminal:

```bash
cd frontend
npm ci
npm run dev
```

## Quality checks

Run the same checks used by CI:

```bash
cd backend
dotnet restore
dotnet format ServisBinjaket.sln --verify-no-changes --no-restore
dotnet test ServisBinjaket.sln --configuration Release --no-restore
dotnet list ServisBinjaket.sln package --vulnerable --include-transitive --no-restore
```

```bash
cd frontend
npm ci
npm run lint
npm run typecheck
npm run build
npm audit --audit-level=high
```

NuGet advisories are treated as build errors. The lockfile is committed so CI and local installs resolve the same frontend dependency graph.

## Configuration highlights

| Variable | Purpose | Development default |
|---|---|---|
| `DATABASE_CONNECTION_STRING` | PostgreSQL connection | Local Compose database |
| `JWT_SECRET` | Administrator JWT signing key | Placeholder; rejected in Production |
| `ADMIN_DEFAULT_EMAIL` | First administrator email | Required only while bootstrapping an empty Production database |
| `ADMIN_DEFAULT_PASSWORD` | First administrator password | Required only for first bootstrap; placeholder/short values are rejected |
| `CORS_ORIGINS` | Comma-separated browser origins | `http://localhost:3000` |
| `MAX_UPLOAD_MB` | Maximum size of one attachment | `25` |
| `MAX_UPLOAD_FILES` | Maximum attachments per repair upload | `4` |
| `MAX_UPLOAD_TOTAL_MB` | Maximum combined attachment size | `30` |

See [`.env.example`](.env.example) for the complete list. Production deployment should use HTTPS, unique secrets, a non-default database password, and real business contact details.

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [API](docs/API.md)
- [Data model](docs/DATA_MODEL.md)
- [Internationalization](docs/I18N.md)
- [Security](docs/SECURITY.md)
- [Deployment](docs/DEPLOYMENT.md)
- [Architecture decisions](docs/DECISIONS.md)

## Scope

This version supports cash payment workflows only (`CashOnDelivery` and `CashInStore`). Online card processing, object storage, antivirus scanning, and automated HTTPS certificate provisioning are intentionally documented as follow-up work rather than presented as completed features.

## License

[MIT](LICENSE)
