# Security

MVP-уровень, но всерьёз.

## Admin auth

- Passwords хэшируются (BCrypt cost ≥ 12 или Argon2id). Никаких plaintext.
- Default admin создаётся при первом запуске из `ADMIN_DEFAULT_EMAIL` / `ADMIN_DEFAULT_PASSWORD`, password при логине должен быть сразу сменён (TODO в TASK-012).
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

Минимум в nginx: `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, базовый CSP (TODO в TASK-014).
