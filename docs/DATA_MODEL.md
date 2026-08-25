# Data model

## Main entities

| Entity | Purpose | Important fields |
|---|---|---|
| `Customer` | Buyer or repair requester | Contact and address details |
| `ProductCategory` | Localized catalog category | Names, slugs, sort/publish state |
| `Product` | Sellable item | Localized content, price, condition, stock, category, warranty |
| `ProductImage` | Product media | Storage path, localized alt text, order |
| `ServiceCategory` | Localized repair category | Names, slugs, sort/publish state |
| `Service` | Repair service | Localized content, price note, category, publish state |
| `RepairRequest` | Customer repair intake | Device/problem/delivery details, status, comments |
| `RepairRequestFile` | Repair attachment metadata | Path, safe display name, trusted MIME, byte size |
| `Order` | Cash order | Customer, delivery/payment/status, subtotal, comments |
| `OrderItem` | Order line snapshot | Product, name/price snapshot, quantity |
| `Payment` | Future payment record | Owner, method, status, amount, date |
| `AdminUser` | Back-office identity | Email, BCrypt hash, active/login/logout timestamps |

## Relationships

- Product category 1:N products; product 1:N images.
- Service category 1:N services.
- Customer 1:N orders and 1:N repair requests.
- Order 1:N order items; each item references a product and preserves name/price snapshots.
- Repair request N:0..1 service and 1:N attachment metadata.
- A payment can belong to an order or repair request when that feature is implemented.

## Localized fields

Public content uses paired Albanian/English columns such as `NameSq`/`NameEn`, `SlugSq`/`SlugEn`, and `DescriptionSq`/`DescriptionEn`. The API returns a locale-selected DTO rather than exposing both persistence fields. Missing English values fall back to Albanian.

## Conventions

- Enum properties use EF Core string conversion.
- Currency defaults to Albanian lek (`ALL`).
- Timestamps are UTC.
- English localized fields can be nullable; Albanian primary content is required where configured.
- Slugs are unique within their entity type.
- Migrations are the authoritative schema history; avoid manual production DDL.
