# Development Status and Roadmap

## Current Development Status

### Milestone 1 --- Authentication

#### Completed

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

#### Current task

-   [ ] Login endpoint
-   [ ] JWT token service
-   [ ] JWT ******
-   [ ] Protected `/auth/me`
-   [ ] Authentication tests

------------------------------------------------------------------------

## Roadmap

### Phase 1 --- Authentication

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

### Phase 2 --- Documents

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

### Phase 3 --- Processing

``` text
Processing abstraction
   ↓
Worker
   ↓
Document status
   ↓
Processing result
```

### Phase 4 --- Intelligence

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

### Phase 5 --- Actions

``` text
Validated information
   ↓
Business rules
   ↓
Actions
   ↓
External integrations
```

### Phase 6 --- Infrastructure

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

## Current Next Step

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
