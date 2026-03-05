# Driving License API Documentation

This document provides a comprehensive guide to the API endpoints related to the Driving License module in the Morourak system.

---

## 1. Authentication & Login
All endpoints in this section are available at `/api/Auth`.

### Request OTP / Register
Citizens must register using their National ID and Mobile Number.
- **Endpoint**: `POST /api/Auth/register`
- **Request Body**:
  ```json
  {
    "nationalId": "29001011234567",
    "mobileNumber": "01012345678",
    "username": "johndoe",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "password": "Password123!",
    "confirmPassword": "Password123!"
  }
  ```
- **Response**: `200 OK` (OTP sent to email).

### Verify OTP
- **Endpoint**: `POST /api/Auth/verify-otp`
- **Request Body**:
  ```json
  {
    "email": "john.doe@example.com",
    "code": "123456"
  }
  ```
- **Response**: `200 OK` (Account verified).

### Login
- **Endpoint**: `POST /api/Auth/login`
- **Login Scenarios**:
  - **Success**: Returns a JWT token and user roles.
  - **Wrong Password**: Returns `401 Unauthorized`.
  - **Not Verified**: Returns `400 BadRequest` if OTP hasn't been verified.
  - **Deleted Account**: Returns `401 Unauthorized`.
- **Request Body**:
  ```json
  {
    "mobileNumber": "01012345678",
    "password": "Password123!"
  }
  ```

### Forgot/Reset Password
- **Forgot Password**: `POST /api/Auth/forgot-password` (Sends OTP).
- **Reset Password**: `POST /api/Auth/reset-password` (Verifies OTP and updates password).

---

## 2. Appointment System
Endpoints for booking medical examinations and driving tests.

### Get Available Slots
- **Endpoint**: `GET /api/appointments/available-slots`
- **Query Parameters**:
  - `date`: `YYYY-MM-DD`
  - `type`: `1` (Medical), `2` (Technical), `3` (Driving)
- **Response**: List of available time slots.

### Book Appointment
- **Endpoint**: `POST /api/appointments/book`
- **Authorization**: Required (Citizen Role)
- **Request Body**:
  ```json
  {
    "requestNumber": "DL-2026-0001",
    "type": 1,
    "date": "2026-03-01",
    "startTime": "09:00:00"
  }
  ```

---

## 3. Driving License Lifecycle
All endpoints in this section are available at `/api/DrivingLicense`.

### Step 1: Upload Documents (New License)
- **Endpoint**: `POST /api/DrivingLicense/upload-documents`
- **Format**: `multipart/form-data`
- **Fields**:
  - `Category`: `1` (Private), `2` (Professional), etc.
  - `EducationalCertificate`: File
  - `PersonalPhoto`: File
  - `IdCard`: File
  - `ResidenceProof`: File
  - `Governorate`: string
  - `LicensingUnit`: string

### Step 2: Finalize License (After passing all tests)
- **Endpoint**: `POST /api/DrivingLicense/finalize/{requestNumber}`
- **Request Body**: `DeliveryInfoDto` (See Delivery Methods section).

### Renewal Request
- **Endpoint**: `POST /api/DrivingLicense/renewal-request`
- **Format**: `multipart/form-data`
- **Fields**:
  - `NewCategory`: (Optional)

### Finalize Renewal
- **Endpoint**: `POST /api/DrivingLicense/finalize-renewal/{requestNumber}`
- **Request Body**: `DeliveryInfoDto`.

### Issue Replacement (Lost/Damaged)
- **Endpoint**: `POST /api/DrivingLicense/issue-replacement/{drivingLicenseNumber}`
- **Request Body**:
  ```json
  {
    "replacementType": "Lost", 
    "delivery": {
      "method": 2,
      "address": {
        "governorate": "Cairo",
        "city": "Maadi",
        "details": "123 Street, Building 4"
      }
    }
  }
  ```

---

## 4. Delivery Methods
The system supports two delivery methods for receiving the physical license card.

### Case A: Traffic Unit (Pickup)
- **Method ID**: `1`
- **Payload**:
  ```json
  {
    "method": 1,
    "address": null
  }
  ```

### Case B: Home Delivery
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

## 5. Common Error Responses
- **400 Bad Request**: Validation errors or business logic violations (e.g., booking on a full day).
- **401 Unauthorized**: Missing or invalid JWT token.
- **403 Forbidden**: User does not have the "CITIZEN" role.
- **500 Internal Server Error**: Unexpected system failure.
