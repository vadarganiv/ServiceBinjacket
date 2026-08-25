# API

Base path: `/api/v1`. Localized public reads accept `?locale=sq|en`; `sq` is the default and missing English content falls back to Albanian.

## Public endpoints

| Method | Path | Purpose |
|---|---|---|
| GET | `/health` | Service health |
| GET | `/product-categories?locale=` | Published product categories |
| GET | `/products?...` | Filtered, sorted, paginated catalog |
| GET | `/products/{slug}?locale=` | Product detail |
| GET | `/service-categories?locale=` | Published service categories |
| GET | `/services?locale=&categoryId=` | Published services |
| GET | `/services/{slug}?locale=` | Service detail |
| POST | `/orders` | Create a cash order |
| POST | `/repair-requests` | Create a repair request |
| POST | `/repair-requests/{id}/files` | Attach repair files as multipart form data |

Repair uploads accept at most four files, 25 MB per file and 30 MB combined by default. Allowed types are JPEG, PNG, WebP, MP4, and PDF. Extension, MIME, and file signature must agree. Limits can be lowered through configuration but cannot exceed the transport safety caps.

## Authentication

| Method | Path | Purpose |
|---|---|---|
| POST | `/auth/login` | Validate credentials and set the HttpOnly auth cookie |
| POST | `/auth/logout` | Invalidate the current token and delete the cookie |
| GET | `/auth/me` | Return the authenticated administrator |

## Administrator endpoints

All routes below require the administrator cookie.

```text
GET    /admin/products
POST   /admin/products
GET    /admin/products/{id}
PUT    /admin/products/{id}
POST   /admin/products/{id}/publish
POST   /admin/products/{id}/unpublish
POST   /admin/products/{id}/images
DELETE /admin/products/{id}/images/{imageId}
GET    /admin/products/categories

GET    /admin/services
POST   /admin/services
GET    /admin/services/{id}
PUT    /admin/services/{id}
POST   /admin/services/{id}/publish
POST   /admin/services/{id}/unpublish
GET    /admin/services/categories

GET /admin/orders
GET /admin/orders/{id}
PUT /admin/orders/{id}/status
PUT /admin/orders/{id}/comment

GET /admin/repair-requests
GET /admin/repair-requests/{id}
PUT /admin/repair-requests/{id}/status
PUT /admin/repair-requests/{id}/comment
```

## Error envelope

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

Common codes include `VALIDATION_ERROR` (400), `UNAUTHORIZED` (401), `FORBIDDEN` (403), `NOT_FOUND` (404), `CONFLICT` (409), `FILE_TOO_LARGE`/`UPLOAD_TOO_LARGE` (413), and `RATE_LIMITED` (429).

Breaking contract changes require a new version prefix; existing `/v1` behavior should not be silently changed.
