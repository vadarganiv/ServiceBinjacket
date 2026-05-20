# Deployment

## Локальная разработка

```bash
cp .env.example .env
docker compose up --build
```

URL'ы:
- Frontend: `http://localhost:3000` (root → `/sq`)
- Backend:  `http://localhost:5000/api/v1`
- Swagger:  `http://localhost:5000/swagger`
- Postgres: `localhost:5432`

Раздельный запуск без Docker:

```bash
# Postgres (нужен Docker)
docker compose up -d postgres

# Backend
cd backend && dotnet restore && dotnet run --project src/ServisBinjaket.Api

# Frontend
cd frontend && npm install && npm run dev
```

## Environment

Переменные задаются в `.env` (не коммитится). Шаблон — `.env.example`.

Все `change_me` **обязательно** меняются перед деплоем.

## Production — деплой на VPS по IP (текущий режим)

Схема: `[Internet] → [nginx :80] → [frontend :3000] / [backend :5000] → [postgres :5432]`

### Шаги

**1. Подготовить VPS (Ubuntu 22.04+)**

```bash
# Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
newgrp docker

# Docker Compose plugin
sudo apt-get install -y docker-compose-plugin
docker compose version
```

**2. Получить код и настроить env**

```bash
git clone <repo-url> /opt/servis-binjaket
cd /opt/servis-binjaket

cp .env.example .env
nano .env          # заполнить все переменные (заменить change_me)
```

**3. Запустить**

```bash
docker compose -f docker-compose.prod.yml build --pull
docker compose -f docker-compose.prod.yml up -d
```

**4. Проверить**

```bash
curl http://<VPS_IP>/health               # → 200
curl http://<VPS_IP>/api/v1/products      # → JSON
curl -I http://<VPS_IP>/                  # → 302 → /sq
docker compose -f docker-compose.prod.yml ps
```

**5. Смотреть логи**

```bash
docker compose -f docker-compose.prod.yml logs -f
docker compose -f docker-compose.prod.yml logs nginx
docker compose -f docker-compose.prod.yml logs backend
```

**6. Сменить дефолтный пароль admin после первого логина**

Если в `.env` оставлен placeholder `ADMIN_DEFAULT_PASSWORD=change_me`, backend
залогирует предупреждение и создаст admin'а с этим паролем. **Сразу после первого
входа** обновите хэш через psql.

Сгенерировать bcrypt-хэш можно временным контейнером с тем же `BCrypt.Net-Next`,
который использует backend:

```bash
# Получить хэш нового пароля (заменить NEW_PASSWORD)
docker run --rm mcr.microsoft.com/dotnet/sdk:9.0 sh -c \
    'mkdir /t && cd /t && dotnet new console -o . --force >/dev/null && \
     dotnet add package BCrypt.Net-Next >/dev/null && \
     echo "Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(\"NEW_PASSWORD\"));" > Program.cs && \
     dotnet run 2>/dev/null'

# Записать новый хэш в БД
docker compose -f docker-compose.prod.yml exec postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
    -c "UPDATE admin_users SET password_hash = '<BCRYPT_HASH>' WHERE email = '<ADMIN_EMAIL>';"
```

Если `JWT_SECRET` оставлен placeholder в Production — backend упадёт на старте
с понятной ошибкой и не запустится. Сгенерируйте: `openssl rand -base64 48`.

### Обновление (новый релиз)

```bash
cd /opt/servis-binjaket
git pull

# Сделать backup перед деплоем
bash scripts/backup-db.sh

docker compose -f docker-compose.prod.yml up -d --build
```

### После security incident / подозрения на компрометацию

Если frontend/backend контейнер мог получить RCE, не переиспользуйте старые контейнеры и слои как доверенные:

```bash
cd /opt/servis-binjaket

# 1) Сохранить бэкапы/forensic artifacts до удаления, если они нужны расследованию
bash scripts/backup-db.sh

# 2) Остановить старый runtime
docker compose -f docker-compose.prod.yml down --remove-orphans

# 3) Забрать проверенный код и пересобрать без cache
git fetch --all --prune
git checkout <trusted-commit-or-tag>
docker compose -f docker-compose.prod.yml build --no-cache --pull
docker compose -f docker-compose.prod.yml up -d

# 4) После проверки удалить старые dangling/compromised images вручную по ID
docker image ls
docker image rm <old_image_id>
```

Обязательно вручную на VPS:

1. Ротировать все секреты из `.env` без вывода значений в терминал/логи: `JWT_SECRET`, `ADMIN_DEFAULT_PASSWORD`/admin password hash, `POSTGRES_PASSWORD`, `DATABASE_CONNECTION_STRING`, deploy/registry credentials и внешние API keys.
2. Проверить `uploads_data` на неожиданные executable/scripts и удалить вредоносные файлы.
3. Настроить host-level egress firewall для Docker workloads: запретить исходящее сканирование, разрешить только необходимые направления/порты. Compose не является достаточным механизмом egress firewall.
4. Удалить старые контейнеры/images только после сохранения нужных forensic artifacts.

---

## Production — переход на HTTPS (когда появится домен)

Когда будет куплен домен и прописана A-запись на IP VPS:

**1. Обновить `.env`**

```env
DOMAIN=your-domain.com
CERTBOT_EMAIL=your@email.com
PUBLIC_SITE_URL=https://your-domain.com
```

**2. Обновить `nginx/nginx.conf`**

- Найти `YOUR_DOMAIN` во всём файле и заменить на реальный домен
- Закомментировать блок `server { listen 80; server_name _; ... }` (HTTP catch-all)
- Раскомментировать блоки `# HTTP → HTTPS redirect` и `# HTTPS` в конце файла

**3. Обновить `docker-compose.prod.yml`**

- Раскомментировать `- "443:443"` в сервисе nginx
- Раскомментировать весь сервис `certbot`

**4. Получить сертификат**

```bash
# Сначала убедиться что nginx запущен (нужен для webroot challenge)
docker compose -f docker-compose.prod.yml up -d nginx

# Получить сертификат
docker compose -f docker-compose.prod.yml run --rm certbot

# Перезагрузить nginx
docker compose -f docker-compose.prod.yml exec nginx nginx -s reload
```

**5. Настроить автообновление (cron)**

```bash
crontab -e
```

Добавить:

```cron
0 3 * * * cd /opt/servis-binjaket && docker compose -f docker-compose.prod.yml run --rm certbot renew --quiet && docker compose -f docker-compose.prod.yml exec nginx nginx -s reload
```

**6. Проверить**

```bash
curl https://your-domain.com/health
curl -I http://your-domain.com/     # должен вернуть 301 → https://
```

---

## Backups

### База данных

```bash
# Вручную
bash /opt/servis-binjaket/scripts/backup-db.sh

# По расписанию (cron, ежедневно в 03:00)
crontab -e
# Добавить:
0 3 * * * /opt/servis-binjaket/scripts/backup-db.sh >> /var/log/servis-backup.log 2>&1
```

Бэкапы сохраняются в `BACKUP_DIR` (по умолчанию `/backups`). Файлы старше 30 дней удаляются автоматически.

### Restore базы данных

```bash
bash /opt/servis-binjaket/scripts/restore-db.sh /backups/db-20260515-030000.sql.gz
```

Скрипт выводит предупреждение и даёт 5 секунд на отмену (Ctrl+C).

### Загруженные файлы (uploads)

```bash
# Backup uploads volume
docker run --rm \
  -v servis-binjaket_uploads_data:/source:ro \
  -v /backups:/dest \
  alpine tar -czf /dest/uploads-$(date +%Y%m%d).tar.gz -C /source .

# По расписанию (cron, еженедельно в воскресенье 04:00)
0 4 * * 0 docker run --rm -v servis-binjaket_uploads_data:/source:ro -v /backups:/dest alpine tar -czf /dest/uploads-$(date +\%Y\%m\%d).tar.gz -C /source .
```

> Имя volume соответствует `servis-binjaket_uploads_data` (префикс — имя папки проекта).

### Restore uploads

```bash
docker run --rm \
  -v servis-binjaket_uploads_data:/dest \
  -v /backups:/source \
  alpine sh -c "rm -rf /dest/* && tar -xzf /source/uploads-YYYYMMDD.tar.gz -C /dest"
```

---

## Чек-лист перед каждым прод-деплоем

1. `dotnet test` зелёный.
2. `npm run build` проходит.
3. Skill `task-done` пройден.
4. Reviewer (Opus) сделал ревью если задача затрагивала auth/uploads/payments/deploy.
5. Backup сделан **до** деплоя.
6. После деплоя — smoke test: `/` → `/sq`, `/sq/products`, `/api/v1/health`, admin login.

## Не хранить в репо

- `.env`, `nginx/certs/`, `backups/`, `*.sql.gz`, `node_modules/`, `bin/`, `obj/`
- В Docker image: `.env`, секреты — только через runtime env vars
