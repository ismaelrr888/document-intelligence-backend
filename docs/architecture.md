# Architecture

## Architectural Principles

### Build incrementally

The platform must be developed in vertical slices.

Each milestone should produce working functionality.

We should avoid implementing infrastructure or abstractions before they
are needed.

### Avoid unnecessary microservices

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

### Clean Architecture

Each service should follow the same general structure:

``` text
Service
├── Api
├── Application
├── Domain
└── Infrastructure
```

Responsibilities:

#### Domain

Contains business entities, value objects, domain rules and domain
abstractions.

The Domain layer should not depend on:

-   ASP.NET Core
-   Entity Framework
-   SQL Server
-   external APIs
-   infrastructure implementations

#### Application

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

#### Infrastructure

Contains implementations for:

-   EF Core
-   repositories
-   databases
-   file storage
-   external services
-   authentication providers
-   AI/OCR providers

#### API

Contains:

-   Controllers/endpoints
-   HTTP contracts
-   request/response DTOs
-   authentication configuration
-   API-specific concerns

Controllers should remain thin and delegate work to Application use
cases.

------------------------------------------------------------------------

## Planned Backend Services

The platform will evolve around a small number of meaningful services.

### Auth Service

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

### Documents Service

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

### Processing Service / Worker

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

### Intelligence Layer

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

### Action / Integration Layer

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

## High-Level Architecture

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

## Data Ownership

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

## Document Lifecycle

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

## Authentication Flow

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
Client sends ******
   ↓
[Authorize]
   ↓
Authenticated request
```

Example:

``` http
Authorization: ******
```

JWT configuration must not contain hardcoded secrets in source code.

Secrets should be provided through appropriate configuration mechanisms
such as environment variables or development secrets.

------------------------------------------------------------------------

## Repository Structure

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

## Architectural Decisions

### Decision: Clean Architecture

Chosen to keep business logic independent from infrastructure and
frameworks.

### Decision: .NET 10

Chosen as the current backend platform for the project.

### Decision: SQL Server

Chosen as the initial relational database.

### Decision: Object storage for files

Documents should eventually live in blob/object storage rather than
inside SQL Server.

### Decision: Asynchronous processing

Document processing should not block HTTP requests.

### Decision: Incremental infrastructure

Queues, cloud infrastructure, Terraform and additional
distributed-system components should be introduced when justified by
actual requirements.

### Decision: Limited number of services

The system should have a small number of meaningful services rather than
decomposing every capability into its own microservice.
