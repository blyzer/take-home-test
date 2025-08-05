# Loan Management System - Take-Home Test

## Implementation Overview

This is a full-stack Loan Management System built with .NET Core 6 backend and Angular 19 frontend. The system allows users to view loan data in a clean, responsive table interface.

### ✅ Completed Features

#### Backend (.NET Core 6)
- ✅ **RESTful API** with all required endpoints:
  - `POST /loans` - Create a new loan
  - `GET /loans/{id}` - Retrieve loan details by ID
  - `GET /loans` - List all loans
  - `POST /loans/{id}/payment` - Make payment to reduce balance
- ✅ **Entity Framework Core** with SQL Server
- ✅ **Seed data** with 5 sample loans
- ✅ **Unit and Integration Tests** (7 tests total, all passing)
- ✅ **Docker support** with Docker Compose
- ✅ **CORS configuration** for Angular frontend
- ✅ **Input validation** and error handling
- ✅ **Authentication and Authorization** with JWT 

#### Frontend (Angular 19)
- ✅ **Responsive data table** displaying loan information
- ✅ **HTTP service** to consume backend API
- ✅ **Error handling** with retry functionality
- ✅ **Loading states** and user feedback
- ✅ **Angular Material** components for professional UI
- ✅ **Responsive design** for mobile and desktop
- ✅ **Authentication and Authorization** with a login interface, JWT token handling, and route guards for role-based access control.

#### DevOps
- ✅ **Dockerized backend** with multi-stage build
- ✅ **Docker Compose** with SQL Server database
- ✅ **Health checks** for database connectivity
- ✅ **Environment configuration** for different deployments

---

## Architecture & Technology Stack

### Backend
- **.NET Core 6** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Database engine
- **xUnit** - Testing framework
- **Moq** replaced with **In-Memory Database** for better EF Core testing

### Frontend
- **Angular 19** - Frontend framework
- **Angular Material** - UI component library
- **RxJS** - Reactive programming for HTTP calls
- **TypeScript** - Type-safe JavaScript

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration

---

## Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (for local development)
- [Node.js 18+](https://nodejs.org/) and npm (for frontend development)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)

### 🚀 Docker Setup (Recommended)

1. **Clone the repository**
   ```bash
   git clone <your-fork-url>
   cd take-home-test
   ```

2. **Start the application with Docker Compose**
   ```bash
   docker-compose up --build -d
   ```
   This will:
   - Build the .NET API Docker image
   - Start SQL Server database
   - Run database migrations and seed data
   - Start the API on http://localhost:5001

3. **Start the Angular frontend**
   ```bash
   cd frontend
   npm install
   ng serve
   ```
   - Frontend will be available at http://localhost:4200

4. **View the application**
   - Open http://localhost:4200 to see the loan management interface
   - API documentation available at http://localhost:5001/loans

### 🔧 Local Development Setup

#### Backend Setup
```bash
# Navigate to backend directory
cd backend/src/Fundo.Applications.WebApi

# Restore dependencies
dotnet restore

# Update connection string in appsettings.json for local SQL Server
# Run database migrations
dotnet ef database update

# Run the API
dotnet run
# API will be available at https://localhost:7001 or http://localhost:5000
```

#### Frontend Setup
```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Update API URL in src/app/services/loan.service.ts if needed

# Start development server
ng serve
# Application will be available at http://localhost:4200
```

#### Running Tests
```bash
# Backend tests
cd backend/src/Fundo.Services.Tests
dotnet test

# Frontend tests
cd frontend
npm test
```

---

## API Documentation

### Base URL
- Docker: `http://localhost:5001`
- Local: `https://localhost:7001` or `http://localhost:5000`

### Endpoints

#### GET /loans
Retrieve all loans

**Response:**
```json
[
  {
    "id": 1,
    "amount": 25000.00,
    "currentBalance": 18750.00,
    "applicantName": "John Doe",
    "status": "active"
  }
]
```

#### GET /loans/{id}
Retrieve a specific loan by ID

**Response:**
```json
{
  "id": 1,
  "amount": 25000.00,
  "currentBalance": 18750.00,
  "applicantName": "John Doe",
  "status": "active"
}
```

#### POST /loans
Create a new loan

**Request Body:**
```json
{
  "amount": 10000.00,
  "currentBalance": 10000.00,
  "applicantName": "Jane Doe",
  "status": "active"
}
```

#### POST /loans/{id}/payment
Make a payment towards a loan

**Request Body:**
```json
500.00
```

**Notes:**
- Payment amount must be positive and not exceed current balance
- Loan status automatically changes to "paid" when balance reaches zero

#### POST /auth/login
Authenticate a user and receive a JWT token.

**Request Body:**
```json
{
  "username": "admin",
  "password": "string"
}
```

**Response:**
```json
{
  "token": "<jwt_token>",
  "username": "admin",
  "roles": [
    "Admin"
  ]
}
```

#### POST /auth/logout
Logs out the current user.

---

## Implementation Approach

### Backend Architecture
- **Clean Architecture**: Separated concerns with Controllers, Services (via DbContext), and Data layers
- **Entity Framework Core**: Used Code-First approach with migrations for database schema
- **Dependency Injection**: Leveraged .NET Core's built-in DI container
- **Testing Strategy**: Replaced traditional mocking with in-memory database for more realistic EF Core testing
- **Error Handling**: Implemented proper HTTP status codes and validation

### Frontend Architecture
- **Reactive Programming**: Used RxJS Observables for HTTP operations
- **Component-Based**: Single-responsibility components with clear separation of concerns
- **Service Layer**: Centralized API communication through dedicated service
- **Error Boundaries**: Graceful error handling with user-friendly messages
- **Responsive Design**: Mobile-first approach with Angular Material components

### Key Design Decisions
1. **In-Memory Database for Tests**: Replaced Moq with EF Core's in-memory provider for more reliable testing
2. **CORS Configuration**: Enabled specific origin (Angular dev server) for security
3. **Docker Multi-Stage Build**: Optimized container size with separate build and runtime stages
4. **Health Checks**: Added database health checks for reliable container startup

---

## Challenges Faced & Solutions

### 1. Testing Entity Framework with Moq
**Challenge**: Traditional mocking of DbContext and DbSet is complex and error-prone with EF Core.

**Solution**: Switched to EF Core's in-memory database provider, which provides more realistic testing scenarios and is easier to maintain.

### 2. API Route Consistency
**Challenge**: Initial implementation had inconsistent route naming between requirements (/loans) and implementation (/loan).

**Solution**: Updated controller route to match API specifications and updated all tests accordingly.

### 3. Docker Container Health Checks
**Challenge**: API container was starting before SQL Server was fully ready, causing connection failures.

**Solution**: Implemented proper health checks in docker-compose.yml with retry logic and wait conditions.

### 4. CORS Configuration
**Challenge**: Angular frontend couldn't communicate with API due to CORS restrictions.

**Solution**: Added specific CORS policy allowing the Angular development server origin with proper headers and methods.

---

### Advanced Features
- **Pagination**: Large dataset handling with server-side pagination
- **Search & Filtering**: Advanced query capabilities for loan data
- **Audit Logging**: Track all loan modifications and payments
- **Payment History**: Detailed transaction history for each loan
- **Notification System**: Email/SMS notifications for payment reminders

### DevOps & Monitoring
- **GitHub Actions**: CI/CD pipeline for automated testing and deployment
- **Structured Logging**: Serilog integration with centralized logging
- **Health Endpoints**: Comprehensive application health monitoring
- **Performance Monitoring**: Application Insights or similar APM tools
- **Database Backup Strategy**: Automated backup and recovery procedures

### Frontend Enhancements
- **State Management**: NgRx for complex state management
- **Progressive Web App**: Offline capabilities and push notifications
- **Advanced UI**: Charts and dashboards for loan analytics
- **Print/Export**: PDF generation and Excel export functionality
- **Internationalization**: Multi-language support

---

## Project Structure

```
take-home-test/
├── backend/
│   └── src/
│       ├── Fundo.Applications.WebApi/      # Main API project
│       │   ├── Controllers/                 # API controllers
│       │   ├── Models/                      # Data models
│       │   ├── Migrations/                  # EF Core migrations
│       │   ├── LoanContext.cs               # Database context
│       │   └── Startup.cs                   # App configuration
│       └── Fundo.Services.Tests/           # Test project
│           ├── Unit/                        # Unit tests
│           └── Integration/                 # Integration tests
├── frontend/
│   └── src/
│       └── app/
│           ├── services/                    # HTTP services
│           ├── app.component.*              # Main component
│           └── app.config.ts                # App configuration
├── Dockerfile                          # Backend containerization
├── docker-compose.yml                  # Multi-container setup
└── README.md                           # This file
```

---

## Conclusion

This implementation demonstrates a production-ready foundation for a loan management system with:
- **Robust Backend**: RESTful API with proper validation, testing, and error handling
- **Modern Frontend**: Responsive Angular application with professional UI/UX
- **DevOps Ready**: Containerized deployment with Docker Compose
- **Test Coverage**: Comprehensive unit and integration tests
- **Documentation**: Clear setup instructions and API documentation

The codebase follows industry best practices and is structured for easy maintenance and future enhancements. The modular architecture allows for straightforward addition of new features like authentication, advanced search, and payment processing.
