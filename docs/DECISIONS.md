# Architecture decisions

| ID | Decision | Status |
|---|---|---|
| D-001 | ASP.NET Core 10 controller-based Web API | Accepted |
| D-002 | Next.js 16 App Router, React, TypeScript, Tailwind CSS | Accepted |
| D-003 | PostgreSQL through EF Core/Npgsql | Accepted |
| D-004 | MVP payments are cash-only | Accepted |
| D-005 | Keep a payment model for future methods without exposing unfinished UI | Accepted |
| D-006 | Local Docker volume for current file storage; private object storage is follow-up work | Accepted |
| D-007 | Docker Compose and Nginx for the reference VPS topology | Accepted |
| D-008 | Albanian (`sq`) is the primary public locale; English (`en`) is secondary | Accepted |
| D-009 | Public routes use `/sq/*` and `/en/*`; admin routes have no locale prefix | Accepted |
| D-010 | Backend layers are Domain, Application, Infrastructure, and Api | Accepted |
| D-011 | Frontend localization uses next-intl and checked-in JSON messages | Accepted |
| D-012 | Empty English database fields fall back to Albanian in application mapping | Accepted |
| D-013 | Database enums use readable string conversions | Accepted |
| D-014 | Admin auth uses JWT in an HttpOnly, SameSite=Lax cookie | Accepted |
| D-015 | Logout invalidates issued tokens through `AdminUser.LastLogoutAt` | Accepted |
| D-016 | Public deployment links require HTTPS and non-placeholder data | Accepted |
| D-017 | Uploads require extension/MIME/signature agreement and bounded request limits | Accepted |

## Open decisions

- Move repair attachments behind an authenticated download boundary or private object storage.
- Add malware scanning before accepting sensitive customer documents.
- Choose a payment provider only when online payments enter scope.
- Decide whether administrator accounts need MFA and self-service password management.

For a non-trivial future decision, record context, the chosen option, alternatives, and consequences rather than only changing code.
