# Licenses (Driving & Vehicle) — API Specification

The Morourak system manages the entire lifecycle of driving and vehicle licenses. All endpoints require **JWT Bearer Authentication**.

## 🛡️ Common Request Headers
| Header | Value | Description |
|--------|-------|-------------|
| `Authorization` | `Bearer <token>` | Required |
| `Content-Type` | `multipart/form-data` | For document uploads |
| `Content-Type` | `application/json` | For status updates |

---

## 🪪 Driving Licenses

### 1. Issue a New License (Step 1)
- **Endpoint**: `POST /api/DrivingLicense/upload-documents`
- **Content-Type**: `multipart/form-data`
- **Fields**:
  - `Category`: `1` (Private), `2` (Professional)
  - `EducationalCertificate`: File (JPG/PNG/PDF)
  - `PersonalPhoto`: File (JPG/PNG/PDF)
  - `IdCard`: File (JPG/PNG/PDF)
  - `ResidenceProof`: File (JPG/PNG/PDF)
  - `Governorate`: string
  - `LicensingUnit`: string

### 2. Renew 	Driving License
- **Endpoint**: `POST /api/DrivingLicense/renewal-request`
- **Fields**:
  - `NewCategory`: (Optional) Change category during renewal.

### 3. Issue Replacement (Lost/Damaged)
- **Endpoint**: `POST /api/DrivingLicense/issue-replacement/{licenseNumber}`
- **Payload**:
  ```json
  {
    "replacementType": "Lost", 
    "delivery": { "method": 2, "address": { "city": "Maadi", ... } }
  }
  ```

---

## 🚗 Vehicle Licenses

### 1. Issue a New Vehicle License
- **Endpoint**: `POST /api/v1/vehicle/new`
- **Content-Type**: `multipart/form-data`
- **Fields**:
  - `vehicle_type`: (e.g., `Private`, `Truck`, `Motorcycle`)
  - `ownership_proof`: File
  - `vehicle_data_certificate`: File
  - `insurance_certificate`: File

### 2. Renew 	Vehicle License
- **Endpoint**: `POST /api/v1/vehicle/renew`
- **Payload**:
  ```json
  {
    "licenseId": "GUID-STRING",
    "deliveryMethod": "Mail",
    "medicalExamAppointment": "2025-10-25"
  }
  ```

---

## 📦 Delivery Options (Case A/B)

### 1. Pickup at Traffic Unit
- **Method ID**: `1`
- **Payload**: `{ "method": 1 }`

### 2. Home Delivery
- **Method ID**: `2`
- **Payload**:
  ```json
  {
    "method": 2,
    "address": {
      "governorate": "Cairo",
      "city": "Nasr City",
      "details": "15 Abbas El Akkad St."
    }
  }
  ```

---
*For appointment booking details, see the [Appointments Guide](./appointments.md).*
