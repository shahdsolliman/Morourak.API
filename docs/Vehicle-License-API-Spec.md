# Vehicle License Management API — Specification

Base URL: `/api/vehicle-license`  
Authentication: JWT Bearer token required for `POST /new` and `POST /renew`.

---

## 1. Issue a New Vehicle License

**Method:** `POST`  
**URL:** `/api/vehicle-license/new`  
**Content-Type:** `multipart/form-data`  
**Authorization:** Required (Bearer token)

### Request (multipart/form-data)

| Field | Type | Required | Allowed file types | Description |
|-------|------|----------|--------------------|-------------|
| `vehicle_type` | string | Yes | — | One of: `Private`, `Truck`, `Taxi`, `Motorcycle`, `Bus`, `Private Bus`, `Trailer` |
| `ownership_proof` | file | Yes | JPG, PNG, PDF | Ownership proof document |
| `vehicle_data_certificate` | file | Yes | JPG, PNG, PDF | Vehicle data certificate |
| `id_card_front` | file | Yes | JPG, PNG, PDF | National ID card (front) |
| `id_card_back` | file | Yes | JPG, PNG, PDF | National ID card (back) |
| `insurance_certificate` | file | Yes | JPG, PNG, PDF | Insurance certificate |
| `custom_clearance` | file | No | JPG, PNG, PDF | Custom clearance (optional) |
| `technical_inspection_receipt` | file | No | JPG, PNG, PDF | Technical inspection receipt (optional) |

**Note:** Form field names are case-insensitive; `vehicle_type` and `VehicleType` are both accepted.

### Validation rules

- `vehicle_type`: required; must be one of the values listed above.
- `ownership_proof`, `vehicle_data_certificate`, `id_card_front`, `id_card_back`, `insurance_certificate`: required; each must be a non-empty file with extension `.jpg`, `.jpeg`, `.png`, or `.pdf`.
- `custom_clearance`, `technical_inspection_receipt`: optional; if provided, must be JPG, PNG, or PDF.

### Success response (200 OK)

```json
{
  "status": "success",
  "license_id": "12345",
  "message": "Vehicle license request submitted successfully."
}
```

*(In the implementation, `license_id` is returned as the GUID string of the created license.)*

### Error responses

**400 Bad Request** — Validation failed or request body missing:

```json
{
  "status": "error",
  "errors": [
    "vehicle_type is required.",
    "insurance_certificate must be JPG, PNG or PDF."
  ]
}
```

**401 Unauthorized** — Missing or invalid token / user identity not resolved:

```json
{
  "status": "error",
  "errors": [
    "User identity could not be resolved."
  ]
}
```

---

## 2. Renew Vehicle License

**Method:** `POST`  
**URL:** `/api/vehicle-license/renew`  
**Content-Type:** `application/json`  
**Authorization:** Required (Bearer token)

### Request body (JSON)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `license_id` | string | Yes | GUID of the existing vehicle license (must belong to the authenticated user) |
| `governorate` | string | Yes | Governorate name (e.g. `Cairo`) |
| `traffic_unit` | string | Yes | Traffic unit name (e.g. `Unit 10`) |
| `medical_exam_appointment` | string | Yes | Date in `YYYY-MM-DD` format; must be a valid date |
| `delivery_method` | string | Yes | Either `Mail` or `Pickup at Unit` |

**Example request body:**

```json
{
  "license_id": "12345",
  "governorate": "Cairo",
  "traffic_unit": "Unit 10",
  "medical_exam_appointment": "2025-03-15",
  "delivery_method": "Mail"
}
```

*(In the implementation, `license_id` is the string representation of the license’s GUID.)*

### Validation rules

- `license_id`: required; must be a valid GUID and must exist and be linked to the authenticated user’s account.
- `governorate`: required; non-empty string.
- `traffic_unit`: required; non-empty string.
- `medical_exam_appointment`: required; must be a valid date in `YYYY-MM-DD` format.
- `delivery_method`: required; must be exactly `Mail` or `Pickup at Unit`.

### Success response (200 OK)

```json
{
  "status": "success",
  "renewal_id": "67890",
  "message": "Vehicle license renewed successfully and medical appointment booked."
}
```

*(In the implementation, `renewal_id` is the GUID string of the new renewal license record.)*

### Error responses

**400 Bad Request** — Validation failed, invalid date, or license not found:

```json
{
  "status": "error",
  "errors": [
    "license_id not found or not linked to your account.",
    "appointment date is not available"
  ]
}
```

Other possible error messages include:

- `"license_id is invalid."`
- `"governorate is required."`
- `"traffic_unit is required."`
- `"delivery_method is required."`
- `"delivery_method must be either \"Mail\" or \"Pickup at Unit\"."`
- `"medical_exam_appointment must be a valid date (YYYY-MM-DD)."`

**401 Unauthorized** — Missing or invalid token:

```json
{
  "status": "error",
  "errors": [
    "User identity could not be resolved."
  ]
}
```

---

## Summary

| Endpoint | Method | Auth | Content-Type | Purpose |
|----------|--------|------|--------------|---------|
| `/api/vehicle-license/new` | POST | Bearer | multipart/form-data | Issue a new vehicle license with documents |
| `/api/vehicle-license/renew` | POST | Bearer | application/json | Renew an existing vehicle license |

**Supported file extensions:** `.jpg`, `.jpeg`, `.png`, `.pdf` (case-insensitive).

**API response shape:** All success and error responses use the same structure: `status`, optional `license_id` or `renewal_id`, optional `message`, and optional `errors` array.
