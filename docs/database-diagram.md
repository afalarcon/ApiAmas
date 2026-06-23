# AMAS API - Diagrama de base de datos

Este documento resume la estructura actual de PostgreSQL para AMAS API y debe actualizarse cuando se agreguen nuevas migraciones.

## Principios

- La llave primaria interna de las entidades principales sigue siendo `Id` tipo `uuid`.
- Los campos visibles para busqueda y operacion son numericos y unicos:
  - `CategoryNumber`
  - `ProductNumber`
  - `InventoryItemNumber`
  - `InventoryMovementNumber`
  - `InvoiceImportNumber`
  - `SupplierNumber`
  - `ContactRequestNumber`
- Las secuencias numericas inician en `1001`.
- PostgreSQL es la fuente principal de datos.
- Redis se usa como cache y no debe ser tratado como fuente de verdad.
- El script idempotente de migraciones para despliegue esta en `deploy/sql/amas_migrations_idempotent.sql`.

## Esquemas

- `identity`: usuarios, roles, permisos y tokens.
- `core`: catalogos, productos, inventario, proveedores, facturas, clientes, cotizaciones, ordenes y configuraciones.
- `automation`: eventos, notificaciones y auditoria.

## Diagrama ER

```mermaid
erDiagram
    USERS {
        uuid Id PK
        string Email UK
        string PasswordHash
        string FullName
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }

    ROLES {
        uuid Id PK
        string Name UK
        string Description
        datetime CreatedAt
        datetime UpdatedAt
    }

    PERMISSIONS {
        uuid Id PK
        string Code UK
        string Description
        datetime CreatedAt
        datetime UpdatedAt
    }

    USER_ROLES {
        uuid UserId PK,FK
        uuid RoleId PK,FK
    }

    ROLE_PERMISSIONS {
        uuid RoleId PK,FK
        uuid PermissionId PK,FK
    }

    REFRESH_TOKENS {
        uuid Id PK
        uuid UserId FK
        string TokenHash UK
        datetime ExpiresAt
        datetime RevokedAt
        datetime CreatedAt
    }

    CATEGORIES {
        uuid Id PK
        bigint CategoryNumber UK
        string Name
        string Slug UK
        string Description
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }

    CATEGORY_IMAGES {
        uuid Id PK
        uuid CategoryId FK
        string Url
        string StoragePath
        string StorageProvider
        string FileName
        string ContentType
        string AltText
        int SortOrder
        datetime CreatedAt
        datetime UpdatedAt
    }

    PRODUCTS {
        uuid Id PK
        bigint ProductNumber UK
        uuid CategoryId FK
        string Name
        string Slug UK
        string Sku UK
        decimal Price
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }

    PRODUCT_IMAGES {
        uuid Id PK
        uuid ProductId FK
        string Url
        string AltText
        int SortOrder
        datetime CreatedAt
        datetime UpdatedAt
    }

    INVENTORY_ITEMS {
        uuid Id PK
        bigint InventoryItemNumber UK
        uuid ProductId FK
        string Name
        string Sku UK
        string Type
        string Unit
        decimal CurrentStock
        decimal MinimumStock
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }

    INVENTORY_MOVEMENTS {
        uuid Id PK
        bigint InventoryMovementNumber UK
        uuid InventoryItemId FK
        string MovementType
        decimal Quantity
        decimal StockAfter
        decimal UnitCost
        string Reason
        string Reference
        datetime OccurredAt
        datetime CreatedAt
        datetime UpdatedAt
    }

    SUPPLIERS {
        uuid Id PK
        bigint SupplierNumber UK
        uuid CategoryId FK
        string Name
        string TaxId UK
        string ContactName
        string Email
        string Phone
        string Address
        string City
        string Country
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }

    INVENTORY_INVOICE_IMPORTS {
        uuid Id PK
        bigint InvoiceImportNumber UK
        uuid SupplierId FK
        string Status
        string OriginalFileName
        string StoredFileName
        string StorageProvider
        string StoragePath
        string Url
        string SupplierName
        string SupplierTaxId
        string InvoiceNumber
        decimal Subtotal
        decimal TaxTotal
        decimal Total
        jsonb ExtractedJson
        string ExtractionProvider
        datetime CreatedAt
        datetime UpdatedAt
    }

    INVENTORY_INVOICE_IMPORT_LINES {
        uuid Id PK
        uuid InventoryInvoiceImportId FK
        uuid InventoryItemId FK
        int LineNumber
        string Status
        string MatchStatus
        string ExtractedSku
        string ExtractedName
        decimal Quantity
        decimal UnitCost
        decimal TaxPercent
        decimal TaxAmount
        decimal LineTotal
        datetime CreatedAt
        datetime UpdatedAt
    }

    CUSTOMERS {
        uuid Id PK
        string FullName
        string Email
        string Phone
        datetime CreatedAt
        datetime UpdatedAt
    }

    QUOTES {
        uuid Id PK
        string Status
        decimal Total
        datetime CreatedAt
        datetime UpdatedAt
    }

    ORDERS {
        uuid Id PK
        string Status
        decimal Total
        datetime CreatedAt
        datetime UpdatedAt
    }

    CONFIGURATIONS {
        uuid Id PK
        string Key UK
        string Value
        string Description
        datetime CreatedAt
        datetime UpdatedAt
    }

    CONTACT_REQUESTS {
        uuid Id PK
        bigint ContactRequestNumber UK
        string FullName
        string Email
        string Phone
        string RequestType
        string Message
        string SourcePage
        string Status
        string IpAddressHash
        string UserAgent
        string CaptchaProvider
        bool CaptchaTokenProvided
        bool WebhookDelivered
        datetime WebhookDeliveredAt
        string WebhookError
        datetime CreatedAt
        datetime UpdatedAt
    }

    WORKFLOW_EVENTS {
        uuid Id PK
        string Name
        jsonb PayloadJson
        datetime CreatedAt
        datetime UpdatedAt
    }

    NOTIFICATIONS {
        uuid Id PK
        string Channel
        string Recipient
        string Message
        datetime CreatedAt
        datetime UpdatedAt
    }

    AUDIT_LOGS {
        uuid Id PK
        string Actor
        string Action
        string EntityName
        string EntityId
        jsonb DetailsJson
        datetime CreatedAt
        datetime UpdatedAt
    }

    USERS ||--o{ USER_ROLES : has
    ROLES ||--o{ USER_ROLES : assigned
    ROLES ||--o{ ROLE_PERMISSIONS : grants
    PERMISSIONS ||--o{ ROLE_PERMISSIONS : included
    USERS ||--o{ REFRESH_TOKENS : owns

    CATEGORIES ||--o{ PRODUCTS : groups
    CATEGORIES ||--o{ CATEGORY_IMAGES : has
    CATEGORIES ||--o{ SUPPLIERS : classifies
    PRODUCTS ||--o{ PRODUCT_IMAGES : has
    PRODUCTS ||--o| INVENTORY_ITEMS : tracks
    INVENTORY_ITEMS ||--o{ INVENTORY_MOVEMENTS : records

    SUPPLIERS ||--o{ INVENTORY_INVOICE_IMPORTS : issues
    INVENTORY_INVOICE_IMPORTS ||--o{ INVENTORY_INVOICE_IMPORT_LINES : contains
    INVENTORY_ITEMS ||--o{ INVENTORY_INVOICE_IMPORT_LINES : matches
```

## Migraciones actuales

| Orden | Migracion | Proposito |
| --- | --- | --- |
| 1 | `20260603033801_InitialCreate` | Estructura base de `identity`, `core` y `automation`. |
| 2 | `20260604000000_AddCategoryImages` | Imagenes asociadas a categorias. |
| 3 | `20260605000000_AddIdentityRolesPermissions` | Roles, permisos y relacion con usuarios. |
| 4 | `20260610171313_AddInventory` | Kardex: items de inventario y movimientos. |
| 5 | `20260611170105_AddInvoiceImports` | Cargue de facturas de inventario. |
| 6 | `20260611191221_AddInvoiceExtractionJson` | JSON extraido de facturas. |
| 7 | `20260611203000_SeedInventoryPermissions` | Permisos de inventario y facturas. |
| 8 | `20260612194106_AddSuppliers` | Administracion de proveedores y relacion con facturas. |
| 9 | `20260616201951_AddVisibleNumbers` | Numeros visibles autoincrementales sin reemplazar los `uuid`. |
| 10 | `20260617201332_AddProductGalleryAndInventoryImage` | Galeria por producto/categoria e imagen identificadora de inventario. |
| 11 | `20260618223140_AddContactRequests` | Solicitudes de contacto publicas con estado de webhook. |

## Uso recomendado en produccion

1. Tomar backup de PostgreSQL antes de aplicar cambios.
2. Probar el script en staging contra una copia reciente.
3. Ejecutar `deploy/sql/amas_migrations_idempotent.sql` en produccion.
4. Validar `/health`, login, productos, proveedores, kardex, cargue de facturas y `POST /api/v1/contact-requests`.
5. No ejecutar scripts locales contra produccion desde `.env` de desarrollo.
