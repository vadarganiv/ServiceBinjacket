# Architecture Decision Records

Однострочники в таблице. Подробные ADR — снизу, по шаблону.

| ID | Decision | Status |
|---|---|---|
| D-001 | Backend: ASP.NET Core Web API + C# | Accepted |
| D-002 | Frontend: Next.js + TypeScript + Tailwind + shadcn/ui | Accepted |
| D-003 | DB: PostgreSQL | Accepted |
| D-004 | MVP payments: cash-only (`CashOnDelivery`, `CashInStore`) | Accepted |
| D-005 | Payment data model готова к будущим card/bank методам, UI — нет | Accepted |
| D-006 | File storage: локальный volume в MVP; S3-compatible — позже | Accepted |
| D-007 | Deploy: VPS + Docker Compose + Nginx + Let's Encrypt | Accepted |
| D-008 | Primary public locale: `sq` | Accepted |
| D-009 | Secondary public locale: `en` | Accepted |
| D-010 | Locale routing: `/sq/...` + `/en/...`, `/` → `/sq` | Accepted |
| D-011 | Clean Architecture в backend (Domain/Application/Infrastructure/Api) | Accepted |
| D-012 | i18n на фронте: next-intl + JSON-файлы `messages/{sq,en}.json` | Accepted |
| D-013 | Fallback при пустом `en`-поле: возвращать `sq` (на Application слое) | Accepted |
| D-014 | Subagent set на старте: `backend-dev` (Sonnet), `frontend-dev` (Sonnet), `reviewer` (Opus) | Accepted |
| D-015 | Большой исходный ТЗ архивирован в `docs/_archive/`, не загружается рутинно | Accepted |
| D-016 | Enums в PostgreSQL хранятся как `string` (читаемо, не нужна миграция при новых значениях) | Accepted |
| D-017 | Backend integration tests: use-case unit tests с прямым InMemory DbContext вместо WebApplicationFactory+HTTP — из-за конфликта провайдеров EF Core при смешивании Npgsql+InMemory в одном DI-контейнере | Accepted |
| D-018 | Admin auth: JWT в HttpOnly cookie (Secure + SameSite=Lax). Logout инвалидируется через `AdminUser.LastLogoutAt` — при валидации токена `iat > LastLogoutAt`. | Accepted |

## Open (требует решения)

- ~~D-Open-01~~ → закрыто как D-018 (JWT в HttpOnly cookie).
- ~~D-Open-02~~ → закрыто как D-016 (string).
- D-Open-03: API client на фронте — fetch wrapper vs ky vs tanstack-query. Решить в TASK-009.

## ADR шаблон

```
## D-XXX: <title>

### Status
Proposed | Accepted | Superseded by D-YYY | Deprecated

### Context
Что заставило принимать решение, какие силы действуют.

### Decision
Что выбрали, в одном-двух предложениях.

### Consequences
Что это даёт, что усложняет, какие компромиссы.
```

Подробные ADR пишем только когда решение нетривиально или возможен реверс. Для одностроки в таблице — достаточно записи выше.
