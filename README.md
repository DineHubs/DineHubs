# Order Management System

A comprehensive multi-tenant order management system built with .NET 8 and Angular 19, designed for restaurants and food service businesses. The system supports order creation, kitchen queue management, menu management, reporting, and subscription-based billing.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Role-Based Access Control](#role-based-access-control)
- [API Documentation](#api-documentation)
- [Development](#development)
- [Docker Setup](#docker-setup)

## Features

### Core Functionality
- **Order Management**: Create, view, and track orders with real-time status updates
- **Kitchen Queue**: Real-time kitchen display for order preparation tracking
- **Menu Management**: CRUD operations for menu items with role-based permissions
- **Multi-Tenancy**: Support for multiple tenants and branches
- **QR Code Ordering**: Generate QR codes for customer self-service ordering
- **Reporting**: Sales, inventory, and subscription usage reports
- **Subscription Management**: Plan-based subscription system with usage tracking
- **Role-Based Access Control**: Fine-grained permissions for different user roles

### Technical Features
- JWT-based authentication and authorization
- PostgreSQL database with Entity Framework Core
- Redis caching support
- Structured logging with Serilog
- API versioning
- Swagger/OpenAPI documentation
- Health checks
- Background worker services

## Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         OrderManagement.Api             │  ← Presentation Layer
│         (Controllers, Middleware)      │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│      OrderManagement.Application       │  ← Application Layer
│    (Use Cases, Interfaces, DTOs)       │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│        OrderManagement.Domain          │  ← Domain Layer
│    (Entities, Value Objects, Enums)   │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│     OrderManagement.Infrastructure     │  ← Infrastructure Layer
│  (Data Access, External Services, EF)   │
└─────────────────────────────────────────┘
```

### Layer Responsibilities

- **API Layer**: HTTP endpoints, request/response handling, authentication middleware
- **Application Layer**: Business use cases, application services, interfaces
- **Domain Layer**: Core business entities, domain logic, value objects
- **Infrastructure Layer**: Data persistence, external service integrations, cross-cutting concerns

## Technology Stack

### Backend
- **.NET 8.0**: Modern C# framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 8.0**: ORM for database access
- **PostgreSQL**: Primary database
- **Redis**: Caching and session storage
- **JWT Bearer Authentication**: Token-based authentication
- **Serilog**: Structured logging
- **Swashbuckle**: Swagger/OpenAPI documentation
- **FluentValidation**: Request validation
- **MediatR**: CQRS pattern implementation

### Frontend
- **Angular 19**: Modern TypeScript framework
- **Angular Material**: UI component library
- **RxJS**: Reactive programming
- **TypeScript**: Type-safe JavaScript

### Infrastructure
- **Docker & Docker Compose**: Containerization
- **PostgreSQL 16**: Database
- **Redis 7**: Cache
- **Seq**: Log aggregation

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [Angular CLI](https://angular.dev/cli) (v19 or higher)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for containerized setup)
- [PostgreSQL](https://www.postgresql.org/download/) (if not using Docker)
- [Redis](https://redis.io/download) (if not using Docker)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd OrderManagement
```

### 2. Start Infrastructure Services (Docker)

If using Docker, start PostgreSQL, Redis, and Seq:

```bash
docker-compose up -d
```

This will start:
- PostgreSQL on port `5432`
- Redis on port `6379`
- Seq (log viewer) on port `5341`

### 3. Configure Database Connection

Update the connection string in `OrderManagement.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=order_management;Username=om_admin;Password=om_password"
  }
}
```

### 4. Run Database Migrations

```bash
cd OrderManagement.Api
dotnet ef database update --project ../OrderManagement.Infrastructure
```

### 5. Seed Super Admin User

The API provides an endpoint to seed a super admin user:

```bash
POST http://localhost:5000/api/v1/Auth/seed-super-admin
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "YourSecurePassword123!"
}
```

### 6. Run the API

```bash
cd OrderManagement.Api
dotnet run
```

The API will be available at `http://localhost:5000` (or the port specified in `launchSettings.json`).

Swagger documentation is available at `http://localhost:5000/swagger`.

### 7. Run the Frontend

```bash
cd frontend
npm install
ng serve
```

The frontend will be available at `http://localhost:4200`.

### 8. Login

Use the super admin credentials created in step 5 to log in to the application.

## Project Structure

```
OrderManagement/
├── OrderManagement.Api/              # Web API layer
│   ├── Controllers/                  # API controllers
│   ├── Contracts/                   # Request/Response DTOs
│   ├── Middleware/                  # Custom middleware
│   ├── Configuration/               # Swagger configuration
│   └── Program.cs                  # Application entry point
│
├── OrderManagement.Application/     # Application layer
│   ├── Abstractions/               # Interfaces and contracts
│   ├── Auth/                       # Authentication interfaces
│   ├── Navigation/                 # Navigation services
│   ├── Ordering/                   # Order services
│   ├── Reporting/                  # Reporting services
│   ├── Subscriptions/              # Subscription services
│   └── Tenants/                    # Tenant management
│
├── OrderManagement.Domain/          # Domain layer
│   ├── Entities/                   # Domain entities
│   ├── Enums/                      # Domain enumerations
│   ├── Identity/                   # Identity and roles
│   └── Common/                     # Base classes
│
├── OrderManagement.Infrastructure/   # Infrastructure layer
│   ├── Persistence/                # EF Core DbContext
│   ├── Identity/                   # Identity implementation
│   ├── Navigation/                 # Navigation implementation
│   ├── Subscriptions/              # Subscription implementation
│   ├── Payments/                   # Payment gateway integration
│   ├── Messaging/                  # Email/WhatsApp services
│   └── Migrations/                 # Database migrations
│
├── OrderManagement.Identity/        # Identity entities
│   └── Entities/                   # ApplicationUser, ApplicationRole
│
├── OrderManagement.Worker/          # Background services
│   └── Worker.cs                   # Background job processor
│
├── OrderManagement.Tests/           # Unit tests
│
└── frontend/                         # Angular frontend
    ├── src/
    │   ├── app/
    │   │   ├── core/               # Core services, guards, models
    │   │   ├── features/           # Feature modules
    │   │   │   ├── orders/         # Order management
    │   │   │   ├── kitchen/        # Kitchen display
    │   │   │   ├── menu/           # Menu management
    │   │   │   ├── reports/        # Reports
    │   │   │   └── ...
    │   │   └── layout/             # Layout components
    │   └── environments/           # Environment configurations
    └── package.json
```

## Configuration

### API Configuration (`appsettings.json`)

Key configuration sections:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=order_management;Username=om_admin;Password=om_password"
  },
  "Jwt": {
    "Issuer": "OrderManagement",
    "Audience": "OrderManagement",
    "Key": "your-secret-key-here",
    "AccessTokenMinutes": 30,
    "RefreshTokenDays": 14
  },
  "MultiTenancy": {
    "Strategy": "Header",
    "HeaderName": "X-Tenant-Code",
    "BranchHeaderName": "X-Branch-Code"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  }
}
```

### Frontend Configuration (`frontend/src/environments/`)

Update `environment.ts` and `environment.prod.ts` with your API base URL:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1'
};
```

## Role-Based Access Control

The system implements role-based access control (RBAC) with the following roles:

### System Roles

- **SuperAdmin**: System administrator with highest privileges
- **Admin**: Tenant administrator
- **Manager**: Restaurant manager
- **Waiter**: Service staff
- **Kitchen**: Kitchen staff
- **InventoryManager**: Inventory management staff

### Access Matrix

| Module | SuperAdmin | Admin | Manager | Waiter | Kitchen | InventoryManager |
|--------|:----------:|:-----:|:-------:|:------:|:-------:|:----------------:|
| **Orders - Create/View** | ✅ | ❌ | ✅ | ✅ | ❌ | ❌ |
| **Orders - Update Status** | ✅ | ❌ | ✅ | ❌ | ✅ | ❌ |
| **Menu Items (CRUD)** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Kitchen Queue** | ✅ | ❌ | ✅ | ❌ | ✅ | ❌ |
| **Reports - Sales/Inventory** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Reports - Subscription** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Subscriptions** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Menu Management** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Tenant Management** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

> **Note**: For detailed access control documentation, see [ACCESS_LEVELS_DOCUMENTATION.md](./ACCESS_LEVELS_DOCUMENTATION.md)

### Authorization Implementation

- **API**: Uses `[Authorize]` attributes with role requirements
- **Frontend**: Component-level role checks using `AuthService.hasAnyRole()`
- **Navigation**: Backend-driven menu filtering based on `allowedRoles`

## API Documentation

### Swagger UI

Once the API is running, access Swagger documentation at:
```
http://localhost:5000/swagger
```

### API Endpoints

#### Authentication
- `POST /api/v1/Auth/login` - User login
- `POST /api/v1/Auth/seed-super-admin` - Seed super admin (development only)

#### Orders
- `GET /api/v1/Orders` - Get all orders
- `GET /api/v1/Orders/{id}` - Get order by ID
- `POST /api/v1/Orders` - Create new order
- `PATCH /api/v1/Orders/{id}/status` - Update order status
- `POST /api/v1/Orders/qr` - Generate QR session

#### Menu Items
- `GET /api/v1/MenuItems` - Get all menu items
- `GET /api/v1/MenuItems/{id}` - Get menu item by ID
- `POST /api/v1/MenuItems` - Create menu item
- `PUT /api/v1/MenuItems/{id}` - Update menu item
- `DELETE /api/v1/MenuItems/{id}` - Delete menu item

#### Kitchen
- `GET /api/v1/Kitchen/queue` - Get kitchen queue

#### Reports
- `GET /api/v1/Reports/sales` - Get sales report
- `GET /api/v1/Reports/inventory` - Get inventory report
- `GET /api/v1/Reports/subscription` - Get subscription usage (SuperAdmin/Admin only)

#### Navigation
- `GET /api/v1/Navigation/menu` - Get navigation menu (filtered by user roles)

### API Versioning

The API uses versioning via URL path: `/api/v1/...`

### Authentication

All endpoints (except login and seed-super-admin) require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Development

### Building the Solution

```bash
dotnet build OrderManagement.sln
```

### Running Tests

```bash
dotnet test OrderManagement.Tests/OrderManagement.Tests.csproj
```

### Creating Database Migrations

```bash
cd OrderManagement.Api
dotnet ef migrations add MigrationName --project ../OrderManagement.Infrastructure
```

### Frontend Development

```bash
cd frontend
npm install          # Install dependencies
ng serve            # Start development server
ng build            # Build for production
ng test             # Run unit tests
```

### Code Structure Guidelines

- **Controllers**: Handle HTTP requests/responses, minimal business logic
- **Application Services**: Implement use cases and business logic
- **Domain Entities**: Contain domain rules and invariants
- **Infrastructure**: Implement interfaces defined in Application layer

## Docker Setup

### Using Docker Compose

The project includes a `docker-compose.yml` file for easy setup:

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f
```

### Services

- **PostgreSQL**: Database server
- **Redis**: Cache and session storage
- **Seq**: Log aggregation and viewing

### Environment Variables

You can override default settings using environment variables or a `.env` file.

## Additional Resources

- [Access Levels Documentation](./ACCESS_LEVELS_DOCUMENTATION.md) - Detailed RBAC documentation
- [Current Implementation Analysis](./CURRENT_IMPLEMENTATION_ANALYSIS.md) - Implementation details
- [Current Access Matrix](./CURRENT_ACCESS_MATRIX.md) - Quick reference for access levels

## Contributing

1. Create a feature branch from `main`
2. Make your changes
3. Ensure all tests pass
4. Submit a pull request

## License

[Specify your license here]

## Support

For issues and questions, please [create an issue](link-to-issues) or contact the development team.

---

**Built with ❤️ using .NET 8 and Angular 19**

