# Deployment

## Local development

```bash
cp .env.example .env
docker compose up --build
```

- Frontend: `http://localhost:3000` (redirects to `/sq`)
- API: `http://localhost:5000/api/v1`
- Swagger UI: `http://localhost:5000/swagger`
- PostgreSQL: `localhost:5432`

For separate application processes, start only PostgreSQL with `docker compose up -d postgres`, then run the backend with .NET SDK 10 and the frontend with Node.js 24 as shown in the root README.

## Production prerequisites

- A Linux host with a supported Docker Engine and Compose plugin
- A domain pointing to the host
- HTTPS certificate provisioning
- A firewall exposing only the intended HTTP/HTTPS and administration ports
- Unique values for every secret and every value marked as an example
- A tested backup/restore location outside the application containers

Do not advertise or use the old HTTP-by-IP deployment as a public demo.

## Required environment values

Create `.env` from `.env.example` and replace all placeholders. At minimum, verify:

```env
POSTGRES_PASSWORD=<unique-random-value>
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=servis_binjaket;Username=servis_user;Password=<same-db-password>
JWT_SECRET=<at-least-32-random-bytes>
ADMIN_DEFAULT_EMAIL=<real-admin-email>
ADMIN_DEFAULT_PASSWORD=<unique-password-at-least-12-characters>
PUBLIC_SITE_URL=https://example.com
CORS_ORIGINS=https://example.com
PUBLIC_ADDRESS=<real-address>
PUBLIC_PHONE=<real-phone>
WHATSAPP_PHONE=<international-digits>
```

Production always rejects a missing, weak, or placeholder JWT secret. On an empty database it also fails closed unless the administrator bootstrap email and password are present and strong. After the first administrator has been created, remove `ADMIN_DEFAULT_EMAIL` and `ADMIN_DEFAULT_PASSWORD` from the runtime environment; the stored BCrypt hash remains in PostgreSQL.

## Deploy

```bash
git clone <repository-url> /opt/servis-binjaket
cd /opt/servis-binjaket
cp .env.example .env
# Edit .env without printing secrets to logs.

docker compose -f docker-compose.prod.yml build --pull
docker compose -f docker-compose.prod.yml up -d
```

The repository includes an Nginx HTTP configuration and a documented HTTPS block. Before public use, configure the real domain/certificate paths, enable port 443, redirect port 80 to HTTPS, and verify renewal. A platform-managed reverse proxy is also acceptable.

## Verify

```bash
docker compose -f docker-compose.prod.yml ps
curl --fail https://example.com/health
curl --fail https://example.com/api/v1/products
curl --head https://example.com/
docker compose -f docker-compose.prod.yml logs --tail=100 backend frontend nginx
```

Also test `/sq/products`, `/en/products`, repair submission without a file, repair submission with a valid image, administrator login/logout, and an authorized admin update.

## Update

1. Run `scripts/backup-db.sh` and back up the upload volume.
2. Fetch and review the intended release.
3. Run backend and frontend quality checks.
4. Rebuild with `docker compose -f docker-compose.prod.yml up -d --build`.
5. Repeat the smoke tests and inspect logs.

## Database backup and restore

```bash
bash scripts/backup-db.sh
bash scripts/restore-db.sh /backups/db-YYYYMMDD-HHMMSS.sql.gz
```

Test restoration periodically; an untested backup is not a recovery plan. Keep database and upload backups encrypted and outside the host's application volumes.

## Release checklist

1. `dotnet format --verify-no-changes` and `dotnet test` pass.
2. `npm run lint`, `npm run typecheck`, `npm run build`, and `npm audit --audit-level=high` pass.
3. Dependency audits contain no known vulnerabilities.
4. HTTPS, CORS, public metadata, business details, and cookie behavior match the real domain.
5. Placeholder accounts/data and stale screenshots are absent.
6. A backup exists before migrations run.
7. Post-deploy smoke tests pass.
