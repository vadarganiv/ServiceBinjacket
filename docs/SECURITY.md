# Security

MVP-уровень, но всерьёз.

## Admin auth

- Passwords хэшируются (BCrypt cost ≥ 12 или Argon2id). Никаких plaintext.
- Default admin создаётся при первом запуске из `ADMIN_DEFAULT_EMAIL` / `ADMIN_DEFAULT_PASSWORD`, password при логине должен быть сразу сменён (TODO).
- `/admin/*` UI и `/api/v1/admin/*` + `/api/v1/auth/*` (кроме login) — требуют auth.
- Logout инвалидирует session/token.
- 401 без `details`, 403 для авторизованных без прав.
- Rate limit на login: ≥ 5 неудач за минуту → блок IP на 10 минут (минимум).

## File uploads

- Допустимые MIME: `image/jpeg`, `image/png`, `image/webp`, `video/mp4`, `application/pdf` (для repair file). Все остальные — 400.
- Max size — из `MAX_UPLOAD_MB` (default 25). Превышение — 413.
- Имя файла на диске генерируется (`{guid}{ext}`). Оригинальное — только в БД.
- Path traversal: проверять что итоговый path внутри `UPLOADS_ROOT`.
- Загруженные файлы отдавать со статичных путей nginx, **никогда** не выполнять как код.
- Антивирус не обязателен в MVP, но логировать любые анормальные размеры/типы.

## Input validation

- На бэке через FluentValidation — для всех POST/PUT public + admin.
- На фронте через Zod — UX, не safety. Не доверять.
- Не возвращать stack trace в production.

## Secrets

- Только в `.env` (не коммитится). `.env.example` всегда в репо с placeholders.
- `JWT_SECRET` / cookie key — минимум 32 байта random. Уникален per environment.
- DB password — не дефолт, всегда меняется в prod.
- Никаких секретов в логах.

## Logging (Serilog)

Логируем:
- order created (id, без card-данных)
- repair request created (id)
- status changes (entity, old, new, admin id)
- API ошибки (без request body для auth)
- upload errors
- login failures (email + IP, без password)

Не логируем: passwords, tokens, JWT, secret keys, full uploaded file contents.

## HTTPS

- Production обязательно HTTPS (Let's Encrypt через nginx).
- HSTS заголовок включён.
- `Secure` + `HttpOnly` + `SameSite=Lax` для auth cookies.
- HTTP → HTTPS 301 редирект на уровне nginx.

## CORS

- В MVP frontend и backend — одна origin (через nginx). CORS жёсткий, только same-origin.
- Для dev — allow `http://localhost:3000`.

## Headers

Минимум в nginx: `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, базовый CSP (TODO).

## Container hardening

- Frontend production image запускается non-root пользователем `nextjs`; runtime root filesystem в compose read-only.
- Для frontend/backend/nginx в production включены `cap_drop: ALL`, `no-new-privileges`, `pids_limit`, memory/CPU limits и `ulimits`.
- Writable paths должны быть только явно разрешёнными volumes/tmpfs: uploads volume для backend/nginx и tmpfs для `/tmp` / Next.js cache.
- Не добавлять Docker socket, `privileged`, `network_mode: host` или bind mount application code в production compose.
- `API_URL` для Next.js должен указывать только на внутренний backend origin из allowlist (`http://backend:5000` в production; localhost только для dev).

## Edge controls

- Nginx применяет отдельные rate limits для frontend, API, admin и auth endpoints.
- Unsafe HTTP methods на frontend pages и `/uploads/` блокируются; POST/PUT/DELETE должны идти только в backend `/api/` endpoints.
- Подозрительные пути (`.env`, `.git`, WordPress/PHP probes, package/compose manifests и т.п.) логируются security-форматом и возвращают 404.
- Upload/API body limits задаются на edge; увеличивать их только вместе с backend validation.

## Incident recovery checklist

После подозрения на RCE/компрометацию контейнера:

1. Считать текущий image/container недоверенным, остановить его и не переиспользовать filesystem слои.
2. Ротировать все секреты из `.env`: `JWT_SECRET`, admin password, DB password, deploy tokens, registry credentials и любые внешние API keys. Значения секретов не выводить в логи и не коммитить.
3. Выполнить clean rebuild из проверенного commit: `docker compose -f docker-compose.prod.yml build --no-cache --pull` и затем поднять новые контейнеры.
4. Удалить старые скомпрометированные контейнеры/images после сохранения необходимых forensic artifacts: `docker compose -f docker-compose.prod.yml down --remove-orphans`, затем точечно удалить старые image IDs.
5. Проверить uploads volume на неожиданные executable/scripts и удалить вредоносные файлы.
6. На уровне VPS/host firewall ограничить egress контейнеров: разрешить только необходимые направления (например DNS, package registry во время build, внешние сервисы по необходимости), запретить исходящее сканирование приватных/публичных сетей.
