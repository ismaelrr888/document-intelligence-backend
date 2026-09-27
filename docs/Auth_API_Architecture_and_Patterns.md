# Auth API — .NET Architecture & Patterns Notes

> Working notes for the Auth API.  
> Goal: learn .NET and software architecture by applying each concept to the project instead of memorizing isolated definitions.

---

## 1. Clean Architecture

The most important rule:

> **Inner layers do not know about outer layers. Outer layers may depend on inner layers.**

Think of it as an onion:

```text
┌──────────────────────────────────────────────┐
│                    Auth.Api                  │
│                                              │
│   ┌──────────────────────────────────────┐   │
│   │           Infrastructure             │   │
│   │                                      │   │
│   │   ┌──────────────────────────────┐   │   │
│   │   │        Application           │   │   │
│   │   │                              │   │   │
│   │   │   ┌──────────────────────┐   │   │   │
│   │   │   │       Domain         │   │   │   │
│   │   │   └──────────────────────┘   │   │   │
│   │   └──────────────────────────────┘   │   │
│   └──────────────────────────────────────┘   │
└──────────────────────────────────────────────┘
```

### Dependency direction

```text
Auth.Api
   │
   ├──────────────► Auth.Application
   │
   └──────────────► Auth.Infrastructure
                         │
                         ├────────► Auth.Application
                         │
                         └────────► Auth.Domain

Auth.Application
   │
   └──────────────► Auth.Domain

Auth.Domain
   │
   └──────────────► NOTHING
```

### Key rule

```text
Outer layer  ─────────► Inner layer
Inner layer  ─────────X  Outer layer
```

So:

```text
Infrastructure ─────► Domain       ✅
Infrastructure ─────► Application  ✅
Application ─────────► Domain       ✅
Domain ──────────────X Infrastructure ❌
Domain ──────────────X API          ❌
```

---

# 2. Our Auth API Projects

```text
Auth
│
├── Auth.Api
│   └── Program.cs
│
├── Auth.Application
│   ├── Abstractions
│   │   └── Security
│   │       └── IPasswordHasher.cs
│   │
│   ├── Interfaces
│   │   └── IUserRepository.cs
│   │
│   └── Services
│       └── AuthService.cs
│
├── Auth.Domain
│   └── User
│       └── User.cs
│
└── Auth.Infrastructure
    ├── Security
    │   └── PasswordHasher.cs
    │
    ├── Repositories
    │   └── UserRepository.cs
    │
    └── DependencyInjection.cs
```

---

# 3. Dependency Injection

Dependency Injection (DI) means that a class **declares what it needs**, instead of constructing those dependencies itself.

### Without DI

```csharp
public class AuthService
{
    private readonly PasswordHasher _passwordHasher;

    public AuthService()
    {
        _passwordHasher = new PasswordHasher();
    }
}
```

`AuthService` is tightly coupled to `PasswordHasher`.

### With DI

```csharp
public class AuthService
{
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }
}
```

Now `AuthService` only knows the abstraction.

```text
AuthService
     │
     │ needs
     ▼
IPasswordHasher
     ▲
     │ implements
     │
PasswordHasher
```

---

# 4. The DI Container

ASP.NET Core has a built-in Dependency Injection container.

We register mappings:

```csharp
services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<AuthService>();
```

The container can then build the dependency graph.

For example:

```csharp
public AuthService(
    IPasswordHasher passwordHasher,
    IUserRepository userRepository)
{
    _passwordHasher = passwordHasher;
    _userRepository = userRepository;
}
```

The container resolves:

```text
                     AuthService
                    /           \
                   /             \
                  ▼               ▼
       IPasswordHasher       IUserRepository
              │                     │
              ▼                     ▼
      PasswordHasher         UserRepository
                                   │
                                   ▼
                         ILogger<UserRepository>
```

The important part:

> We do not manually instantiate every object with `new`.

---

# 5. Composition Root

`Auth.Api` is the place where the application is assembled.

This is often called the **Composition Root**.

```csharp
builder.Services.AddInfrastructure();
builder.Services.AddScoped<AuthService>();
```

Conceptually:

```text
                    Auth.Api
                 Composition Root
                       │
              ┌────────┴────────┐
              ▼                 ▼
       Application        Infrastructure
              │                 │
              │                 ├── PasswordHasher
              │                 └── UserRepository
              │
              └── AuthService
```

The API knows which concrete implementations should be used.

Application does not need to know.

---

# 6. Infrastructure Dependency Registration

We created an extension method:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
```

Then `Program.cs` only needs:

```csharp
builder.Services.AddInfrastructure();
```

This keeps infrastructure registration together.

### Why not put AuthService here?

Because `AuthService` belongs to Application.

```text
Application
└── AuthService

Infrastructure
├── PasswordHasher
└── UserRepository
```

So:

```csharp
builder.Services.AddScoped<AuthService>();
builder.Services.AddInfrastructure();
```

---

# 7. Abstraction vs Implementation

An interface describes **what something can do**.

An implementation describes **how it does it**.

Example:

```csharp
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}
```

Infrastructure implements it:

```csharp
public class UserRepository : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email)
    {
        // Database implementation eventually goes here
        throw new NotImplementedException();
    }
}
```

Dependency direction:

```text
Application
    │
    │ defines
    ▼
IUserRepository
    ▲
    │ implements
    │
Infrastructure
UserRepository
```

This allows us to replace the implementation without changing Application.

---

# 8. Async / Await

A method ending in `Async` is a naming convention.

```csharp
GetByEmailAsync()
```

The important part is usually the return type:

```csharp
Task<User?>
```

Meaning:

> The operation will eventually produce a `User` or `null`.

Example:

```csharp
public async Task<User?> GetByEmailAsync(string email)
{
    var user = await database.FindUserAsync(email);

    return user;
}
```

### Why repositories use async

Database and network operations are I/O operations.

We don't want a thread sitting blocked while waiting for the database.

```text
Request
  │
  ▼
Repository
  │
  ├── send query ──────────────► Database
  │
  │                             processing...
  │
  └── await
        │
        ▼
     continue
        │
        ▼
      User
```

`Async` does **not** automatically mean "run on another thread" or "parallel".

---

# 9. ILogger and Nested Dependencies

We added:

```csharp
private readonly ILogger<UserRepository> _logger;

public UserRepository(ILogger<UserRepository> logger)
{
    _logger = logger;
}
```

Then:

```csharp
_logger.LogInformation(
    "Searching user with email {Email}",
    email);
```

We did not manually register `ILogger<UserRepository>`.

ASP.NET Core's infrastructure already knows how to provide it.

This demonstrates nested dependency resolution:

```text
IUserRepository
      │
      ▼
UserRepository
      │
      ▼
ILogger<UserRepository>
```

DI can resolve dependency trees recursively.

---

# 10. DI Lifetimes

ASP.NET Core commonly provides three lifetimes.

## Transient

A new instance is created each time it is requested.

```csharp
services.AddTransient<IMyService, MyService>();
```

```text
Request A
 ├── MyService → Instance 1
 └── MyService → Instance 2
```

Useful for lightweight, stateless services.

---

## Scoped

One instance per scope.

In a typical HTTP API:

```text
HTTP Request
     │
     ├── Service → Instance A
     ├── Repository → Instance B
     └── Another consumer → same registered scoped instance
```

Registration:

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

Scoped is commonly used for request-oriented services and database contexts.

---

## Singleton

One instance for the application's lifetime.

```csharp
services.AddSingleton<IMyService, MyService>();
```

Conceptually:

```text
Application lifetime
        │
        ▼
   One instance
```

Be careful with state and thread safety.

---

# 11. Constructor Injection

Constructor Injection is our primary DI technique.

```csharp
public class AuthService
{
    private readonly IUserRepository _repository;

    public AuthService(IUserRepository repository)
    {
        _repository = repository;
    }
}
```

The constructor makes dependencies explicit.

```text
AuthService
    │
    └── requires IUserRepository
```

This also makes testing easier because a test can provide a fake/mock implementation.

---

# 12. Static Classes

We previously discussed static classes.

A static class is used without creating an instance:

```csharp
MyUtility.DoSomething();
```

No:

```csharp
new MyUtility();
```

Static classes can be useful for genuinely stateless utilities.

But they don't participate in normal constructor-based DI because there is no object instance to inject.

---

# 13. Primary Constructors

Modern C# allows:

```csharp
public class UserRepository(
    ILogger<UserRepository> logger) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email)
    {
        logger.LogInformation("Searching user");

        throw new NotImplementedException();
    }
}
```

Instead of:

```csharp
public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ILogger<UserRepository> logger)
    {
        _logger = logger;
    }
}
```

Primary constructors reduce boilerplate.

For learning DI, the traditional constructor syntax can be clearer because the dependency flow is explicit.

---

# 14. Repository Pattern

The repository abstracts data access.

Instead of Application doing:

```csharp
// Bad coupling
var user = database.Users.FirstOrDefault(...);
```

Application asks:

```csharp
var user = await _userRepository.GetByEmailAsync(email);
```

The implementation decides how data is retrieved.

```text
Application
     │
     ▼
IUserRepository
     ▲
     │
     ▼
UserRepository
     │
     ▼
Database
```

This becomes especially useful when we introduce Entity Framework Core.

---

# 15. Abstract Factory

The **Factory Method** generally focuses on creating one kind of product.

The **Abstract Factory** creates families of related products.

### Factory Method

```text
Factory
   │
   └── creates one product
             │
             ▼
          Product
```

Example:

```csharp
public interface INotification
{
}

public interface INotificationFactory
{
    INotification Create();
}
```

### Abstract Factory

```text
              Abstract Factory
               /           \
              ▼             ▼
        CreateButton   CreateCheckbox
              │             │
              ▼             ▼
          Button          Checkbox
```

The important distinction:

> Abstract Factory is useful when you need to create **multiple related products that belong together**.

---

# 16. Bridge Pattern

Bridge separates an abstraction from its implementation so they can evolve independently.

```text
       Abstraction
           │
           ▼
     Implementation
```

More concretely:

```text
RemoteControl
     │
     └──────────────► Device
                         ▲
                         │
                 ┌───────┴───────┐
                 │               │
               TV             Radio
```

Instead of creating every combination:

```text
BasicRemote + TV
BasicRemote + Radio
AdvancedRemote + TV
AdvancedRemote + Radio
...
```

we compose them:

```text
Remote ─────► Device
```

This avoids combinatorial explosion.

---

# 17. SOLID — Quick Reference

## S — Single Responsibility

A class should have one reason to change.

```text
UserRepository
    → data access

PasswordHasher
    → password hashing

AuthService
    → authentication use cases
```

Avoid:

```text
AuthService
    ├── database
    ├── email
    ├── hashing
    ├── logging
    ├── HTTP
    └── PDF generation
```

---

## O — Open/Closed

Open for extension, closed for modification.

Instead of modifying a giant `switch` every time:

```csharp
switch (provider)
{
    case "A":
    case "B":
    case "C":
}
```

use abstractions and implementations when appropriate.

---

## L — Liskov Substitution

Implementations should be usable wherever their abstraction is expected without breaking the expected contract.

```text
IUserRepository
     ▲
     │
     ├── SqlUserRepository
     └── InMemoryUserRepository
```

Both should honor the interface's expected behavior.

---

## I — Interface Segregation

Prefer focused interfaces.

Instead of:

```csharp
interface IUserEverything
{
    Create();
    Delete();
    ExportPdf();
    SendEmail();
    Authenticate();
    ...
}
```

prefer smaller contracts when the consumers need only part of the behavior.

---

## D — Dependency Inversion

High-level logic should depend on abstractions, not concrete infrastructure.

```text
AuthService
     │
     ▼
IUserRepository
     ▲
     │
UserRepository
```

Not:

```text
AuthService
     │
     ▼
UserRepository
```

This principle is one of the foundations behind our architecture.

---

# 18. The Current Auth API Dependency Graph

At this stage:

```text
                         Auth.Api
                            │
                            │ composition root
                            ▼
                    ┌───────────────┐
                    │      DI       │
                    └───────┬───────┘
                            │
              ┌─────────────┴─────────────┐
              ▼                           ▼
        AuthService                 Infrastructure
              │                           │
       ┌──────┴──────┐             ┌──────┴──────┐
       ▼             ▼             ▼             ▼
IPasswordHasher  IUserRepository  PasswordHasher UserRepository
       ▲             ▲                           │
       │             │                           ▼
       │             │                 ILogger<UserRepository>
       │             │
       └─────────────┴───────────────────────────┘
```

The important architecture boundary:

```text
Application knows:

    IPasswordHasher
    IUserRepository

Application does NOT know:

    PasswordHasher
    UserRepository
```

---

# 19. Current Development Strategy

We are deliberately learning the architecture incrementally.

```text
1. Domain
      ↓
2. Application abstractions
      ↓
3. Infrastructure implementations
      ↓
4. Dependency Injection
      ↓
5. AuthService
      ↓
6. EF Core
      ↓
7. Database
      ↓
8. Authentication
      ↓
9. JWT
      ↓
10. API endpoints
      ↓
11. Testing
```

Do not introduce a pattern merely because it exists.

The rule for this project:

> **Use a pattern when the problem justifies it.**

The goal is not to create the most complicated architecture.

The goal is to create an architecture where responsibilities, dependencies, and boundaries are easy to understand.
