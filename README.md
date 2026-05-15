# Servis Binjaket

Albanian-first интернет-магазин электроники + сервис ремонта в Дурресе (Албания).

- **Frontend:** Next.js (App Router) · TypeScript · Tailwind · next-intl — локали `sq` / `en`
- **Backend:** ASP.NET Core Web API · EF Core · PostgreSQL
- **Infra:** Docker Compose · Nginx · Let's Encrypt

---

## Локальный запуск

### Вариант 1 — Docker Compose (рекомендуется)

```bash
cp .env.example .env
# При необходимости отредактировать .env

docker compose up --build
```

| Сервис   | URL                                |
|----------|------------------------------------|
| Frontend | http://localhost:3000  (→ `/sq`)   |
| Backend  | http://localhost:5000/api/v1       |
| Swagger  | http://localhost:5000/swagger      |
| Postgres | localhost:5432                     |

### Вариант 2 — Раздельный запуск

**Postgres** (нужен Docker):

```bash
docker compose up -d postgres
```

**Backend:**

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/ServisBinjaket.Api
```

**Frontend:**

```bash
cd frontend
npm install
npm run dev
```

---

## Тесты

```bash
# Backend unit + integration
cd backend && dotnet test

# Frontend type-check
cd frontend && npm run build
```

---

## Структура репозитория

```
backend/      — ASP.NET Core solution (Clean Architecture)
frontend/     — Next.js app (App Router + i18n)
nginx/        — nginx.conf для production
scripts/      — backup-db.sh, restore-db.sh
docs/         — проектная документация
docker-compose.yml       — локальная разработка
docker-compose.prod.yml  — production (создаётся в TASK-014)
.env.example             — шаблон переменных окружения
```

---

## Production deploy

Описан в [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md).
