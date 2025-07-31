# HIV System API

A robust, modular, and secure RESTful API built with **.NET 8** and **C# 12** for managing HIV patient care, medical records, appointments, ARV regimens, notifications, and more. The system is designed for healthcare environments, supporting multiple user roles with fine-grained access control and extensible business logic.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Technology Stack](#technology-stack)
- [System Architecture](#system-architecture)
- [User Roles & Permissions](#user-roles--permissions)
  - [Admin](#admin)
  - [Doctor](#doctor)
  - [Patient](#patient)
  - [Staff](#staff)
  - [Manager](#manager)
- [Key Modules & Features](#key-modules--features)
- [API Endpoints Overview](#api-endpoints-overview)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)

---

## Project Overview

This API provides a comprehensive backend for an HIV management system, enabling secure and efficient handling of:

- User registration, authentication, and profile management
- Patient and doctor management
- Appointment scheduling and tracking
- ARV regimen and medication management
- Medical records and test results
- Notifications and role-based communication
- Payment processing and billing

The system is designed for extensibility, security, and compliance with healthcare data standards.

---

## Technology Stack

- **.NET 8 / C# 12**
- **Entity Framework Core** (data access)
- **ASP.NET Core Web API**
- **Json Web Token** (JWT)
- **Microsoft.Extensions.Caching.Memory** (in-memory caching)
- **Stripe** (payment integration)
- **Dependency Injection** (built-in)
- **Unit Testing Support**
- **SQL Server** (or compatible RDBMS)

---

## System Architecture

- **Layered Architecture**: Separation of DTOs, business objects, services, repositories, and controllers.
- **Role-Based Access Control**: Each user role has specific permissions and accessible endpoints.
- **Validation & Security**: Strong input validation, PBKDF2 password hashing, and protection against common vulnerabilities.
- **Caching**: Used for verification codes, pending registrations, and more.
- **Extensible**: Easily add new modules or roles.

---

## User Roles & Permissions

### Admin

- **Account Management**: Create, update, delete any user account.
- **View All Accounts**: Retrieve all user accounts and details.
- **Profile Update**: Update own basic profile (Fullname, DOB, Gender).
- **Password Management**: Initiate and reset passwords for any user.
- **Notification Management**: Create and send notifications to any role or user.
- **System Oversight**: Access to all patient, doctor, staff, and manager data.

### Doctor

- **Profile Management**: Update own profile (Fullname, DOB, Gender, Degree, Bio).
- **Patient Management**: View and manage assigned patients.
- **Appointment Management**: View, accept, or reject appointments.
- **Medical Records**: Access and update patient medical records and test results.
- **Work Schedule**: Manage own work schedule.

### Patient

- **Self-Registration**: Register and verify account via email.
- **Profile Management**: Update own basic profile (Fullname, DOB, Gender).
- **View Medical Records**: Access own medical records and test results.
- **Appointment Management**: Book, view, and manage appointments.
- **ARV Regimen Tracking**: View and track own ARV regimens and medications.
- **Payment**: View and pay for medical services.

### Staff

- **Profile Management**: Update own profile (Fullname, DOB, Gender, Degree, Bio).
- **Test Results**: Add and manage component test results for patients.
- **Support Functions**: Assist with patient and appointment management.
- **Notification Management**: Receive and manage notifications.

### Manager

- **Profile Management**: Update own basic profile (Fullname, DOB, Gender).
- **Dashboard Access**: View system statistics, reports, and analytics.
- **Oversight**: Monitor staff, doctors, and patient activities.

---

## Key Modules & Features

### 1. **Account & Authentication**
- Registration (with email verification)
- Login (with secure password hashing)
- Password reset (with verification code)
- Role-based profile update
- Change password

### 2. **Patient Management**
- Create, update, delete patient accounts
- View patient details and medical records
- Manage ARV regimens and medications

### 3. **Doctor Management**
- Create, update, delete doctor accounts
- Manage doctor profiles (degree, bio)
- Manage work schedules
- View and manage appointments

### 4. **Appointment Management**
- Book, update, cancel appointments
- Role-based appointment views (doctor, patient, admin)
- Appointment status tracking

### 5. **ARV Regimen & Medication**
- Create, update, delete ARV regimens and medication templates
- Assign regimens and medications to patients
- Track medication usage and adherence

### 6. **Medical Records & Test Results**
- Create and update patient medical records
- Add and manage test results (including component test results)
- Role-based access to records

### 7. **Notifications**
- Create and send notifications to roles or specific users
- Mark notifications as read/unread
- View personal and role-based notifications

### 8. **Payments**
- Create and manage payments for medical services
- Stripe integration for payment processing
- View payment history (admin, patient)

---

## API Endpoints Overview

> **Note:** For full request/response models, see the DTOs and controller files.

### Account & Authentication

- `POST /api/accounts/register` — Register new account (role-based)
- `POST /api/accounts/login` — Authenticate user
- `POST /api/accounts/password-reset/initiate` — Initiate password reset
- `POST /api/accounts/password-reset/verify` — Verify password reset code
- `POST /api/accounts/password-reset/complete` — Complete password reset
- `PUT /api/accounts/{id}` — Update account (role-based fields)
- `DELETE /api/accounts/{id}` — Delete account (admin only)
- `GET /api/accounts/{id}` — Get account by ID

### Profile Management

- `PUT /api/accounts/profile/basic` — Update basic profile (Admin, Patient, Manager)
- `PUT /api/accounts/profile/doctor` — Update doctor profile (Doctor only)
- `PUT /api/accounts/profile/staff` — Update staff profile (Staff only)
- `PUT /api/accounts/profile/patient` — Update patient profile (Patient only)

### Patient & Medical Records

- `POST /api/patients` — Create patient account
- `GET /api/patients/{id}` — Get patient details
- `PUT /api/patients/{id}` — Update patient info
- `DELETE /api/patients/{id}` — Delete patient
- `GET /api/patients/{id}/medical-records` — Get patient medical records

### Doctor Management

- `POST /api/doctors` — Create doctor account
- `GET /api/doctors/{id}` — Get doctor details
- `PUT /api/doctors/{id}` — Update doctor info
- `DELETE /api/doctors/{id}` — Delete doctor
- `GET /api/doctors/{id}/work-schedules` — Get doctor work schedules

### Appointment Management

- `POST /api/appointments` — Create appointment
- `GET /api/appointments/{id}` — Get appointment details
- `PUT /api/appointments/{id}` — Update appointment
- `DELETE /api/appointments/{id}` — Cancel appointment
- `GET /api/appointments/account/{accountId}` — Get appointments by account

### ARV Regimen & Medication

- `POST /api/arv-regimens` — Create ARV regimen template
- `GET /api/arv-regimens/{id}` — Get ARV regimen template
- `PUT /api/arv-regimens/{id}` — Update ARV regimen template
- `DELETE /api/arv-regimens/{id}` — Delete ARV regimen template
- `POST /api/arv-medications` — Create ARV medication template
- `GET /api/arv-medications/{id}` — Get ARV medication template

### Test Results

- `POST /api/test-results` — Create test result
- `GET /api/test-results/{id}` — Get test result
- `PUT /api/test-results/{id}` — Update test result
- `DELETE /api/test-results/{id}` — Delete test result
- `POST /api/component-test-results` — Add component test result

### Notifications

- `POST /api/notifications` — Create notification
- `POST /api/notifications/send-to-role` — Send notification to role
- `POST /api/notifications/send-to-account` — Send notification to account
- `GET /api/notifications/personal/{accountId}` — Get personal notifications

### Payments

- `POST /api/payments` — Create payment (with Stripe intent)
- `GET /api/payments/{id}` — Get payment details
- `GET /api/payments/personal/{accountId}` — Get personal payment history

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or compatible database
- Visual Studio 2022 or later

### Installation

1. **Clone the repository:**
- `git clone https://github.com/haibeu2901/HIV_System_API.git`
- `cd HIV_System_API`

2. **Configure the database connection:**
- Update `appsettings.json` with your database connection string.

3. **Restore dependencies:**
- `dotnet restore`

4. **Apply migrations (if using EF Core):**
- `dotnet ef database update`

5. **Run the API:**
- `dotnet run --project HIV_System_API_Backen`

---

## Project Structure

- `HIV_System_API_BOs/` — Business objects (entities/models)
- `HIV_System_API_DAOs/` — Data access objects
- `HIV_System_API_DTOs/` — Data transfer objects
- `HIV_System_API_Services/` — Business logic and service layer
- `HIV_System_API_Repositories/` — Data access and repository pattern
- `HIV_System_API_Backend/` — API entry point and controllers

---

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

---

## License

This project is licensed under the MIT License.

---

**Note:** This project is for educational and research purposes. Ensure compliance with healthcare data regulations (e.g., HIPAA, GDPR) before deploying in production.
