# OneStop Basket Calculator API

A .NET 10 backend API for retail basket calculation with discounts and loyalty points, demonstrating clean architecture principles.

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [API Usage](#api-usage)
- [Project Structure](#project-structure)
- [Documentation](#documentation)
- [Running Tests](#running-tests)

## Overview

This API calculates basket totals including:
- **Discount promotions** - Date-based, product-specific, percentage discounts
- **Points promotions** - Date-based, category-filtered (Fuel/Shop/Any), points per dollar

### Key Features

- Clean Architecture with vertical slice organization
- Pure domain service for business logic (no mocks needed for unit tests)
- MediatR pipeline with validation and logging
- Serilog structured logging
- Scalar OpenAPI documentation (modern .NET 9+ approach)
- Comprehensive test coverage

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run the API

```bash
# Clone and navigate to project
cd onestop

# Restore dependencies
dotnet restore

# Run in Development mode
dotnet run --project src/OneStop.Presentation --launch-profile Development

# Or run in Production mode
dotnet run --project src/OneStop.Presentation --launch-profile Production
```

### Access Scalar API Reference

Open http://localhost:5000/scalar/v1 in your browser (Development mode only).

## API Usage

### Calculate Basket

**POST** `/api/basket/calculate`

#### Request

```json
{
  "CustomerId": "8e4e8991-aaee-495b-9f24-52d5d0e509c5",
  "LoyaltyCard": "CTX0000001",
  "TransactionDate": "10-Jan-2020",
  "Basket": [
    { "ProductId": "PRD01", "Quantity": 3 },
    { "ProductId": "PRD02", "Quantity": 10 }
  ]
}
```

#### Response

```json
{
  "CustomerId": "8e4e8991-aaee-495b-9f24-52d5d0e509c5",
  "LoyaltyCard": "CTX0000001",
  "TransactionDate": "10-Jan-2020",
  "TotalAmount": 16.60,
  "DiscountApplied": 2.60,
  "GrandTotal": 14.00,
  "PointsEarned": 28
}
```

### Available Products

| ProductId | Name          | Category | Price |
|-----------|---------------|----------|-------|
| PRD01     | Vortex 95     | Fuel     | $1.20 |
| PRD02     | Vortex 98     | Fuel     | $1.30 |
| PRD03     | Diesel        | Fuel     | $1.10 |
| PRD04     | Twix 55g      | Shop     | $2.30 |
| PRD05     | Mars 72g      | Shop     | $5.10 |
| PRD06     | SNICKERS 72G  | Shop     | $3.40 |
| PRD07     | Bounty 3 63g  | Shop     | $6.90 |
| PRD08     | Snickers 50g  | Shop     | $4.00 |

### Active Promotions

#### Discount Promotions

| Period                   | Promotion            | Discount | Products |
|--------------------------|----------------------|----------|----------|
| Jan 1 - Feb 15, 2020    | Fuel Discount Promo  | 20%      | PRD02    |
| Mar 2 - Mar 20, 2020    | Happy Promo          | 15%      | None     |

#### Points Promotions

| Period                   | Promotion      | Points/$ | Category |
|--------------------------|----------------|----------|----------|
| Jan 1 - Jan 30, 2020    | New Year Promo | 2        | Any      |
| Feb 5 - Feb 15, 2020    | Fuel Promo     | 3        | Fuel     |
| Mar 1 - Mar 20, 2020    | Shop Promo     | 4        | Shop     |

## Project Structure

```
OneStop/
├── docs/                           # Documentation
│   ├── assumptions.md              # Business assumptions
│   ├── architecture.md             # Architecture decisions
│   ├── testing.md                  # Testing strategy
│   └── extending.md                # Extension guides
├── src/
│   ├── OneStop.Domain/             # Entities, services, value objects
│   ├── OneStop.Application/        # Handlers, validators, interfaces
│   ├── OneStop.Infrastructure/     # Repository implementations
│   └── OneStop.Presentation/       # API endpoints, contracts
├── tests/
│   └── OneStop.Tests/              # Unit and integration tests
├── README.md
└── OneStop.sln
```

## Documentation

| Document | Description |
|----------|-------------|
| [Assumptions](docs/assumptions.md) | Business rules and data assumptions |
| [Architecture](docs/architecture.md) | Clean architecture explanation |
| [Testing](docs/testing.md) | Test strategy and test case mapping |
| [Extending](docs/extending.md) | How to add EF Core, caching, etc. |

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbosity
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~BasketCalculatorServiceTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Summary

- **Unit Tests:** 18 tests covering domain service logic
- **Integration Tests:** 15 tests covering handler orchestration

## Configuration

### Launch Profiles

| Profile | URL | Environment | Scalar |
|---------|-----|-------------|--------|
| Development | http://localhost:5000, https://localhost:5001 | Development | ✅ |
| Production | http://localhost:5010, https://localhost:5011 | Production | ❌ |

### Logging

Configured via `appsettings.json`:
- **Development:** Debug level, verbose output
- **Production:** Warning level, OneStop namespace at Information

## Error Responses

All errors return a consistent structure:

```json
{
  "type": "ValidationError",
  "message": "One or more validation errors occurred.",
  "errors": {
    "Basket": ["Basket cannot be empty."]
  }
}
```

| Error Type | HTTP Status | Cause |
|------------|-------------|-------|
| BadRequestError | 400 | Invalid JSON or missing required fields |
| ValidationError | 400 | Input validation failed |
| NotFoundError | 400 | Product not found |
| DomainError | 400 | Business rule violation |
| InternalError | 500 | Unexpected error |