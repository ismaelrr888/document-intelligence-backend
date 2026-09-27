# Document Intelligence Platform --- Backend

## 1. Overview

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

------------------------------------------------------------------------

# 2. Current Technology Stack

## Backend

-   .NET 10
-   C#
-   ASP.NET Core
-   Entity Framework Core 10
-   SQL Server 2022
-   Docker / Docker Compose
-   OpenAPI

## Architecture

-   Clean Architecture
-   Domain-driven boundaries
-   REST APIs
-   JWT authentication
-   Asynchronous processing where appropriate

## Planned infrastructure

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

# 3. Architectural Principles

## 3.1 Build incrementally

The platform must be developed in vertical slices.

Each milestone should produce working functionality.

We should avoid implementing infrastructure or abstractions before they
are needed.

## 3.2 Avoid unnecessary microservices

The architecture should use services when they represent meaningful
business or technical boundaries.

We explicitly want to avoid creating a large number of
repositories/services for functionality that can reasonably live
together.

The goal is:

``` text
Clear boundaries
+
Independent responsibilities
+
Low operational complexity
```

not:

``` text
More microservices = better architecture
```

## 3.3 Clean Architecture

Each service should follow the same general structure:

``` text
Service
├── Api
├── Application
├── Domain
└── Infrastructure
```

Responsibilities:

### Domain

Contains business entities, value objects, domain rules and domain
abstractions.

The Domain layer should not depend on:

-   ASP.NET Core
-   Entity Framework
-   SQL Server
-   external APIs
-   infrastructure implementations

### Application

Contains use cases and application-level business orchestration.

Examples:

``` text
RegisterUser
LoginUser
CreateDocument
UploadDocument
ProcessDocument
GetDocument
```

The Application layer depends on abstractions rather than concrete
infrastructure implementations.

### Infrastructure

Contains implementations for:

-   EF Core
-   repositories
-   databases
-   file storage
-   external services
-   authentication providers
-   AI/OCR providers

### API

Contains:

-   Controllers/endpoints
-   HTTP contracts
-   request/response DTOs
-   authentication configuration
-   API-specific concerns

Controllers should remain thin and delegate work to Application use
cases.

------------------------------------------------------------------------

# 4. Planned Backend Services

The platform will evolve around a small number of meaningful services.

## 4.1 Auth Service

Responsibility:

``` text
Identity
Authentication
Authorization foundation
```

Initial functionality:

``` text
POST /auth/register
POST /auth/login
GET  /auth/me
```

Current status:

-   User entity implemented
-   User repository implemented
-   Password hashing implemented
-   Register use case implemented
-   Register endpoint implemented
-   EF Core configured
-   SQL Server configured
-   Database migrations created
-   OpenAPI configured
-   JWT authentication: current milestone

------------------------------------------------------------------------

## 4.2 Documents Service

Responsibility:

``` text
Document lifecycle
Document metadata
Ownership
Upload
Retrieval
Document status
```

Planned endpoints:

``` text
POST   /documents
GET    /documents
GET    /documents/{id}
GET    /documents/{id}/status
GET    /documents/{id}/result
DELETE /documents/{id}
```

The Documents Service should own document metadata and lifecycle.

It should not contain OCR/AI processing logic.

------------------------------------------------------------------------

## 4.3 Processing Service / Worker

Responsibility:

``` text
Asynchronous document processing
```

Conceptual flow:

``` text
Document
   ↓
Processing Request
   ↓
Worker
   ↓
OCR
   ↓
Classification
   ↓
Extraction
   ↓
Validation
   ↓
Processing Result
```

The processing implementation should be decoupled from the HTTP request
lifecycle.

The initial implementation can be simple and local.

A message broker should only be introduced when asynchronous processing
requires it.

------------------------------------------------------------------------

## 4.4 Intelligence Layer

The intelligence layer will eventually provide:

-   AI-assisted classification
-   Structured extraction
-   Confidence scoring
-   Validation
-   Document understanding
-   Business rules
-   Agent-based processing where appropriate

Example:

``` json
{
  "documentType": "invoice",
  "confidence": 0.97,
  "data": {
    "invoiceNumber": "INV-123",
    "supplier": "ACME",
    "total": 1250.50,
    "currency": "EUR"
  }
}
```

The system should distinguish between:

``` text
Raw document
      ↓
Extracted information
      ↓
Validated information
      ↓
Business intelligence
```

------------------------------------------------------------------------

## 4.5 Action / Integration Layer

The final stage of the platform is:

``` text
Document
   ↓
Intelligence
   ↓
Action
```

Potential actions include:

-   Generate reports
-   Send emails
-   Create tickets
-   Update external systems
-   Trigger workflows
-   Call APIs
-   Execute agent/MCP-based integrations

This layer will be designed after the document intelligence pipeline is
stable.

------------------------------------------------------------------------

# 5. High-Level Architecture

The intended architecture is:

``` text
                         ┌───────────────────┐
                         │      Client       │
                         └─────────┬─────────┘
                                   │
                                   ▼
                         ┌───────────────────┐
                         │     Auth API      │
                         └─────────┬─────────┘
                                   │
                                  JWT
                                   │
                                   ▼
                         ┌───────────────────┐
                         │   Documents API   │
                         └─────────┬─────────┘
                                   │
                              Document
                                   │
                                   ▼
                         ┌───────────────────┐
                         │ Processing Worker │
                         └─────────┬─────────┘
                                   │
                         ┌─────────┼─────────┐
                         ▼         ▼         ▼
                        OCR  Classification Extraction
                                   │
                                   ▼
                              Validation
                                   │
                                   ▼
                             Intelligence
                                   │
                                   ▼
                                Actions
```

------------------------------------------------------------------------

# 6. Data Ownership

A fundamental architectural rule is that each service owns its data.

Initial ownership:

``` text
Auth Service
    └── Users / Identity

Documents Service
    └── Documents / Metadata / Lifecycle

Processing
    └── Processing state / Results
```

The actual document binary should eventually be stored in object/blob
storage rather than SQL Server.

SQL Server should primarily store structured application data and
metadata.

------------------------------------------------------------------------

# 7. Document Lifecycle

A document should move through explicit states.

Initial state model:

``` text
Uploaded
    ↓
Processing
    ↓
Processed
```

Failure path:

``` text
Processing
    ↓
Failed
```

Future states may include:

``` text
Validated
Rejected
Archived
```

but they should only be introduced when the product requires them.

------------------------------------------------------------------------

# 8. Authentication Flow

The authentication flow is:

``` text
Register
   ↓
User stored in database
   ↓
Login
   ↓
Password verification
   ↓
JWT generated
   ↓
Client sends Bearer token
   ↓
[Authorize]
   ↓
Authenticated request
```

Example:

``` http
Authorization: Bearer <JWT>
```

JWT configuration must not contain hardcoded secrets in source code.

Secrets should be provided through appropriate configuration mechanisms
such as environment variables or development secrets.

------------------------------------------------------------------------

# 9. Repository Structure

Current structure:

``` text
document-intelligence-backend/
│
├── apps/
│   └── auth-service/
│       └── src/
│           ├── Auth.Api/
│           ├── Auth.Application/
│           ├── Auth.Domain/
│           └── Auth.Infrastructure/
│
├── DocumentIntelligence.sln
│
└── ...
```

Planned evolution:

``` text
document-intelligence-backend/
│
├── apps/
│   ├── auth-service/
│   │   └── src/
│   │       ├── Auth.Api/
│   │       ├── Auth.Application/
│   │       ├── Auth.Domain/
│   │       └── Auth.Infrastructure/
│   │
│   ├── documents-service/
│   │   └── src/
│   │       ├── Documents.Api/
│   │       ├── Documents.Application/
│   │       ├── Documents.Domain/
│   │       └── Documents.Infrastructure/
│   │
│   └── processing-service/
│       └── src/
│           ├── Processing.Application/
│           ├── Processing.Domain/
│           └── Processing.Infrastructure/
│
├── infra/
│   ├── docker/
│   └── terraform/
│
├── docs/
│
└── DocumentIntelligence.sln
```

This is a target structure, not a requirement to create every directory
immediately.

------------------------------------------------------------------------

# 10. Current Development Status

## Milestone 1 --- Authentication

### Completed

-   [x] .NET 10 solution
-   [x] Auth Service structure
-   [x] Clean Architecture layers
-   [x] User domain entity
-   [x] User repository abstraction
-   [x] User repository implementation
-   [x] Password hashing abstraction
-   [x] Password hashing implementation
-   [x] Auth service abstraction
-   [x] Register use case
-   [x] Register endpoint
-   [x] AuthDbContext
-   [x] EF Core configuration
-   [x] SQL Server Docker setup
-   [x] EF migrations
-   [x] Database creation
-   [x] OpenAPI
-   [x] Dependency Injection

### Current task

-   [ ] Login endpoint
-   [ ] JWT token service
-   [ ] JWT Bearer authentication
-   [ ] Protected `/auth/me`
-   [ ] Authentication tests

------------------------------------------------------------------------

# 11. Roadmap

## Phase 1 --- Authentication

``` text
Register
   ↓
Login
   ↓
JWT
   ↓
Authorization
   ↓
/auth/me
   ↓
Tests
```

## Phase 2 --- Documents

``` text
Documents Service
   ↓
Document entity
   ↓
Metadata
   ↓
Upload
   ↓
Storage
   ↓
Document lifecycle
```

## Phase 3 --- Processing

``` text
Processing abstraction
   ↓
Worker
   ↓
Document status
   ↓
Processing result
```

## Phase 4 --- Intelligence

``` text
OCR
   ↓
Classification
   ↓
Extraction
   ↓
Validation
   ↓
AI / LLM integration
```

## Phase 5 --- Actions

``` text
Validated information
   ↓
Business rules
   ↓
Actions
   ↓
External integrations
```

## Phase 6 --- Infrastructure

Only after the application architecture is proven locally:

``` text
Docker
   ↓
CI/CD
   ↓
Cloud
   ↓
Terraform
   ↓
Observability
   ↓
Scaling
```

------------------------------------------------------------------------

# 12. Testing Strategy

Testing should be introduced alongside features.

## Unit tests

Focus on:

-   Domain rules
-   Application use cases
-   Authentication
-   Document lifecycle
-   Processing logic

## Integration tests

Focus on:

``` text
API
 ↓
Application
 ↓
Infrastructure
 ↓
SQL Server
```

Important integration scenarios include:

-   Register user
-   Login
-   JWT authentication
-   Protected endpoints
-   Create document
-   Retrieve document
-   Document processing lifecycle

------------------------------------------------------------------------

# 13. Local Development

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

# 14. Database Strategy

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

# 15. Security Principles

The backend must follow these principles:

-   Never store plaintext passwords.
-   Never hardcode secrets.
-   Never expose passwords through API responses.
-   Use JWT for authenticated API requests.
-   Keep authorization separate from authentication.
-   Validate input at API boundaries.
-   Avoid exposing unnecessary internal errors.
-   Avoid user enumeration through authentication errors.
-   Keep service boundaries explicit.
-   Do not trust client-provided ownership information.

Security should be treated as part of the architecture rather than as a
final phase.

------------------------------------------------------------------------

# 16. Architectural Decisions

## Decision: Clean Architecture

Chosen to keep business logic independent from infrastructure and
frameworks.

## Decision: .NET 10

Chosen as the current backend platform for the project.

## Decision: SQL Server

Chosen as the initial relational database.

## Decision: Object storage for files

Documents should eventually live in blob/object storage rather than
inside SQL Server.

## Decision: Asynchronous processing

Document processing should not block HTTP requests.

## Decision: Incremental infrastructure

Queues, cloud infrastructure, Terraform and additional
distributed-system components should be introduced when justified by
actual requirements.

## Decision: Limited number of services

The system should have a small number of meaningful services rather than
decomposing every capability into its own microservice.

------------------------------------------------------------------------

# 17. Engineering Rules for AI Coding Agents

AI coding agents such as Codex should follow these rules:

1.  Read this README before making architectural changes.
2.  Inspect existing code before creating new abstractions.
3.  Reuse existing interfaces and services when appropriate.
4.  Do not duplicate functionality.
5.  Do not introduce new frameworks without a concrete reason.
6.  Do not create new microservices unless the architecture requires
    them.
7.  Do not implement future roadmap items prematurely.
8.  Keep controllers/endpoints thin.
9.  Keep business rules out of Infrastructure.
10. Keep Domain independent from Infrastructure.
11. Add tests with meaningful new functionality.
12. Run `dotnet build` after significant changes.
13. Run the test suite before considering a milestone complete.
14. Prefer simple solutions over premature distributed-system
    complexity.
15. When an architectural decision is unclear, document the trade-off
    before making a large structural change.

------------------------------------------------------------------------

# 18. Current Next Step

The immediate implementation target is:

``` text
AUTHENTICATION

POST /auth/register
        ↓
POST /auth/login
        ↓
JWT
        ↓
GET /auth/me
        ↓
Authorization
        ↓
Tests
```

Do not begin Documents Service until this authentication milestone is
working and tested.

After authentication is complete, the next milestone is:

``` text
DOCUMENTS SERVICE
```

------------------------------------------------------------------------

# 19. Product Vision

The long-term platform is:

``` text
                 DOCUMENT
                    │
                    ▼
              INGESTION
                    │
                    ▼
                  OCR
                    │
                    ▼
            CLASSIFICATION
                    │
                    ▼
              EXTRACTION
                    │
                    ▼
              VALIDATION
                    │
                    ▼
             INTELLIGENCE
                    │
                    ▼
                 ACTION
                    │
        ┌───────────┼───────────┐
        ▼           ▼           ▼
      Report      Email       External
                              Systems
```

The backend should evolve toward this vision without prematurely
implementing all of it.

The primary engineering objective is to maintain a clean, understandable
foundation that can scale from a local MVP to a production B2B platform.
