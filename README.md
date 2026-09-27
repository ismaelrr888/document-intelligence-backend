# Document Intelligence Platform --- Backend

## Overview

Document Intelligence Platform is a B2B backend platform designed to
transform unstructured documents into structured, validated information
and eventually trigger business actions.

The core product flow is:

``` text
Document → Intelligence → Action
```

The platform is intended to ingest documents such as:

-   PDFs
-   Invoices
-   Contracts
-   Emails
-   Other business documents

The system will progressively provide:

1.  Document ingestion
2.  Document storage and metadata management
3.  OCR / text extraction
4.  Document classification
5.  Structured data extraction
6.  Validation
7.  AI-assisted intelligence
8.  Automated actions and integrations

The initial goal is not to build the entire platform at once. We will
build a small, reliable foundation and evolve it incrementally.

For architecture, roadmap, testing strategy, security principles and
other project documentation, see [docs/](docs/).

------------------------------------------------------------------------

## Technology Stack

### Backend

-   .NET 10
-   C#
-   ASP.NET Core
-   Entity Framework Core 10
-   SQL Server 2022
-   Docker / Docker Compose
-   OpenAPI

### Architecture

-   Clean Architecture
-   Domain-driven boundaries
-   REST APIs
-   JWT authentication
-   Asynchronous processing where appropriate

### Planned infrastructure

Infrastructure will be introduced progressively:

-   Object/blob storage
-   Message queues
-   Background workers
-   CI/CD
-   Terraform
-   Cloud deployment

Potential technologies such as RabbitMQ, Kafka, Azure services, AWS
services, Cloudflare services, etc. should only be introduced when there
is a concrete architectural requirement.

------------------------------------------------------------------------

## Local Development

SQL Server runs through Docker.

The application can run directly using the .NET SDK during development.

Example:

``` bash
docker compose up -d
```

Build:

``` bash
dotnet build
```

Run Auth API:

``` bash
dotnet run --project apps/auth-service/src/Auth.Api
```

Current development API:

``` text
http://localhost:5044
```

------------------------------------------------------------------------

## Database Strategy

EF Core migrations are the source-controlled mechanism for database
schema evolution.

Typical workflow:

``` bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Database:

``` text
DocumentProcessorDb
```

The database name may be renamed later if the platform adopts a broader
naming convention.

------------------------------------------------------------------------

## Documentation

-   [Architecture](docs/architecture.md) --- architectural principles,
    planned services, high-level architecture, data ownership, document
    lifecycle, authentication flow, repository structure and
    architectural decisions.
-   [Roadmap](docs/roadmap.md) --- current development status, phased
    roadmap and next steps.
-   [Testing Strategy](docs/testing.md)
-   [Security Principles](docs/security.md)
-   [Engineering Rules for AI Coding Agents](docs/agent-guidelines.md)
-   [Product Vision](docs/product-vision.md)
-   [CLI](docs/CLI.md)
-   [Auth API Architecture and Patterns](docs/Auth_API_Architecture_and_Patterns.md)
