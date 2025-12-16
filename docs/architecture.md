# Architecture

This document describes the architectural decisions and structure of the OneStop Basket Calculator API.

## Overview

The solution follows **Clean Architecture** principles with four distinct layers:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  (Minimal API, Contracts, Exception Handler)                │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  (MediatR Handlers, Validators, Repository Interfaces)      │
│  Orchestration only - fetches data, maps DTOs, calls domain │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              ▼                               ▼
┌─────────────────────────┐     ┌─────────────────────────────┐
│      Domain Layer       │     │    Infrastructure Layer     │
│  (Entities with behavior│     │  (In-memory repositories)   │
│   Domain services -     │     │                             │
│   PURE business logic)  │     │                             │
│                         │     │                             │
└─────────────────────────┘     └─────────────────────────────┘
```

## Layer Responsibilities

### Domain Layer (`OneStop.Domain`)

**Dependencies:** None (pure .NET)

The innermost layer containing:

- **Entities:** Rich domain objects with behavior (`Product`, `DiscountPromotion`, `PointsPromotion`)
- **Value Objects:** Immutable objects defined by their values (`BasketLineItem`)
- **Domain Services:** Pure business logic (`BasketCalculatorService`)
- **Models:** Result objects (`BasketCalculationResult`)
- **Exceptions:** Domain rule violations (`DomainException`)

Key design decisions:
- Entities are **not anaemic** - they contain relevant behavior methods
- Domain service is **synchronous and pure** - no I/O, no async, no external dependencies
- This makes domain logic trivially unit testable without mocks

### Application Layer (`OneStop.Application`)

**Dependencies:** Domain, MediatR, FluentValidation

The orchestration layer containing:

- **Features:** Vertical slices with Command + Handler + Validator
- **Interfaces:** Repository contracts (implemented by Infrastructure)
- **Behaviors:** MediatR pipeline behaviors (logging, validation)
- **Exceptions:** Application-level errors (`ValidationException`, `NotFoundException`)

Key design decisions:
- Uses **CQRS pattern** via MediatR (only commands for now, queries can be added)
- **Vertical slice organization** - each feature is self-contained
- Handler is **public** for testability
- Repository interfaces defined here, implemented in Infrastructure (Dependency Inversion)

### Infrastructure Layer (`OneStop.Infrastructure`)

**Dependencies:** Application

The data access layer containing:

- **In-memory Repositories:** Seeded with sample data

Key design decisions:
- Repositories return `Task<>` for future EF Core compatibility
- Data is static for this exercise; easily replaceable with EF Core
- Case-insensitive product ID lookup

### Presentation Layer (`OneStop.Presentation`)

**Dependencies:** Application, Infrastructure

The entry point containing:

- **API Endpoints:** Minimal API endpoint definitions
- **Contracts:** HTTP request/response DTOs
- **Exception Handler:** Global error handling via `IExceptionHandler`
- **Program.cs:** DI composition and middleware setup

Key design decisions:
- **Minimal API** over Controllers (modern .NET approach)
- Contracts separate from MediatR commands (HTTP concerns vs application concerns)
- `IExceptionHandler` over middleware (modern .NET 8+ approach)

## Technology Choices

| Choice | Decision | Rationale                                           |
|--------|----------|-----------------------------------------------------|
| .NET Version | 10 | Latest preview, modern features                     |
| Minimal API | Yes | Modern .NET approach for simple endpoints           |
| FluentValidation | Yes | Declarative input validation in pipeline            |
| Serilog | Yes | Structured logging with console sink                |
| Exception Handling | IExceptionHandler | Modern .NET 8+ approach                             |
| Feature Organization | Vertical slices | Command + Handler + Validator (outside) per feature |

## Key Separation

The architecture intentionally separates:

1. **Domain Service vs Handler:**
   - Domain Service: Pure calculation logic, receives domain objects
   - Handler: Orchestration, fetches from repos, maps DTOs

2. **HTTP Contracts vs MediatR Commands:**
   - Contracts: HTTP-specific (date as string, JSON attributes)
   - Commands: Application contract (date as DateTime)

3. **Validation Layers:**
   - FluentValidation: Input validation (empty basket, missing fields)
   - Domain: Business rule validation (overlapping promotions)

## Error Handling Strategy

```
Exception Type            → HTTP Status → Error Type
──────────────────────────────────────────────────────
BadHttpRequestException   → 400         → BadRequestError
ValidationException       → 400         → ValidationError
NotFoundException         → 400         → NotFoundError
DomainException           → 400         → DomainError
ArgumentException         → 400         → ArgumentError
Other                     → 500         → InternalError
```

All errors return a consistent `ErrorResponse` structure with type, message, and optional field-level errors.
