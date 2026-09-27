# Testing Strategy

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
