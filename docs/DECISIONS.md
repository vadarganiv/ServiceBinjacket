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

## Open (требует решения)

- D-Open-01: Auth механизм — cookie session vs JWT. Решить в TASK-012.
- D-Open-02: Хранение enum в БД — `int` vs `string`. Решить в TASK-007.
- D-Open-03: API client на фронте — fetch wrapper vs ky vs tanstack-query. Решить в TASK-008/009.

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
