# Testing & Webhook Simulation Guide

This guide provides instructions for testing the advanced features of **Morourak.API**, including Paymob webhooks and Redis caching.

## 💳 Testing Paymob Webhooks
Since you cannot easily trigger a real Paymob callback in local development without a public URL (like ngrok), you can simulate it using **Postman**.

1.  **Endpoint**: `POST /api/v1/payment/paymob-callback`
2.  **Headers**:
    - `hmac`: `test` (Bypasses signature validation in `Development` mode)
3.  **Body (JSON)**:
    ```json
    {
      "obj": {
        "id": 12345678, 
        "success": true,
        "order": {
          "id": 489453984,
          "merchant_order_id": "MOR-2026-xxxx-xxxx"
        }
      }
    }
    ```
4.  **Verification**: Check the `Payments` table in the database to ensure `Status` is `Paid` and `TransactionId` is updated.

## ⚡ Testing Redis Caching
To verify that caching is working:
1.  **Enable Logging**: Set `Serilog` to `Information` or `Debug`.
2.  **Request Data**: Call `GET /api/v1/appointments/my-appointments`.
    - **First Call**: Log should show `Cache miss` and a database query.
    - **Second Call**: Log should show `Cache hit` and no database query.
3.  **Invalidation**: Book a new appointment or cancel one.
4.  **Re-verify**: The next `GET` call should show a `Cache miss` (invalidation successful).

## 🧪 Localization Testing
Check the responses for appointment booking to see Arabic formatting:
- **Date**: `25 اكتوبر 2025`
- **Time**: `10:30 صباحا`

---
*For general setup, see the [Setup Guide](./setup-guide.md).*
