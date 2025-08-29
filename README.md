# ShiftsLogger

A comprehensive shift management system built with .NET 9.0, featuring a Web API backend, console frontend, and extensive testing suite. The application follows SOLID principles and clean architecture patterns to provide a robust solution for recording, storing, and managing work shifts.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Technologies & NuGet Packages](#technologies--nuget-packages)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Pagination](#pagination)
- [Getting Started](#getting-started)
- [Database Configuration](#database-configuration)
- [Testing](#testing)
- [Contributing](#contributing)

## 🎯 Overview

ShiftsLogger is a WebAPI-based application designed for recording and managing work shifts. It provides:

- **RESTful API** for shift, worker, and location management
- **Console-based frontend** with interactive menus using Spectre.Console
- **Comprehensive testing suite** with xUnit, Moq, and FluentAssertions
- **Cross-platform database support** (SQL Server for Windows, SQL Server for Linux)
- **In-memory database** for testing environments

## Requirements
- This is an application where you should record a worker's shifts.
- You need to create two applications: the Web API and the UI that will call it.
- All validation and user input should happen in the UI app.
- Your API's controller should be lean. Any logic should be handled in a separate "service".
- You should use SQL Server, not SQLite
- You should use the "code first" approach to create your database, using Entity Framework's migrations tool.
- Your front-end project needs to have try-catch blocks around the API calls so it handles unexpected errors (i.e. the API isn't running or returns a 500 error.)

## 🏗️ Architecture

The solution follows a clean architecture approach with clear separation of concerns:

### Core Components

- **ShiftsLoggerV2.RyanW84** - Web API backend
- **ConsoleFrontEnd** - Interactive console application
- **ShiftsLoggerV2.RyanW84.Tests** - Comprehensive test suite

### Key Design Patterns

- **SOLID Principles** implementation
- **Repository Pattern** for data access
- **Service Layer** for business logic
- **DTO Pattern** for API communication
- **Validation Layer** for data integrity
- **Dependency Injection** throughout

## ✨ Features

### API Features
- Full CRUD operations for Shifts, Workers, and Locations
- Advanced filtering and querying capabilities
- **Pagination support** with configurable page size and navigation
- Date range filtering for shifts
- Worker-specific shift retrieval
- Location-based queries (by country, county)
- Comprehensive error handling and logging

### Console Frontend Features
- Interactive menu system
- Real-time API communication
- Data validation and error handling
- Formatted output with Spectre.Console
- SOLID-compliant service architecture

### Testing Features
- Unit tests for all major components
- Integration tests for API endpoints
- Model validation tests
- Service layer tests
- Mock-based testing with Moq

## � Recent Improvements

### Security & Configuration
- ✅ **Database Credentials Secured**: Moved sensitive database passwords to user secrets
- ✅ **Cross-Platform Compatibility**: Fixed VS Code configuration files for Windows/Linux compatibility
- ✅ **Setup Scripts**: Created automated setup scripts for user secrets configuration

### Logging & Monitoring
- ✅ **Structured Logging**: Replaced Console.WriteLine with proper ILogger implementation
- ✅ **Health Checks**: Added comprehensive health check endpoints for monitoring
- ✅ **Error Standardization**: Consistent error handling across all controllers

### Performance & Database
- ✅ **Database Indexes**: Added performance indexes for frequently queried fields
- ✅ **Query Optimization**: Composite indexes for common query patterns
- ✅ **Connection Management**: Improved database connection handling
- ✅ **Pagination Implementation**: Added efficient pagination across all endpoints to handle large datasets
- ✅ **HTTP Response Optimization**: Resolved size limitations with paginated responses

### Code Quality
- ✅ **Dependency Alignment**: Synchronized package versions across projects
- ✅ **Response Standardization**: Consistent API response formatting
- ✅ **Integration Tests**: Enabled and fixed integration test suite

## �🛠️ Technologies & NuGet Packages

### Core Framework
- **.NET 9.0** - Latest .NET framework

### Web API Project (ShiftsLoggerV2.RyanW84)

#### Entity Framework & Database
- `Microsoft.EntityFrameworkCore` (9.0.8) - ORM framework
- `Microsoft.EntityFrameworkCore.Abstractions` (9.0.8) - EF Core abstractions
- `Microsoft.EntityFrameworkCore.Design` (9.0.8) - Design-time tools
- `Microsoft.EntityFrameworkCore.SqlServer` (9.0.8) - SQL Server provider
- `Microsoft.EntityFrameworkCore.InMemory` (9.0.8) - In-memory provider for testing
- `Microsoft.EntityFrameworkCore.Tools` (9.0.8) - Migration tools

#### API & Documentation
- `Microsoft.AspNetCore.OpenApi` (9.0.8) - OpenAPI support
- `Microsoft.OpenApi` (1.6.24) - OpenAPI specifications
- `Scalar.AspNetCore` (2.7.2) - Modern API documentation

#### Mapping & DI
- `AutoMapper` (15.0.1) - Object-to-object mapping
- `Microsoft.Extensions.DependencyInjection` (9.0.8) - Dependency injection
- `Microsoft.Extensions.Logging` (9.0.8) - Logging framework

#### UI & Console
- `Spectre.Console` (0.50.0) - Rich console applications

### Console Frontend Project (ConsoleFrontEnd)

#### Core Dependencies
- `AutoMapper` (15.0.1) - Object mapping

#### Hosting & Configuration
- `Microsoft.Extensions.DependencyInjection` (9.0.8) - DI container
- `Microsoft.Extensions.Hosting` (9.0.8) - Host builder
- `Microsoft.Extensions.Http` (9.0.8) - HTTP client factory
- `Microsoft.Extensions.Logging` (9.0.8) - Logging

#### UI & Console
- `Spectre.Console` (0.50.0) - Console UI

### Test Project (ShiftsLoggerV2.RyanW84.Tests)

#### Testing Framework
- `Microsoft.NET.Test.Sdk` (17.14.1) - Test SDK
- `xunit` (2.9.3) - Testing framework
- `xunit.runner.visualstudio` (3.1.4) - Visual Studio test runner

#### Mocking & Assertions
- `Moq` (4.20.72) - Mocking framework
- `FluentAssertions` (8.6.0) - Fluent assertion library

#### Integration Testing
- `Microsoft.AspNetCore.Mvc.Testing` (9.0.8) - ASP.NET Core testing
- `Microsoft.EntityFrameworkCore.InMemory` (9.0.8) - In-memory database for tests

#### Code Coverage
- `coverlet.collector` (6.0.4) - Code coverage collection

## 📁 Project Structure

```
ShiftsLogger/
├── README.md
├── ShiftsLoggerV2.RyanW84.sln          # Main solution file
├── run-tests.sh                        # Test execution script
├── TEST_DOCUMENTATION.md               # Testing documentation
└── test-config.json                    # Test configuration

├── ShiftsLoggerV2.RyanW84/             # Web API Project
│   ├── Controllers/                    # API Controllers
│   │   ├── LocationsController.cs      # Location endpoints
│   │   ├── ShiftsController.cs         # Shift endpoints
│   │   └── WorkersController.cs        # Worker endpoints
│   ├── Data/                          # Database context
│   ├── Models/                        # Entity models
│   ├── Dtos/                          # Data transfer objects
│   ├── Services/                      # Business services
│   ├── Repositories/                  # Data repositories
│   ├── Migrations/                    # EF migrations
│   └── Program.cs                     # Application entry point

├── ConsoleFrontEnd/                   # Console Application
│   ├── MenuSystem/                    # Interactive menus
│   ├── Services/                      # Frontend services
│   ├── Models/                        # Frontend models
│   ├── Interfaces/                    # Service contracts
│   └── Program.cs                     # Console entry point

└── ShiftsLoggerV2.RyanW84.Tests/      # Test Project
    ├── Controllers/                   # Controller tests
    ├── Models/                        # Model tests
    ├── Services/                      # Service tests
    ├── Integration/                   # Integration tests
    └── Utilities/                     # Test utilities
```

## 🚀 API Endpoints

### Shifts Controller (`/api/shifts`)
- `GET /api/shifts` - Get all shifts with filtering and pagination
  - Query parameters: `pageNumber`, `pageSize`, `workerId`, `locationId`, `startDate`, `endDate`
- `GET /api/shifts/{id}` - Get shift by ID
- `POST /api/shifts` - Create new shift
- `PUT /api/shifts/{id}` - Update shift
- `DELETE /api/shifts/{id}` - Delete shift
- `GET /api/shifts/by-date-range` - Get shifts by date range
- `GET /api/shifts/worker/{workerId}` - Get shifts by worker

### Workers Controller (`/api/workers`)
- `GET /api/workers` - Get all workers with filtering and pagination
  - Query parameters: `pageNumber`, `pageSize`, `name`, `email`, `phone`
- `GET /api/workers/{id}` - Get worker by ID
- `POST /api/workers` - Create new worker
- `PUT /api/workers/{id}` - Update worker
- `DELETE /api/workers/{id}` - Delete worker
- `GET /api/workers/by-email-domain` - Get workers by email domain
- `GET /api/workers/by-phone-area-code` - Get workers by phone area code

### Locations Controller (`/api/locations`)
- `GET /api/locations` - Get all locations with filtering and pagination
  - Query parameters: `pageNumber`, `pageSize`, `name`, `country`, `county`
- `GET /api/locations/{id}` - Get location by ID
- `POST /api/locations` - Create new location
- `PUT /api/locations/{id}` - Update location
- `DELETE /api/locations/{id}` - Delete location
- `GET /api/locations/by-country/{country}` - Get locations by country
- `GET /api/locations/by-county/{county}` - Get locations by county

### Health Checks
- `GET /health` - Overall application health status
- `GET /health/database` - Database connectivity health check
- `GET /health/custom` - Custom application health checks

## � Pagination

The API implements efficient pagination for all list endpoints to handle large datasets effectively:

### Pagination Parameters
- `pageNumber` (int, default: 1) - The page number to retrieve
- `pageSize` (int, default: 10, max: 1000) - Number of items per page

### Response Format
```json
{
  "items": [...],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 15,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Usage Examples
```bash
# Get first page with 20 items
GET /api/shifts?pageNumber=1&pageSize=20

# Get second page of workers
GET /api/workers?pageNumber=2&pageSize=10

# Combine with filtering
GET /api/locations?pageNumber=1&pageSize=5&country=USA
```

## �🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (Windows) or SQL Server for Linux
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/RyanW84/ShiftsLogger.git
   cd ShiftsLogger
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Set up user secrets** (for Linux SQL Server)
   ```bash
   # Run the setup script
   ./setup-user-secrets.sh  # Linux/macOS
   # OR
   setup-user-secrets.bat   # Windows

   # Then set your connection string
   cd ShiftsLoggerV2.RyanW84
   dotnet user-secrets set "ConnectionStrings:LinuxSqlServer" "Server=127.0.0.1,1433;Database=ShiftsLoggerDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=yes;Encrypt=false;MultipleActiveResultSets=yes"
   ```

4. **Set up the database**
   ```bash
   dotnet ef database update --project ShiftsLoggerV2.RyanW84
   ```

5. **Run the API**
   ```bash
   dotnet run --project ShiftsLoggerV2.RyanW84
   ```

6. **Run the console frontend** (in a separate terminal)
   ```bash
   dotnet run --project ConsoleFrontEnd
   ```

### Quick Start with VS Code Tasks

The project includes pre-configured VS Code tasks:

- **Run ShiftsLogger API (background)** - Starts the API server
- **Run ConsoleFrontEnd** - Starts the console application
- **wait-for-backend** - Utility to wait for API availability

## 🗃️ Database Configuration

### Connection Strings

The application automatically selects the appropriate connection string based on the operating system:

#### Windows (LocalDB)
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ShiftsLoggerDb;Trusted_Connection=yes;TrustServerCertificate=yes;"
```

#### Linux (SQL Server)
For Linux environments, configure the connection string using user secrets:

```bash
# Navigate to the API project directory
cd ShiftsLoggerV2.RyanW84

# Set the Linux SQL Server connection string in user secrets
dotnet user-secrets set "ConnectionStrings:LinuxSqlServer" "Server=127.0.0.1,1433;Database=ShiftsLoggerDb;User Id=sa;Password=YourSecurePassword;TrustServerCertificate=yes;Encrypt=false;MultipleActiveResultSets=yes"
```

**Security Note**: Never commit database passwords to source control. Always use user secrets or environment variables for sensitive configuration.

#### Testing (In-Memory)
For testing environments, the application uses Entity Framework's in-memory database provider.

### Migrations

Entity Framework migrations are located in the `Migrations/` folder:
- `20250826125258_InitialCreate.cs` - Initial database schema

To add a new migration:
```bash
dotnet ef migrations add <MigrationName> --project ShiftsLoggerV2.RyanW84
```

## 🧪 Testing

The project includes a comprehensive test suite with **55+ tests** covering:

### Test Categories
- **Model Tests** (24 tests) - Entity validation and behavior
- **Service Tests** (29+ tests) - Business logic and data operations
- **Controller Tests** - API endpoint behavior
- **Integration Tests** - End-to-end API testing

### Running Tests

Execute all tests:
```bash
./run-tests.sh
```

Or run specific test categories:
```bash
# Run only unit tests
dotnet test --filter "Category!=Integration"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Technologies
- **xUnit** - Primary testing framework
- **Moq** - Mocking framework for dependencies
- **FluentAssertions** - Expressive assertions
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing
- **InMemory Database** - Isolated test database

For detailed testing information, see [TEST_DOCUMENTATION.md](./TEST_DOCUMENTATION.md).

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow SOLID principles
- Maintain test coverage above 80%
- Use meaningful commit messages
- Update documentation for new features
- Ensure all tests pass before submitting PRs

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**RyanW84** - [GitHub Profile](https://github.com/RyanW84)

## 🙏 Acknowledgments

- The CSharp Academy
- The CSharp School (premium membership of C# Academy)
- Pablo De Souza
- [The C sharp School API module](https://thecsharpschool.getlearnworlds.com/course/aspnet-core-web-apis)
- [Back-end Web Development with .NET for Beginners](https://learn.microsoft.com/en-us/shows/back-end-web-development-with-dotnet-for-beginners/)
- [Introduction to web APIs](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Client-side_APIs/Introduction)
- [Tutorial: Create a Controller-based Web Api](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-9.0&preserve-view=true&tabs=visual-studio)
