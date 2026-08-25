# Security

This document describes controls implemented in the repository and the remaining limitations. It is not a claim of independent security certification.

## Administrator authentication

- Passwords are hashed with BCrypt using work factor 12.
- An eight-hour JWT is stored in the `sb_admin_token` HttpOnly cookie.
- The cookie uses `SameSite=Lax`; `Secure` is enabled when the original request is HTTPS (after forwarded-header processing).
- Logout records `LastLogoutAt`; tokens issued at or before that timestamp are rejected.
- All `/api/v1/admin/*` controllers and the authenticated auth endpoints use `[Authorize]`.
- Login is limited to five attempts per IP in a ten-minute sliding window.
- Production startup always rejects a missing/placeholder JWT secret. Administrator bootstrap credentials are required and validated only while the database has no administrator; remove them from the runtime environment after the first successful bootstrap.

The default administrator is bootstrap functionality, not an account-management system. MFA, password reset/change UI, account lockout, refresh tokens, and explicit antiforgery tokens are not implemented. Password rotation currently requires an explicit administrative database operation followed by session invalidation; this limitation should be addressed before using the project for a real business.

## File uploads

Public repair attachments allow JPEG, PNG, WebP, MP4, and PDF. Product images allow JPEG, PNG, and WebP only.

Before anything is written, the API verifies:

1. A recognized extension is present.
2. The browser-provided MIME agrees with that extension.
3. Magic bytes agree with the expected format.
4. The per-file, file-count, combined-size, and transport limits are satisfied.

Stored names are generated GUIDs with a normalized trusted extension. `Path.GetFileName` strips client path components from the original display name. The public repair upload endpoint is also rate-limited.

Known limitation: this MVP stores uploads on a local volume and serves them from `/uploads`. Repair attachment URLs contain generated GUID names but are not protected by administrator authorization. Before accepting sensitive documents in a real deployment, move repair files to private object storage or an authenticated download endpoint and add malware scanning.

## Secrets and production configuration

- `.env` is ignored; `.env.example` contains placeholders only.
- Generate independent high-entropy JWT, administrator, and database credentials for each environment.
- Do not put production secrets into image layers, source control, logs, screenshots, or support messages.
- Use a managed secret store when the hosting platform provides one.
- HTTPS is required for a real public deployment; do not publish the HTTP-by-IP demo URL.

## HTTP boundary

Nginx sets baseline security headers, blocks unsafe methods on frontend/static routes, limits request rates and connections, and keeps PostgreSQL/backend ports private in the production Compose file. ASP.NET Core processes forwarded headers before authentication.

The current Content Security Policy still permits inline script/style behavior required by the present frontend setup. Tightening CSP with nonces is follow-up work.

## Dependency and CI controls

- NuGet audit warnings `NU1901` through `NU1904` are treated as errors.
- CI restores, checks formatting, tests the backend, lints/type-checks/builds the frontend, and runs `npm audit --audit-level=high`.
- Dependabot checks NuGet, npm, and GitHub Actions dependencies.

## Incident response outline

If a deployed container may be compromised:

1. Isolate it and preserve required forensic artifacts.
2. Rotate JWT, administrator, database, deployment, registry, and external-service credentials.
3. Rebuild from a reviewed commit with `--no-cache --pull`.
4. Inspect database and upload volumes before reuse.
5. Replace compromised containers/images and verify firewall and outbound-network rules.

Never paste secret values into logs while investigating.
