# Security Principles

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
