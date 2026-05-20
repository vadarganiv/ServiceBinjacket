# Architecture

## Stack

- **Frontend:** Next.js (App Router) + TypeScript + Tailwind CSS + shadcn/ui + React Hook Form + Zod + next-intl.
- **Backend:** ASP.NET Core Web API + C# + EF Core + FluentValidation + Serilog + Swagger.
- **DB:** PostgreSQL.
- **Infra:** Docker + Docker Compose + Nginx + Let's Encrypt + VPS.

## Diagram (text)

```
[browser sq/en] ── HTTPS ──> [nginx] ──> [next.js]
                                  └──> [asp.net core api] ──> [postgres]
                                                          └──> [local uploads volume]
```

## Backend — Clean Architecture слои

| Project | Содержит | Зависит от |
|---|---|---|
| `ServisBinjaket.Domain` | Entities, enums, value objects, доменные правила | — |
| `ServisBinjaket.Application` | UseCases / services, DTOs, FluentValidators, интерфейсы репозиториев | Domain |
| `ServisBinjaket.Infrastructure` | EF Core DbContext, конфигурации, миграции, файловое хранилище, внешние интеграции | Application, Domain |
| `ServisBinjaket.Api` | Controllers / minimal API endpoints, middleware, DI, Swagger | Application, Infrastructure |
| `ServisBinjaket.Tests` | xUnit + FluentAssertions, юнит и интеграционные тесты | All |

**Dependency rule:** зависимости идут только внутрь (Api → Infrastructure → Application → Domain). Domain ни от чего не зависит.

**Controllers** — только маршрутизация, model binding, авторизация, вызов UseCase. Никакой бизнес-логики.

## Frontend — структура

```
frontend/
  app/
    [locale]/            — public пути
      page.tsx
      products/
      services/
      repair-request/
      checkout/
      ...
    admin/               — admin UI (вне locale prefix)
  components/            — переиспользуемые UI (button, card, modal, ...)
  features/              — feature-folders (catalog, repair-request, order, admin/*)
  lib/                   — api client, утилиты, i18n helpers, validation schemas
  messages/
    sq.json
    en.json
  public/
```

- Feature-first: всё, что относится к одной фиче (страница + компоненты + хуки + schema + api calls) — в `features/<name>/`.
- Общие компоненты (Button, Card, Input) — в `components/`.
- API-клиент — один в `lib/api.ts`, проксирует на ASP.NET через `process.env.API_URL`.

## Repository layout

```
Servis Binjaket/
  docs/
    *.md
  backend/
    ServisBinjaket.sln
    src/
      ServisBinjaket.Api/
      ServisBinjaket.Application/
      ServisBinjaket.Domain/
      ServisBinjaket.Infrastructure/
    tests/
      ServisBinjaket.Tests/
  frontend/
    app/, components/, features/, lib/, messages/, public/
    package.json
  nginx/
    nginx.conf
  scripts/
    backup-db.sh
    restore-db.sh
  docker-compose.yml
  docker-compose.prod.yml
  .env.example
  .gitignore
  README.md
```

## Cross-cutting

- **Логирование:** Serilog на backend, console JSON в production, ротация через docker.
- **Файлы:** в MVP — локальный volume `/app/uploads`. Будущее — S3-compatible (Cloudflare R2 / Backblaze B2 / MinIO).
- **Errors:** единый формат ошибок (см. `docs/API.md`).
- **i18n:** на бэке локализованные поля в БД (`*Sq`, `*En`), DTO отдаёт по `?locale`. На фронте — next-intl + JSON-файлы.
