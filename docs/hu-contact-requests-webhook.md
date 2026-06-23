# HU - Formulario publico de contacto y webhook

## Objetivo

Permitir que un visitante de la landing envie una solicitud de contacto sin depender de correo SMTP. La API debe guardar la solicitud en PostgreSQL y luego disparar un webhook configurable para que n8n u otra herramienta gestione la notificacion por WhatsApp, correo, Telegram u otro canal.

## Flujo esperado

1. El front envia `POST /api/v1/contact-requests`.
2. La API valida campos, honeypot y rate limit publico.
3. La API guarda la solicitud en `core.contact_requests`.
4. La API responde al usuario con mensaje de recibido.
5. Si `ContactWebhook__Enabled=true`, la API envia un evento `contact.created` al webhook configurado.
6. Si el webhook falla, la solicitud queda guardada con estado `WebhookPending`.

## Endpoint

`POST /api/v1/contact-requests`

Payload:

```json
{
  "fullName": "Cliente AMAS",
  "email": "cliente@example.com",
  "phone": "3101234567",
  "requestType": "Producto personalizado",
  "message": "Quiero cotizar un producto personalizado.",
  "sourcePage": "/",
  "captchaToken": "token-opcional",
  "website": null
}
```

Respuesta:

```json
{
  "succeeded": true,
  "data": {
    "contactRequestNumber": 1001,
    "status": "New",
    "receivedAt": "2026-06-18T00:00:00Z"
  },
  "error": null
}
```

## Seguridad implementada

- Endpoint anonimo con politica de rate limit `contact-public`.
- Validacion server-side de longitudes, email, tipo de solicitud y mensaje minimo.
- Honeypot `website`: si llega diligenciado se simula exito y no se guarda la solicitud.
- IP almacenada como SHA-256, no en texto plano.
- User-Agent truncado.
- Mensajes de error simples para el front.
- Webhook con secreto opcional en header `X-Amas-Webhook-Secret`.

## Configuracion

```env
RateLimiting__ContactPermitLimit=3
RateLimiting__ContactWindowSeconds=60

ContactWebhook__Enabled=true
ContactWebhook__Url=https://n8n.example.com/webhook/amas-contact
ContactWebhook__Secret=CAMBIAR_WEBHOOK_SECRET
ContactWebhook__TimeoutSeconds=8
```

## Payload enviado al webhook

```json
{
  "event": "contact.created",
  "id": "uuid",
  "number": 1001,
  "fullName": "Cliente AMAS",
  "email": "cliente@example.com",
  "phone": "3101234567",
  "requestType": "Producto personalizado",
  "message": "Quiero cotizar un producto personalizado.",
  "sourcePage": "/",
  "createdAt": "2026-06-18T00:00:00Z"
}
```

## Pendiente recomendado

- Agregar administracion de solicitudes en el panel admin.
- Validar Turnstile server-side cuando se defina el site key/secret de Cloudflare.
- Crear flujo n8n que reciba el webhook y dispare WhatsApp por el proveedor seleccionado.
