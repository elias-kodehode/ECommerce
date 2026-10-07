# ECommerce

A portfolio e-commerce backend built with .NET 10, ASP.NET Core Minimal APIs,
.NET Aspire, PostgreSQL, and Entity Framework Core.

The project currently provides a tested product-catalog API. It uses vertical
feature slices, lightweight command/query dispatching, FluentValidation, typed
results, OpenAPI documentation, and in-memory caching for product lookups.

## Features

- Create, retrieve, update, and delete products
- Unique, normalized product SKUs
- Product pricing and stock tracking
- Paginated product listing
- Request validation and Problem Details error responses
- PostgreSQL persistence with EF Core migrations
- Cached product-by-ID queries with cache invalidation after writes
- OpenAPI documentation and Scalar API reference
- Aspire orchestration, health checks, and observability defaults
- Handler, endpoint, validation, and OpenAPI tests

## API endpoints

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/products` | Create a product |
| `GET` | `/products` | List products with pagination |
| `GET` | `/products/{id}` | Retrieve a product by ID |
| `PATCH` | `/products/{id}` | Partially update a product |
| `DELETE` | `/products/{id}` | Delete a product |

The paginated product endpoint accepts `page` and `pageSize` query parameters.
Page size must be between 1 and 100.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop using Linux containers

## Run locally

Start Docker Desktop, then run:

```powershell
dotnet run --project ECommerce.AppHost
```

The AppHost starts PostgreSQL, creates the `ecommerce` database, applies the EF
Core migrations, and starts the API after the database is ready. Aspire supplies
the connection string automatically, so database credentials are not stored in
the repository. PostgreSQL data persists in a Docker volume between runs.

Open the Aspire dashboard URL printed in the terminal. In development, the API
exposes:

- `/scalar/v1` for the interactive API reference
- `/openapi/v1.json` for the OpenAPI document
- `/health` for application and database health
- `/alive` for the liveness check

## Run the tests

```powershell
dotnet test ECommerce.slnx
```

The test project covers product handlers, HTTP endpoints, validation behavior,
and the generated OpenAPI document.

## Project structure

```text
ECommerce.AppHost/          Aspire application orchestration
ECommerce.ServiceDefaults/ Shared health and observability configuration
src/ECommerce.Api/         API, domain model, persistence, and feature slices
tests/ECommerce.Api.Tests/ Automated API tests
```

Product functionality is organized by use case under
`src/ECommerce.Api/Features/Products`. Shared messaging, result, endpoint, and
OpenAPI infrastructure lives under `src/ECommerce.Api/Common`.

## Roadmap

- Expand the catalog with product descriptions, categories, images, searching,
  filtering, and sorting
- Add customer accounts and authentication
- Add carts, orders, and transactional inventory updates
- Integrate a payment provider in test mode
- Add a storefront client and deployment pipeline
