# Data Model

## Сущности (MVP)

| Entity | Назначение | Ключевые поля |
|---|---|---|
| `Customer` | покупатель / автор заявки | `Id`, `FullName`, `Phone`, `WhatsAppPhone?`, `Email?`, `City`, `Address?`, `CreatedAt` |
| `ProductCategory` | категория товаров | `Id`, локализованные `Name*`/`Slug*`, `ParentId?`, `SortOrder`, `IsPublished` |
| `Product` | товар | `Id`, локализованные `Name*`/`Slug*`/`ShortDescription*`/`Description*`, `Price`, `Currency`, `Condition` (enum), `StockQty?`, `CategoryId`, `WarrantyMonths?`, `IsPublished`, `CreatedAt` |
| `ProductImage` | картинка товара | `Id`, `ProductId`, `Path`, `Alt*` локализованный, `SortOrder` |
| `ServiceCategory` | категория услуг ремонта | `Id`, локализованные `Name*`/`Slug*`, `SortOrder`, `IsPublished` |
| `Service` | услуга ремонта | `Id`, локализованные `Name*`/`Slug*`/`ShortDescription*`/`Description*`, `PriceNote*` локализованный, `CategoryId`, `IsPublished` |
| `RepairRequest` | заявка на ремонт | `Id`, `CustomerId`, `ServiceId?`, `DeviceType`, `Brand?`, `Model?`, `ProblemDescription`, `PreferredDeliveryMethod` (enum), `CustomerComment?`, `Status` (enum), `AdminComment?`, `CreatedAt`, `UpdatedAt` |
| `RepairRequestFile` | файл к заявке (фото/видео) | `Id`, `RepairRequestId`, `Path`, `OriginalName`, `MimeType`, `SizeBytes`, `UploadedAt` |
| `Order` | заказ товара | `Id`, `CustomerId`, `DeliveryMethod` (enum), `PaymentMethod` (enum), `Status` (enum), `Subtotal`, `Currency`, `CustomerComment?`, `AdminComment?`, `CreatedAt`, `UpdatedAt` |
| `OrderItem` | позиция в заказе | `Id`, `OrderId`, `ProductId`, `NameSnapshot`, `PriceSnapshot`, `Quantity` |
| `Payment` | факт оплаты (заготовка) | `Id`, `OrderId?` или `RepairRequestId?`, `Method` (enum), `Status` (enum), `Amount`, `Currency`, `PaidAt?`, `Notes?` |
| `AdminUser` | админ | `Id`, `Email`, `PasswordHash`, `DisplayName`, `IsActive`, `CreatedAt`, `LastLoginAt?` |

`*` означает пару `*Sq` / `*En` (см. ниже).

## Локализованные поля

Для public-контента (ProductCategory, Product, ServiceCategory, Service, ProductImage.Alt):

```
NameSq            NameEn
SlugSq            SlugEn
ShortDescriptionSq  ShortDescriptionEn
DescriptionSq     DescriptionEn
PriceNoteSq       PriceNoteEn        (только для Service)
AltSq             AltEn              (только для ProductImage)
```

### Fallback rule

```
locale=sq          => Sq поля
locale=en          => En поля; если En пусто → Sq
locale отсутствует => sq
```

Реализовать в Application слое в маппере DTO. **Не** реализовывать в контроллерах или EF query.

### Slugs

- Уникальны в рамках своей сущности.
- Генерация: предпочтительно admin задаёт вручную; если пусто — auto из `Name` (lowercase, dashes, без диакритики). Для albanian-символов (`ç`, `ë`) — `c`, `e`.

## Enums (C#)

```csharp
public enum ProductCondition  { New, Used, Refurbished, Unknown }
public enum DeliveryMethod    { StorePickup, LocalCourier, PostalShipping, ManualAgreement }
public enum PaymentMethod     { CashOnDelivery, CashInStore, BankCardOnline, BankTransfer }
public enum PaymentStatus     { Pending, Paid, Failed, Cancelled, Refunded }
public enum OrderStatus       { New, Confirmed, Preparing, OutForDelivery, SentByPost, Delivered, Completed, Cancelled }
public enum RepairStatus      { New, Contacted, WaitingForDevice, Received, Diagnostics, PriceOffered, PriceAgreed, Repairing, Ready, SentBack, Completed, Cancelled }
```

В БД enums хранить как `int` (см. `HasConversion<int>()` в EF configuration) или как `string` — выбрать единообразно в первой миграции и зафиксировать в `DECISIONS.md`.

## Relationships

- `Product` N:1 `ProductCategory`; `Product` 1:N `ProductImage`.
- `Service` N:1 `ServiceCategory`.
- `Order` N:1 `Customer`; `Order` 1:N `OrderItem` → N:1 `Product`.
- `RepairRequest` N:1 `Customer`; N:0..1 `Service`; 1:N `RepairRequestFile`.
- `Payment` 0..1:1 `Order` или `RepairRequest` (один из двух).

## Migrations

- Через EF Core migrations. Никакого ручного DDL.
- Первая миграция — все таблицы + seed для категорий и базовых сервисов.
- Все локализованные поля — `nvarchar` / `text`, nullable у En.
- Currency по умолчанию `ALL`.
- Audit поля (`CreatedAt`, `UpdatedAt`) — UTC, обновлять в `DbContext.SaveChangesAsync`.
