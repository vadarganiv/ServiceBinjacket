# API

Base: `/api/v1`. Все localized GET'ы принимают `?locale=sq|en` (default `sq`, fallback `en→sq`).

## Public

| Method | Path | Описание |
|---|---|---|
| GET | `/health` | health check, без auth |
| GET | `/product-categories?locale=` | список категорий |
| GET | `/products?locale=&categoryId=&q=&minPrice=&maxPrice=&condition=&inStock=&sort=&page=&pageSize=` | список товаров с фильтрацией |
| GET | `/products/{slug}?locale=` | детали товара |
| GET | `/service-categories?locale=` | список категорий услуг |
| GET | `/services?locale=&categoryId=` | список услуг |
| GET | `/services/{slug}?locale=` | детали услуги |
| POST | `/orders` | создать заказ (cash-only) |
| POST | `/repair-requests` | создать заявку на ремонт |
| POST | `/repair-requests/{id}/files` | загрузить файлы к заявке (multipart) |

## Auth (admin)

| Method | Path | Описание |
|---|---|---|
| POST | `/auth/login` | `{email, password}` → cookie/token |
| POST | `/auth/logout` | invalidate session |
| GET | `/auth/me` | текущий admin |

## Admin — products

```
GET    /admin/products                              ?q=&isPublished=&page=&pageSize=
POST   /admin/products                              create
GET    /admin/products/{id}
PUT    /admin/products/{id}                          update (включая localized fields)
POST   /admin/products/{id}/publish
POST   /admin/products/{id}/unpublish
POST   /admin/products/{id}/images                   multipart
DELETE /admin/products/{id}/images/{imageId}
```

## Admin — services

```
GET    /admin/services                              ?q=&isPublished=
POST   /admin/services
GET    /admin/services/{id}
PUT    /admin/services/{id}
POST   /admin/services/{id}/publish
POST   /admin/services/{id}/unpublish
```

## Admin — orders / repairs

```
GET /admin/orders                                   ?status=&page=&pageSize=
GET /admin/orders/{id}
PUT /admin/orders/{id}/status                       {status}
PUT /admin/orders/{id}/comment                      {comment}

GET /admin/repair-requests                          ?status=&page=&pageSize=
GET /admin/repair-requests/{id}
PUT /admin/repair-requests/{id}/status              {status}
PUT /admin/repair-requests/{id}/comment             {comment}
```

## Error format (единый)

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      { "field": "phone", "message": "Phone is required" }
    ]
  }
}
```

Стандартные `code`:
- `VALIDATION_ERROR` — 400
- `UNAUTHORIZED` — 401
- `FORBIDDEN` — 403
- `NOT_FOUND` — 404
- `CONFLICT` — 409
- `INTERNAL_ERROR` — 500 (без `details`, message сокращён)

## DTO примеры

`ProductListItemDto` (localized):

```json
{
  "id": "...",
  "slug": "iphone-12-128gb",
  "name": "iPhone 12 128GB",
  "shortDescription": "...",
  "price": 45000,
  "currency": "ALL",
  "condition": "Used",
  "image": { "path": "/uploads/...", "alt": "..." },
  "inStock": true
}
```

`OrderCreateDto`:

```json
{
  "customer": { "fullName": "...", "phone": "+355...", "whatsAppPhone": null, "city": "Durrës", "address": "..." },
  "deliveryMethod": "LocalCourier",
  "paymentMethod": "CashOnDelivery",
  "items": [{ "productId": "...", "quantity": 1 }],
  "customerComment": null,
  "consent": true
}
```

`RepairRequestCreateDto`:

```json
{
  "customer": { "fullName": "...", "phone": "+355...", "city": "Durrës" },
  "serviceId": null,
  "deviceType": "phone",
  "brand": "Apple",
  "model": "iPhone 12",
  "problemDescription": "...",
  "preferredDeliveryMethod": "StorePickup",
  "customerComment": null,
  "consent": true
}
```

## Versioning

- Все public endpoints — под `/api/v1`. Breaking changes — новый `/v2`, не ломать `/v1` до явного решения в `DECISIONS.md`.

## Swagger

- Включён только в `Development` + опционально в `Staging`. В production — отключён или защищён.
