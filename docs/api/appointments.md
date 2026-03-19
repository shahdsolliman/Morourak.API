# Appointment System — API Specification

The appointment system allows citizens to book slots for medical examinations, technical inspections, and driving tests.

## 📅 Available Slots
Retrieve all open slots for a specific date and service type.
- **Endpoint**: `GET /api/v1/appointments/available-slots`
- **Query Parameters**:
  - `date`: `YYYY-MM-DD`
  - `type`: `1` (Medical), `2` (Technical), `3` (Driving)
- **Response**: List of available time slots.

## 📅 Book Appointment
Reserve a specific slot for a service request.
- **Endpoint**: `POST /api/v1/appointments/book`
- **Authorization**: Required (Citizen Role)
- **Request Body**:
  ```json
  {
    "nationalId": "29001011234567",
    "serviceType": "DrivingLicenseIssue",
    "date": "2025-10-25",
    "time": "10:30",
    "governorateId": 1,
    "trafficUnitId": 2
  }
  ```

## 🌍 Localization & Customization
The response for the booking endpoint includes localized Arabic details:
- **`dateFormatted`**: Properly formatted Arabic date (e.g., `25 اكتوبر 2025`).
- **`timeFormatted`**: Localized time (e.g., `10:30 صباحا`).

### 📍 Special Traffic Unit Overrides
For the **"10th of Ramadan"** traffic unit, the API returns hardcoded professional details:
- **Address**: شارع التسعين , العاشر من رمضان , الشرقية
- **Working Hours**: 9 ص الي 3 م (الاحد - الخميس)

---
*For payment and webhook details, see the [Payments Guide](./payments.md).*
