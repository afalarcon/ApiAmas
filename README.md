# AMAS API

API ASP.NET Core en .NET 10 para AMAS, organizada con Clean Architecture:

- `Amas.Api`: controllers, Swagger, JWT, health checks.
- `Amas.Application`: DTOs, validadores, servicios y contratos.
- `Amas.Domain`: entidades de Identity, Core y Automation.
- `Amas.Infrastructure`: EF Core, PostgreSQL, repositorios y Redis cache.

## Endpoints iniciales

- `GET /health`
- `GET /api/v1/products`
- `GET /api/v1/products/{id}`
- `POST /api/v1/products`
- `PUT /api/v1/products/{id}`
- `DELETE /api/v1/products/{id}`
- `GET /api/v1/categories`
- `POST /api/v1/categories`
- `PUT /api/v1/categories/{id}`
- `DELETE /api/v1/categories/{id}`
- `GET /api/v1/categories/{id}/images`
- `POST /api/v1/categories/{id}/images`
- `GET /api/v1/catalogs`
- `GET /api/v1/catalogs/images`
- `POST /api/v1/catalogs/cache/warmup`
- `GET /api/v1/configurations`
- `PUT /api/v1/configurations/{key}`

## Cache Redis

La API consulta Redis antes de PostgreSQL en:

- Products: TTL 10 minutos.
- Categories: TTL 30 minutos.
- Category images: TTL 30 minutos por categoria.
- Catalogs: TTL 30 minutos, categorias activas con imagenes.
- Catalog images: TTL 30 minutos, imagenes agrupadas por categoria.
- Configurations: TTL 1 hora.

En escrituras se invalida la clave de lista correspondiente.

## Base de datos y migraciones

La estructura actual de PostgreSQL esta documentada en `docs/database-diagram.md`, incluyendo esquemas, relaciones principales y campos numericos visibles para busquedas operativas.

La HU de galeria de producto por categoria e imagen referencial de inventario esta documentada en `docs/hu-product-gallery-inventory-image.md`.

Para despliegues en staging o produccion se dejo generado un script idempotente:

```text
deploy/sql/amas_migrations_idempotent.sql
```

Uso recomendado antes de ejecutar en produccion:

1. Tomar backup de PostgreSQL.
2. Probar el script en staging contra una copia reciente.
3. Aplicar el SQL idempotente.
4. Validar `/health`, login, productos, proveedores, Kardex y cargue de facturas.

## Imagenes de categorias

La carga inicial usa storage local configurable y guarda metadata en PostgreSQL. Redis solo cachea la lectura que consume el front.

- `POST /api/v1/categories/{id}/images`: protegido con JWT, `multipart/form-data`.
- Campo para archivos: `files`.
- Campo opcional: `altText`.
- `GET /api/v1/categories/{id}/images`: publico, devuelve las URLs ordenadas desde cache/DB.

Ejemplo Angular:

```ts
const form = new FormData();
for (const file of files) {
  form.append('files', file);
}
form.append('altText', 'Imagen de categoria');

await http.post(`/api/v1/categories/${categoryId}/images`, form).toPromise();
```

Configuracion de storage:

```env
MediaStorage__Provider=Local
MediaStorage__LocalPath=storage/media
MediaStorage__PublicBaseUrl=/media
MediaStorage__MaxFileBytes=5242880
MediaStorage__AllowedContentTypes__0=image/jpeg
MediaStorage__AllowedContentTypes__1=image/png
MediaStorage__AllowedContentTypes__2=image/webp
```

Con esta configuracion los archivos se guardan en `api/storage/media/categories/{categoryId}/...` y la API los sirve desde `/media/categories/{categoryId}/...`. Para moverlo luego a S3, Azure Blob u otro storage, se cambia la implementacion de `IImageStorage` y se conserva el contrato HTTP.

## Catalogos cacheados para frontend

Para pantallas publicas, el front debe preferir los endpoints agregados:

- `GET /api/v1/catalogs`: trae categorias activas con sus imagenes en una sola respuesta.
- `GET /api/v1/catalogs/images`: trae todas las imagenes agrupadas por categoria.

Ambos endpoints consultan Redis primero. Si no hay cache, consultan PostgreSQL una vez, guardan el resultado con TTL y responden. Para precargar Redis despues de deploy, migraciones o carga masiva:

```http
POST /api/v1/catalogs/cache/warmup
Authorization: Bearer <token>
```

Las escrituras de categorias y la carga de imagenes invalidan `amas:catalogs:list` y `amas:catalogs:images`, de forma que el siguiente warmup o lectura reconstruye el cache con informacion vigente.

## Historia: lectura de facturas para Kardex

Esta seccion documenta el requerimiento antes de implementar. El objetivo es construir una carga asistida de facturas en PDF o imagen para precargar entradas de Kardex, comparar contra la informacion existente en PostgreSQL y permitir revision humana antes de afectar inventario.

### Historia de usuario

Como usuario administrador de inventario,
quiero subir una factura de compra o remision en PDF o imagen,
para que el sistema lea productos, cantidades y valores, los compare contra los productos, insumos o elementos existentes, y me permita completar o corregir la informacion antes de registrar entradas en Kardex.

### Objetivo

- Reducir digitacion manual al registrar entradas de inventario.
- Detectar coincidencias entre lineas de factura y datos existentes en base de datos.
- Evitar que una lectura automatica afecte inventario sin confirmacion del usuario.
- Dejar trazabilidad del archivo, usuario, lineas leidas, correcciones y movimientos generados.

### Flujo funcional propuesto

1. El usuario entra a `Kardex > Cargar factura`.
2. Sube un archivo PDF o imagen.
3. La API crea una sesion de importacion y guarda el archivo en storage configurable.
4. El sistema extrae datos principales:
   - Proveedor.
   - Numero de factura.
   - Fecha.
   - Productos o insumos.
   - Cantidades.
   - Valor unitario.
   - IVA, descuentos y total cuando sea posible.
5. El sistema compara cada linea contra inventario existente usando:
   - SKU exacto.
   - Nombre exacto o similar.
   - Codigo de proveedor cuando exista.
   - Tipo de item: producto, insumo o elemento.
6. El usuario revisa una tabla de precarga:
   - Coincidencia exacta.
   - Coincidencia probable.
   - Sin coincidencia.
   - Requiere revision.
7. El usuario puede asociar una linea a un item existente, crear un nuevo item, completar campos faltantes, ignorar una linea o dejarla pendiente.
8. El usuario confirma la importacion.
9. Solo al confirmar se crean movimientos de entrada en Kardex.

### Reglas de negocio

- La lectura de la factura no debe modificar stock automaticamente.
- PostgreSQL sigue siendo la fuente real de datos.
- Redis puede cachear catalogos o items para acelerar comparaciones, pero no debe guardar decisiones finales.
- Toda entrada creada desde factura debe quedar auditada.
- Las lineas sin coincidencia no deben bloquear toda la factura si el usuario decide importarlas parcialmente.
- Si el total calculado no coincide con el total leido de la factura, se debe mostrar advertencia antes de confirmar.

### Estados sugeridos

- `Uploaded`: archivo recibido.
- `Processing`: archivo en lectura.
- `Processed`: lectura terminada.
- `NeedsReview`: hay lineas incompletas o ambiguas.
- `ReadyToConfirm`: todas las lineas seleccionadas estan validadas.
- `Confirmed`: se generaron movimientos Kardex.
- `Cancelled`: el usuario cancelo la importacion.
- `Failed`: no fue posible leer el archivo.

### Endpoints candidatos

```http
POST /api/v1/inventory/invoices/upload
GET /api/v1/inventory/invoices/imports/{id}
PUT /api/v1/inventory/invoices/imports/{id}/lines/{lineId}
POST /api/v1/inventory/invoices/imports/{id}/confirm
POST /api/v1/inventory/invoices/imports/{id}/cancel
```

### Tablas candidatas

- `inventory_invoice_imports`: sesion de importacion, proveedor, numero, fecha, estado y totales.
- `inventory_invoice_import_lines`: lineas leidas, datos extraidos, item asociado, confianza y estado.
- `inventory_invoice_files`: archivo original, storage provider, ruta, hash y metadata.
- `supplier_product_mappings`: relacion opcional entre codigos de proveedor e items internos.
- `audit_logs`: trazabilidad de decisiones y confirmacion.

### Estrategia tecnica por fases

Fase 1, MVP controlado:

- Soportar PDF con texto embebido y carga manual asistida.
- Guardar archivo y sesion de importacion.
- Comparar por SKU y nombre.
- Permitir asociar lineas a items existentes.
- Permitir completar datos faltantes.
- Confirmar para crear movimientos de entrada.

Fase 2:

- Agregar OCR/vision para imagenes y PDF escaneado con OpenAI.
- Mejorar comparacion por similitud.
- Agregar proveedor y mapeos proveedor-item.

Fase 3:

- Evaluar servicio externo de document intelligence para facturas variadas.
- Validar factura electronica en XML si el proveedor la entrega.
- Reglas avanzadas de impuestos, descuentos y conciliacion de totales.

### Criterios de aceptacion iniciales

- El usuario puede subir una factura sin afectar inventario.
- El sistema muestra una vista de revision con las lineas detectadas.
- El sistema indica coincidencias contra productos, insumos o elementos existentes.
- El usuario puede corregir o completar lineas antes de confirmar.
- Al confirmar se crean entradas Kardex solo para lineas validadas.
- El sistema muestra advertencias por diferencias de total o lineas incompletas.
- La importacion queda auditada con usuario, fecha, archivo y decisiones.

### Decisiones pendientes

- Definir tipos de archivo aceptados en MVP: PDF, JPG, PNG, WEBP.
- Definir si se requiere tabla `suppliers` desde la primera version.
- Definir tolerancia para coincidencias por nombre.
- Definir si las lineas sin coincidencia pueden crear items nuevos en el mismo flujo.
- Definir proveedor tecnico para OCR o document intelligence.

### Implementacion actual de lectura con OpenAI

La API ya incluye un lector configurable para facturas en `PDF`, `JPG`, `PNG` y `WEBP`.
El flujo guarda el archivo, llama a OpenAI cuando esta habilitado, almacena el JSON extraido en PostgreSQL y precarga las lineas para revision antes de crear entradas Kardex.

Variables requeridas:

```text
OpenAI__Enabled=true
OpenAI__ApiKey=sk-...
OpenAI__Model=gpt-4.1-mini
OpenAI__ResponsesUrl=https://api.openai.com/v1/responses
```

Si `OpenAI__Enabled=false` o falta `OpenAI__ApiKey`, la factura se carga igual y queda para revision manual con JSON de placeholder. Si OpenAI responde sin cuota o por limite de uso, la API no falla el cargue: registra el estado y el front muestra la barra de extractor como no disponible.

Endpoint de estado:

```http
GET /api/v1/inventory/invoices/extractor/status
```

Nota: no se consulta saldo real de la cuenta desde la API key normal. La disponibilidad se infiere por configuracion y por el ultimo resultado de extraccion, especialmente errores de cuota o limite.

## Configuracion local

El desarrollo local debe usar una base local separada del VPS:

```text
Local:      amas_local
Tests:      repositorios fake o amas_tests si se requiere integracion
Staging:    amas_core_staging
Produccion: amas_core
```

Levantar dependencias locales:

```bash
docker compose -f docker-compose.local.yml up -d
```

Copiar variables locales:

```bash
cp .env.example .env
```

No usar la base del VPS para crear productos desde el entorno local. Tailscale queda reservado para mantenimiento puntual, diagnostico o migraciones controladas.

Ejecutar con .NET:

```bash
dotnet restore Amas.slnx
dotnet ef database update --project Amas.Infrastructure --startup-project Amas.Api
dotnet run --project Amas.Api --no-launch-profile
```

Swagger queda en:

```text
http://localhost:8080/swagger
```

## Docker en VPS

La API escucha internamente en `8080` y se conecta a las redes externas `amas_net` y `traefik_proxy`.

```bash
cp deploy/env/production.env.example .env
docker compose up -d --build
```

Ejecutar migraciones desde el host o desde un contenedor SDK apuntando a la misma red. Ejemplo local:

```bash
dotnet ef database update --project Amas.Infrastructure --startup-project Amas.Api
```

## Variables requeridas

Local:

```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__Postgres=Host=localhost;Port=55432;Database=amas_local;Username=amas_local_user;Password=amas_local_password
Redis__Connection=localhost:56379
Redis__Password=amas_local_redis_password
Jwt__Issuer=amas-api-local
Jwt__Audience=amas-client-local
Jwt__Secret=CAMBIAR_SECRET_LOCAL_MINIMO_32_CARACTERES
Jwt__ExpirationMinutes=60
Auth__AdminEmail=admin@amas.local
Auth__AdminPassword=CAMBIAR_PASSWORD_LOCAL
MediaStorage__Provider=Local
MediaStorage__LocalPath=storage/media
MediaStorage__PublicBaseUrl=/media
MediaStorage__MaxFileBytes=5242880
Swagger__Enabled=true
Cors__AllowedOrigins__0=http://localhost:4200
Cors__AllowedOrigins__1=http://127.0.0.1:4200
RateLimiting__GlobalPermitLimit=240
RateLimiting__GlobalWindowSeconds=60
RateLimiting__LoginPermitLimit=5
RateLimiting__LoginWindowSeconds=60
RateLimiting__ContactPermitLimit=5
RateLimiting__ContactWindowSeconds=60
ContactWebhook__Enabled=false
ContactWebhook__Url=
ContactWebhook__Secret=
ContactWebhook__TimeoutSeconds=8
```

Produccion:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__Postgres=Host=amas-postgres;Port=5432;Database=amas_core;Username=amas_user;Password=CAMBIAR_PASSWORD
Redis__Connection=amas-redis:6379
Redis__Password=CAMBIAR_PASSWORD
Jwt__Issuer=amas-api
Jwt__Audience=amas-client
Jwt__Secret=CAMBIAR_SECRET_LARGO_Y_SEGURO
Jwt__ExpirationMinutes=60
MediaStorage__Provider=Local
MediaStorage__LocalPath=storage/media
MediaStorage__PublicBaseUrl=/media
MediaStorage__MaxFileBytes=5242880
Swagger__Enabled=false
Cors__AllowedOrigins__0=https://amaslohaceposible.cloud
RateLimiting__GlobalPermitLimit=120
RateLimiting__GlobalWindowSeconds=60
RateLimiting__LoginPermitLimit=5
RateLimiting__LoginWindowSeconds=60
RateLimiting__ContactPermitLimit=3
RateLimiting__ContactWindowSeconds=60
ContactWebhook__Enabled=false
ContactWebhook__Url=https://n8n.example.com/webhook/amas-contact
ContactWebhook__Secret=CAMBIAR_WEBHOOK_SECRET
ContactWebhook__TimeoutSeconds=8
```

## Seguridad inicial

- Publicos: `GET /health`, `GET /api/v1/products`, `GET /api/v1/categories`, `GET /api/v1/configurations`, `POST /api/v1/contact-requests`.
- Protegidos con JWT Bearer: `POST/PUT/DELETE products`, `POST/PUT/DELETE categories`, `POST category images`, `PUT configurations`.
- Swagger queda activo en `Development`. En `Production` solo se habilita si `Swagger__Enabled=true`.
- `api/.env` contiene secretos locales y no debe subirse a Git.
- El login tiene rate limiting por IP: 5 intentos por minuto por defecto. La API responde `429` cuando se excede.
- El formulario de contacto tiene rate limiting independiente y guarda la IP como hash SHA-256.
- El webhook de contacto es opcional. Si falla, la solicitud queda guardada con estado `WebhookPending`.
- No se habilita directory browsing. Los archivos de `/media` se sirven como archivos estáticos, sin listado de directorios.

## Traefik

No se usan labels Docker. El ejemplo versionado esta en `deploy/traefik/amas-api.yml`.
Copiarlo al VPS como:

```text
/opt/reverse-proxy/dynamic/amas-api.yml
```

```yaml
http:
  routers:
    amas-api:
      rule: "Host(`api.amaslohaceposible.cloud`)"
      entryPoints:
        - websecure
      tls:
        certResolver: le
      service: amas-api

  services:
    amas-api:
      loadBalancer:
        servers:
          - url: "http://amas-api:8080"
```
