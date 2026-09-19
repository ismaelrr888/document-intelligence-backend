.NET CLI Cheat Sheet — Document Intelligence Backend

Comandos de .NET CLI que usamos o que vamos a usar habitualmente en este proyecto.

1. Soluciones (.sln)

Crear una solución

Desde la carpeta donde queremos que viva la solución:

dotnet new sln -n DocumentIntelligence

Ver la solución

dotnet sln list

Añadir un proyecto existente a la solución

dotnet sln add path/to/project.csproj

Ejemplo:

dotnet sln add apps/auth-service/src/Auth.Domain/Auth.Domain.csproj

Podemos añadir varios proyectos:

dotnet sln add apps/auth-service/src/Auth.Domain/Auth.Domain.csproj
dotnet sln add apps/auth-service/src/Auth.Application/Auth.Application.csproj
dotnet sln add apps/auth-service/src/Auth.Infrastructure/Auth.Infrastructure.csproj
dotnet sln add apps/auth-service/src/Auth.Api/Auth.Api.csproj

2. Crear proyectos dentro de una carpeta

La idea que usamos es:

apps/
└── auth-service/
└── src/
├── Auth.Domain/
├── Auth.Application/
├── Auth.Infrastructure/
└── Auth.Api/

Crear una Class Library

Entramos en la carpeta donde queremos crearla:

cd apps/auth-service/src
dotnet new classlib -n Auth.Domain

Esto crea:

Auth.Domain/
├── Auth.Domain.csproj
└── Class1.cs

Crear la Web API

cd apps/auth-service/src
dotnet new webapi -n Auth.Api

3. Crear una aplicación/servicio desde cero

Para un nuevo servicio:

mkdir -p apps/docs-service/src
cd apps/docs-service/src

Crear sus proyectos:

dotnet new classlib -n Docs.Domain
dotnet new classlib -n Docs.Application
dotnet new classlib -n Docs.Infrastructure
dotnet new webapi -n Docs.Api

La estructura queda:

apps/
└── docs-service/
└── src/
├── Docs.Domain/
├── Docs.Application/
├── Docs.Infrastructure/
└── Docs.Api/

4. Referencias entre proyectos

Una referencia indica que un proyecto puede utilizar otro proyecto.

Ejemplo:

Auth.Api
↓
Auth.Application
↓
Auth.Domain

Auth.Infrastructure
↓
Auth.Application
↓
Auth.Domain

Añadir una referencia

dotnet add <proyecto> reference <proyecto-referenciado>

Ejemplo:

dotnet add Auth.Application/Auth.Application.csproj \
reference Auth.Domain/Auth.Domain.csproj

Otro:

dotnet add Auth.Api/Auth.Api.csproj \
reference Auth.Application/Auth.Application.csproj

Y:

dotnet add Auth.Infrastructure/Auth.Infrastructure.csproj \
reference Auth.Application/Auth.Application.csproj

5. NuGet

Instalar un paquete

Importante: el paquete se instala en el proyecto que lo necesita, no globalmente.

dotnet add <proyecto.csproj> package <PACKAGE>

Ejemplo:

dotnet add Auth.Infrastructure/Auth.Infrastructure.csproj \
package Microsoft.EntityFrameworkCore.SqlServer

Así el paquete queda asociado a Auth.Infrastructure.

Instalar una versión concreta

dotnet add <proyecto.csproj> package <PACKAGE> --version <VERSION>

Ejemplo:

dotnet add Auth.Infrastructure/Auth.Infrastructure.csproj \
package Microsoft.EntityFrameworkCore.SqlServer \
--version 10.0.12

Ver paquetes instalados

dotnet list <proyecto.csproj> package

6. Restaurar dependencias

Normalmente .NET lo hace automáticamente, pero podemos hacerlo manualmente:

dotnet restore

7. Compilar

Desde la solución:

dotnet build

También podemos compilar un proyecto concreto:

dotnet build apps/auth-service/src/Auth.Api/Auth.Api.csproj

8. Ejecutar una API

Desde la carpeta del proyecto:

dotnet run

O indicando el .csproj:

dotnet run --project apps/auth-service/src/Auth.Api/Auth.Api.csproj

9. Ver versión de .NET

dotnet --version

Ver SDKs instalados

dotnet --list-sdks

Ver runtimes instalados

dotnet --list-runtimes

10. Entity Framework Core

Crear una migration

Desde la raíz del repo, indicando el proyecto donde vive el DbContext:

dotnet ef migrations add InitialCreate \
--project apps/auth-service/src/Auth.Infrastructure \
--startup-project apps/auth-service/src/Auth.Api

Aplicar migrations

dotnet ef database update \
--project apps/auth-service/src/Auth.Infrastructure \
--startup-project apps/auth-service/src/Auth.Api

Listar migrations

dotnet ef migrations list \
--project apps/auth-service/src/Auth.Infrastructure \
--startup-project apps/auth-service/src/Auth.Api

Eliminar la última migration

dotnet ef migrations remove \
--project apps/auth-service/src/Auth.Infrastructure \
--startup-project apps/auth-service/src/Auth.Api

11. Patrones de comandos que queremos recordar

Crear un proyecto aquí

dotnet new <template> -n <ProjectName>

Añadir proyecto a la solución

dotnet sln add <path-to-csproj>

Referenciar otro proyecto

dotnet add <project.csproj> reference <other.csproj>

Añadir NuGet a un proyecto específico

dotnet add <project.csproj> package <PACKAGE>

Ejecutar un proyecto específico

dotnet run --project <path-to-csproj>

Concepto clave

En este proyecto distinguimos:

Solution (.sln)
│
├── Auth.Domain
├── Auth.Application
├── Auth.Infrastructure
└── Auth.Api

La solution organiza proyectos.

Los proyectos (.csproj) contienen el código y sus dependencias.

Las project references conectan nuestros proyectos.

Los NuGet packages se instalan en el proyecto que realmente los necesita.