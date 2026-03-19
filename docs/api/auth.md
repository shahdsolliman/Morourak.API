# Authentication & Identity — API Specification

The Morourak system uses **JWT (JSON Web Tokens)** for secure authentication and **ASP.NET Core Identity** for user management.

## 🔐 Authentication Flow

1.  **Register**: User provides National ID and Mobile Number. System sends an OTP via email.
2.  **Verify**: User submits the OTP to activate their account.
3.  **Login**: User logs in with Mobile Number and Password. Server returns a JWT.
4.  **Authorize**: Client includes the JWT in the `Authorization: Bearer <token>` header for all subsequent requests.

## 📡 Endpoints

### 1. Register
- **URL**: `POST /api/v1/auth/register`
- **Payload**:
  ```json
  {
    "nationalId": "29001011234567",
    "mobileNumber": "01012345678",
    "email": "user@example.com",
    "password": "Password123!"
  }
  ```

### 2. Verify OTP
- **URL**: `POST /api/v1/auth/verify-otp`
- **Payload**:
  ```json
  { "email": "user@example.com", "code": "123456" }
  ```

### 3. Login
- **URL**: `POST /api/v1/auth/login`
- **Scenario**: 
  - **Success**: Returns `accessToken`, `expiresIn`, and `roles`.
  - **Failure**: Returns `401 Unauthorized` for wrong credentials or `400 BadRequest` if not verified.

---
*For development testing, see the [Testing Guide](../guides/testing-guide.md).*
