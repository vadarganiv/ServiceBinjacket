# Deployment

## Локальная разработка

```powershell
# одной командой
docker compose up --build

# или раздельно
docker compose up -d postgres

# backend
cd backend
dotnet restore
dotnet build
dotnet test
dotnet run --project src/ServisBinjaket.Api

# frontend
cd frontend
npm install
npm run dev
```

URL'ы:
- Frontend: `http://localhost:3000` (root → `/sq`)
- Backend: `http://localhost:5000/api/v1`
- Swagger: `http://localhost:5000/swagger`
- Postgres: `localhost:5432`

## Environment

`.env.example` содержит:

```env
POSTGRES_DB=servis_binjaket
POSTGRES_USER=servis_user
POSTGRES_PASSWORD=change_me
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=servis_binjaket;Username=servis_user;Password=change_me

JWT_SECRET=change_me_long_random_secret_at_least_32_bytes
ADMIN_DEFAULT_EMAIL=admin@example.com
ADMIN_DEFAULT_PASSWORD=change_me

PUBLIC_SITE_URL=https://example.com
API_URL=http://backend:5000

UPLOADS_ROOT=/app/uploads
MAX_UPLOAD_MB=25

WHATSAPP_PHONE=+355000000000
DEFAULT_CURRENCY=ALL
DEFAULT_LOCALE=sq
SUPPORTED_LOCALES=sq,en
```

Все `change_me` обязательно меняются в production.

## Production (VPS + Docker Compose + nginx + Let's Encrypt)

```
[Internet] → [nginx :443/:80] → [next.js :3000]
                              → [asp.net :5000] → [postgres :5432]
                                                → volume /app/uploads
```

Шаги:

1. Подготовить VPS (Ubuntu LTS), установить Docker + Compose.
2. Склонировать репо, скопировать `.env.example` → `.env`, заполнить.
3. Указать домен в DNS (A-запись на VPS).
4. `docker compose -f docker-compose.prod.yml up -d --build`.
5. Получить cert: `docker run --rm -v ./nginx/certs:/etc/letsencrypt certbot/certbot certonly --webroot -w /var/www/certbot -d example.com` (точная команда — в TASK-014).
6. Reload nginx.
7. Проверить:
   - `https://example.com/` → `/sq`
   - `https://example.com/sq/products` рендерится
   - `https://example.com/api/v1/health` → 200
   - Swagger — недоступен или защищён

## Backups

`scripts/backup-db.sh`:

```bash
#!/usr/bin/env bash
set -euo pipefail
ts=$(date +%Y%m%d-%H%M%S)
docker exec servis-binjaket-postgres pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB" \
  | gzip > "/backups/db-$ts.sql.gz"
find /backups -name 'db-*.sql.gz' -mtime +30 -delete
```

Расписание: cron на VPS, ежедневно ночью.

Uploads backup:

```bash
tar -czf "/backups/uploads-$(date +%Y%m%d).tar.gz" /var/lib/docker/volumes/servis_uploads/_data
```

Restore — см. `scripts/restore-db.sh` (создаётся в TASK-014).

## Не хранить

- В репо: `.env`, `nginx/certs/`, `backups/`, `*.sql.gz`, `node_modules/`, `bin/`, `obj/`.
- В docker image: `.env`, секреты — только через runtime env vars.

## Чек-лист перед каждым прод-деплоем

1. `dotnet test` зелёный.
2. `npm run build` проходит.
3. Skill `task-done` пройден.
4. Reviewer (Opus) сделал ревью если задача затрагивала auth/uploads/payments/deploy.
5. Backup сделан **до** деплоя.
6. После деплоя — smoke test main flow (заявка + заказ + admin login).
