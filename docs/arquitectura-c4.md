# Arquitectura de personapi-dotnet (modelo C4)

El modelo C4 describe la arquitectura del sistema de software en cuatro niveles de abstracción progresiva:

| Nivel | Diagrama | Pregunta que responde | Artefacto PNG |
|---|---|---|---|
| 1 | Contexto | ¿Quién usa el sistema y con qué se relaciona? | [`c4-nivel1-contexto.png`](diagramas/c4-nivel1-contexto.png) |
| 2 | Contenedores | ¿En qué piezas desplegables se divide la solución? | [`c4-nivel2-contenedores.png`](diagramas/c4-nivel2-contenedores.png) |
| 3 | Componentes | ¿Cómo está organizado por dentro el contenedor principal? | [`c4-nivel3-componentes.png`](diagramas/c4-nivel3-componentes.png) |
| 4 | Código | ¿Cómo se estructuran las clases y contratos del patrón DAO? | [`c4-nivel4-codigo.png`](diagramas/c4-nivel4-codigo.png) |
| - | Secuencia CRUD | ¿Cómo interactúan los componentes en CREATE, READ, UPDATE, DELETE? | [`secuencia-crud-*.png`](diagramas/) |
| - | Despliegue | ¿Cómo se distribuyen y comunican los contenedores en Docker? | [`c4-despliegue.png`](diagramas/c4-despliegue.png) |
| - | Modelo ER | ¿Cuál es el modelo relacional de la base de datos? | [`diagrama-er.png`](diagramas/diagrama-er.png) |

---

## Nivel 1 · Contexto del Sistema

Describe las interacciones de los usuarios finales y sistemas clientes con el monolito `personapi-dotnet`.

![C4 - Nivel 1: Contexto](diagramas/c4-nivel1-contexto.png)

<details>
<summary>Ver código fuente Mermaid</summary>

```mermaid
flowchart TB
    usuario["<b>Usuario administrativo</b><br/>[Persona]<br/>Registra y consulta personas,<br/>profesiones, estudios y teléfonos"]
    cliente["<b>Desarrollador / sistema cliente</b><br/>[Persona externa]<br/>Integra o prueba la API REST"]
    sistema["<b>personapi-dotnet</b><br/>[Sistema de software]<br/>Monolito que gestiona la información<br/>de personas: CRUD web y API REST"]

    usuario -. "Administra los datos<br/>[HTTP, HTML]" .-> sistema
    cliente -. "Consulta y modifica datos<br/>[HTTP, JSON/REST]" .-> sistema

    classDef persona fill:#08427B,stroke:#073B6F,color:#fff
    classDef externo fill:#8C8C8C,stroke:#6B6B6B,color:#fff
    classDef sistema fill:#1168BD,stroke:#0B4884,color:#fff
    class usuario persona
    class cliente externo
    class sistema sistema
```
</details>

---

## Nivel 2 · Contenedores

Muestra las unidades de ejecución independientes orquestadas mediante Docker Compose: aplicación web, base de datos e inicializador automatizado.

![C4 - Nivel 2: Contenedores](diagramas/c4-nivel2-contenedores.png)

<details>
<summary>Ver código fuente Mermaid</summary>

```mermaid
flowchart TB
    usuario["<b>Usuario administrativo</b><br/>[Persona]"]
    cliente["<b>Desarrollador / sistema cliente</b><br/>[Persona externa]"]

    subgraph sis ["personapi-dotnet [Sistema de software]"]
        web["<b>Aplicación web</b><br/>[Contenedor: ASP.NET Core 10 MVC, C#, EF Core 10]<br/>Vistas Razor para el CRUD, API REST en /api<br/>y documentación OpenAPI 3 en /swagger"]
        init["<b>Inicializador de BD</b><br/>[Contenedor: sqlcmd]<br/>Ejecuta DDL.sql y DML.sql<br/>una sola vez al desplegar"]
        db[("<b>persona_db</b><br/>[Contenedor: SQL Server 2025 Express]<br/>Tablas persona, profesion,<br/>estudios y telefono")]
    end

    usuario -. "Navega y envía formularios<br/>[HTTP :8080, HTML]" .-> web
    cliente -. "Llama endpoints /api/*<br/>[HTTP :8080, JSON]" .-> web
    web -. "Lee y escribe datos<br/>[EF Core, TDS :1433]" .-> db
    init -. "Crea esquema y datos de prueba<br/>[TDS :1433]" .-> db

    classDef persona fill:#08427B,stroke:#073B6F,color:#fff
    classDef externo fill:#8C8C8C,stroke:#6B6B6B,color:#fff
    classDef contenedor fill:#438DD5,stroke:#2E6295,color:#fff
    class usuario persona
    class cliente externo
    class web,init,db contenedor
    style sis fill:#ffffff,stroke:#666666,stroke-dasharray:6 4
```
</details>

---

## Nivel 3 · Componentes de la Aplicación Web

Detalla los módulos internos del contenedor web organizados bajo los patrones arquitecturales **MVC** y **DAO**.

![C4 - Nivel 3: Componentes](diagramas/c4-nivel3-componentes.png)

<details>
<summary>Ver código fuente Mermaid</summary>

```mermaid
flowchart TB
    usuario["<b>Usuario administrativo</b><br/>[Persona]"]
    cliente["<b>Cliente REST</b><br/>[Persona externa]"]

    subgraph web ["Aplicación web [Contenedor: ASP.NET Core 10]"]
        mvc["<b>Controladores MVC</b><br/>[Componente: Controller]<br/>Personas, Profesiones,<br/>Estudios, Telefonos, Home"]
        vistas["<b>Vistas Razor</b><br/>[Componente: .cshtml]<br/>Index, Create, Edit,<br/>Details, Delete"]
        api["<b>Controladores REST</b><br/>[Componente: ApiController]<br/>/api/personas, /api/profesiones,<br/>/api/estudios, /api/telefonos"]
        dto["<b>DTO y mapeador</b><br/>[Componente: clases C#]<br/>Contrato JSON de la API"]
        swagger["<b>Generador OpenAPI</b><br/>[Componente: Swashbuckle]<br/>swagger.json + Swagger UI"]
        idao["<b>Interfaces DAO</b><br/>[Componente: interfaces C#]<br/>IPersonaRepository, ..."]
        repo["<b>Repositorios DAO</b><br/>[Componente: EF Core]<br/>PersonaRepository, ..."]
        ctx["<b>PersonaDbContext + entidades</b><br/>[Componente: EF Core 10]<br/>Generado con Scaffold-DbContext"]
    end
    db[("<b>persona_db</b><br/>[SQL Server 2025]")]

    usuario -. "Páginas y formularios [HTTP]" .-> mvc
    cliente -. "Peticiones JSON [HTTP]" .-> api
    cliente -. "Lee la documentación" .-> swagger
    mvc -. "Entrega el modelo a" .-> vistas
    api -. "Convierte entidades con" .-> dto
    swagger -. "Describe" .-> api
    mvc -. "Usa (inyección de dependencias)" .-> idao
    api -. "Usa (inyección de dependencias)" .-> idao
    repo -. "Implementa" .-> idao
    repo -. "Consulta y guarda con LINQ" .-> ctx
    ctx -. "SQL [TDS :1433]" .-> db

    classDef persona fill:#08427B,stroke:#073B6F,color:#fff
    classDef externo fill:#8C8C8C,stroke:#6B6B6B,color:#fff
    classDef componente fill:#85BBF0,stroke:#5D82A8,color:#000
    classDef contenedor fill:#438DD5,stroke:#2E6295,color:#fff
    class usuario persona
    class cliente externo
    class mvc,vistas,api,dto,swagger,idao,repo,ctx componente
    class db contenedor
    style web fill:#ffffff,stroke:#666666,stroke-dasharray:6 4
```
</details>

---

## Nivel 4 · Código (Patrón DAO)

Diagrama de clases representativo de la implementación del patrón DAO para la entidad `Persona`. Las entidades `Profesion`, `Estudio` y `Telefono` siguen exactamente la misma estructura.

![C4 - Nivel 4: Código](diagramas/c4-nivel4-codigo.png)

<details>
<summary>Ver código fuente Mermaid</summary>

```mermaid
classDiagram
    direction LR
    class PersonasController {
        -IPersonaRepository _personas
        +Index() Task
        +Details(int id) Task
        +Create(Persona persona) Task
        +Edit(int id, Persona persona) Task
        +DeleteConfirmed(int id) Task
    }
    class PersonaApiController {
        -IPersonaRepository _personas
        +GetAll() Task
        +GetById(int cc) Task
        +Create(PersonaDto dto) Task
        +Update(int cc, PersonaDto dto) Task
        +Delete(int cc) Task
    }
    class IPersonaRepository {
        <<interface>>
        +GetAllAsync() Task
        +GetByIdAsync(int cc) Task
        +ExistsAsync(int cc) Task
        +CreateAsync(Persona persona) Task
        +UpdateAsync(Persona persona) Task
        +DeleteAsync(int cc) Task
    }
    class PersonaRepository {
        -PersonaDbContext _context
        +GetAllAsync() Task
        +GetByIdAsync(int cc) Task
        +ExistsAsync(int cc) Task
        +CreateAsync(Persona persona) Task
        +UpdateAsync(Persona persona) Task
        +DeleteAsync(int cc) Task
    }
    class PersonaDbContext {
        +DbSet Personas
        +DbSet Profesions
        +DbSet Estudios
        +DbSet Telefonos
    }
    class Persona {
        +int Cc
        +string Nombre
        +string Apellido
        +string Genero
        +int Edad
    }
    class PersonaDto {
        +int Cc
        +string Nombre
        +string Apellido
        +string Genero
        +int Edad
    }
    PersonasController --> IPersonaRepository : usa
    PersonaApiController --> IPersonaRepository : usa
    PersonaApiController ..> PersonaDto : convierte a
    PersonaRepository ..|> IPersonaRepository : implementa
    PersonaRepository --> PersonaDbContext : consulta
    PersonaDbContext o-- Persona : contiene
```
</details>

---

## Modelo Entidad - Relación (Base de Datos)

Diagrama relacional correspondiente al esquema de base de datos implementado en `db/DDL.sql` (adaptado desde el modelo original `persona_ddl_mysql.sql`).

![Diagrama Entidad-Relación](diagramas/diagrama-er.png)

<details>
<summary>Ver código fuente Mermaid</summary>

```mermaid
erDiagram
    PROFESION ||--o{ ESTUDIOS : "es estudiada en"
    PERSONA ||--o{ ESTUDIOS : "realiza"
    PERSONA ||--o{ TELEFONO : "es dueña de"
    PROFESION {
        int id PK
        varchar nom "90"
        varchar des "MAX"
    }
    PERSONA {
        int cc PK
        varchar nombre "45"
        varchar apellido "45"
        char genero "M o F"
        int edad
    }
    ESTUDIOS {
        int id_prof PK, FK
        int cc_per PK, FK
        date fecha
        varchar univer "50"
    }
    TELEFONO {
        varchar num PK "15"
        varchar oper "45"
        int duenio FK
    }
```
</details>

---

## Despliegue en Docker Compose

Orquestación de los contenedores dentro de la red aislada de Compose, mapeando puertos de exposición hacia el host y administrando el volumen de almacenamiento persistente para SQL Server.

![Diagrama de Despliegue](diagramas/c4-despliegue.png)

<details>
<summary>Ver código fuente Mermaid</summary>

```mermaid
flowchart TB
    usuario["<b>Usuario / cliente REST</b><br/>[Persona]"]
    subgraph host ["Equipo anfitrión [Docker Desktop]"]
        subgraph red ["Red interna de Compose"]
            subgraph c1 ["personapi-app [aspnet:10.0]"]
                web["<b>Aplicación web</b><br/>[ASP.NET Core 10] · puerto 8080"]
            end
            subgraph c2 ["personapi-db-init [mssql/server:2025]"]
                init["<b>Inicializador de BD</b><br/>[sqlcmd] · corre una vez"]
            end
            subgraph c3 ["personapi-sqlserver [mssql/server:2025-latest]"]
                db[("<b>persona_db</b><br/>[SQL Server 2025 Express] · puerto 1433")]
            end
        end
        vol[("volumen sqlserver-data")]
    end
    usuario -. "HTTP localhost:8080" .-> web
    web -. "TDS sqlserver:1433" .-> db
    init -. "DDL + DML" .-> db
    db -. "/var/opt/mssql" .-> vol

    classDef persona fill:#08427B,stroke:#073B6F,color:#fff
    classDef contenedor fill:#438DD5,stroke:#2E6295,color:#fff
    class usuario persona
    class web,init,db contenedor
```
</details>

---

## Diagramas de Secuencia UML (Operaciones CRUD)

Modelado formal del comportamiento dinámico e interacción temporal entre componentes del sistema (Cliente HTTP, Controladores MVC y REST, Capa DAO de Repositorios, Entity Framework Core y Microsoft SQL Server 2025):

| Operación | Descripción del Flujo | Artefacto PNG |
|---|---|---|
| **CREATE** | Inserción con validación de DataAnnotations, verificación de unicidad de clave y respuesta HTTP 201 Created | [`secuencia-crud-create.png`](diagramas/secuencia-crud-create.png) |
| **READ** | Consulta optimizada con `AsNoTracking`, carga diligente de relaciones (`Include`/`ThenInclude`) y respuesta HTTP 200 OK | [`secuencia-crud-read.png`](diagramas/secuencia-crud-read.png) |
| **UPDATE** | Modificación con validación de consistencia de clave primaria, actualización atómica y respuesta HTTP 204 No Content | [`secuencia-crud-update.png`](diagramas/secuencia-crud-update.png) |
| **DELETE** | Eliminación de registro raíz con activación de borrado en cascada (`ON DELETE CASCADE`) en SQL Server | [`secuencia-crud-delete.png`](diagramas/secuencia-crud-delete.png) |

### 1. Creación (CREATE)
![Secuencia CRUD - CREATE](diagramas/secuencia-crud-create.png)

### 2. Lectura (READ)
![Secuencia CRUD - READ](diagramas/secuencia-crud-read.png)

### 3. Actualización (UPDATE)
![Secuencia CRUD - UPDATE](diagramas/secuencia-crud-update.png)

### 4. Eliminación (DELETE)
![Secuencia CRUD - DELETE](diagramas/secuencia-crud-delete.png)

