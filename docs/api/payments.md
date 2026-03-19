# Payments & Webhooks — API Specification

The Morourak system integrates with the **Paymob** gateway to handle all financial transactions securely.

## 💳 Payment Lifecycle

1.  **Initialize**: Client calls `POST /api/v1/payment/create` with service request details.
2.  **Redirect**: Server returns a `paymentUrl` (iframe). User pays on Paymob's secure page.
3.  **Callback**: Paymob sends a POST request to our **Webhook** (`/api/v1/payment/paymob-callback`).
4.  **Finalize**: Server validates the HMAC signature, updates the payment record, and proceeds with the business logic (e.g., license issuance).

## 🔗 Webhook Specification
- **Endpoint**: `/api/v1/payment/paymob-callback`
- **Method**: `POST`
- **HMAC Validation**: Required! The server validates the `hmac` header against the payload using the `SecretKey` and SHA256.

### 🛡️ Resilience & Idempotency
- **Duplicate Prevention**: If Paymob sends the same webhook twice, the server detects the `Paid` status and ignores the duplicate to prevent double-issuance.
- **Entity Tracking**: Fixed a bug where `AsNoTracking` caused persistence failures. The system now uses explicit `Update` calls to guarantee the `TransactionId` is saved.

---

## 🛠️ Testing the Webhook (Postman)
To simulate a successful payment during development:
1. Use `POST` to `{{baseUrl}}/v1/payment/paymob-callback`.
2. Set Header: `hmac: test` (Allows bypassing validation in Dev environment).
3. Body (JSON):
```json
{
  "obj": {
    "id": 12345678, 
    "success": true,
    "order": {
      "id": 489453984,
      "merchant_order_id": "MOR-20260319-xxxxxxx"
    }
  }
}
```

---
*For environment configuration, see the [Setup Guide](../guides/setup-guide.md).*
