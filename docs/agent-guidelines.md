# Engineering Rules for AI Coding Agents

AI coding agents such as Codex should follow these rules:

1.  Read the project docs (`docs/`) before making architectural changes.
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
