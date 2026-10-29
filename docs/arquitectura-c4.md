# Arquitectura de personapi-dotnet (modelo C4)

El modelo C4 describe un sistema de software en cuatro niveles de zoom. Cada nivel responde una pregunta distinta:

| Nivel | Diagrama | Pregunta que responde |
|---|---|---|
| 1 | Contexto | ¿Quién usa el sistema y con qué se relaciona? |
| 2 | Contenedores | ¿En qué piezas desplegables se divide (aplicación, base de datos...)? |
| 3 | Componentes | ¿Cómo está organizado por dentro el contenedor principal? |
| 4 | Código | ¿Cómo se ven las clases de un componente concreto? |

Además se incluye el diagrama de despliegue, que muestra dónde corre cada contenedor.

Convenciones de color (las de C4): azul oscuro = persona, azul = sistema, azul medio = contenedor, azul claro = componente, gris = actor externo. Las líneas punteadas son relaciones y su texto dice qué se hace y con qué tecnología.

Los diagramas están escritos como código (Mermaid), así GitHub los dibuja directamente y quedan versionados junto al proyecto. La definición formal del modelo está en [`c4/workspace.dsl`](c4/workspace.dsl) (Structurizr DSL), que se puede abrir en <https://structurizr.com/dsl> para ver los mismos diagramas.

---

## Nivel 1 · Contexto

personapi-dotnet es un solo sistema que usan dos tipos de actores: una persona desde el navegador y cualquier programa que consuma la API REST.

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

## Nivel 2 · Contenedores

Por dentro, el sistema tiene tres contenedores: la aplicación web (el monolito), la base de datos y un contenedor que solo se ejecuta al desplegar para crear las tablas.

```mermaid
flowchart TB
    usuario["<b>Usuario administrativo</b><br/>[Persona]"]
    cliente["<b>Desarrollador / sistema cliente</b><br/>[Persona externa]"]

    subgraph sis ["personapi-dotnet [Sistema de software]"]
        web["<b>Aplicación web</b><br/>[Contenedor: ASP.NET Core 7 MVC, C#, EF Core 7]<br/>Vistas Razor para el CRUD, API REST en /api<br/>y documentación OpenAPI 3 en /swagger"]
        init["<b>Inicializador de BD</b><br/>[Contenedor: sqlcmd]<br/>Ejecuta DDL.sql y DML.sql<br/>una sola vez al desplegar"]
        db[("<b>persona_db</b><br/>[Contenedor: SQL Server 2022 Express]<br/>Tablas persona, profesion,<br/>estudios y telefono")]
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

## Nivel 3 · Componentes de la aplicación web

Aquí se ven los patrones del laboratorio: **MVC** (controladores + vistas + modelo) y **DAO** (interfaces + repositorios). Los controladores de vistas y los de la API comparten la misma capa DAO, por eso no se repite lógica de acceso a datos.

```mermaid
flowchart TB
    usuario["<b>Usuario administrativo</b><br/>[Persona]"]
    cliente["<b>Cliente REST</b><br/>[Persona externa]"]

    subgraph web ["Aplicación web [Contenedor: ASP.NET Core 7]"]
        mvc["<b>Controladores MVC</b><br/>[Componente: Controller]<br/>Personas, Profesiones,<br/>Estudios, Telefonos, Home"]
        vistas["<b>Vistas Razor</b><br/>[Componente: .cshtml]<br/>Index, Create, Edit,<br/>Details, Delete"]
        api["<b>Controladores REST</b><br/>[Componente: ApiController]<br/>/api/personas, /api/profesiones,<br/>/api/estudios, /api/telefonos"]
        dto["<b>DTO y mapeador</b><br/>[Componente: clases C#]<br/>Contrato JSON de la API"]
        swagger["<b>Generador OpenAPI</b><br/>[Componente: Swashbuckle]<br/>swagger.json + Swagger UI"]
        idao["<b>Interfaces DAO</b><br/>[Componente: interfaces C#]<br/>IPersonaRepository, ..."]
        repo["<b>Repositorios DAO</b><br/>[Componente: EF Core]<br/>PersonaRepository, ..."]
        ctx["<b>PersonaDbContext + entidades</b><br/>[Componente: EF Core 7]<br/>Generado con Scaffold-DbContext"]
    end
    db[("<b>persona_db</b><br/>[SQL Server 2022]")]

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

## Nivel 4 · Código (patrón DAO aplicado a Persona)

Las otras tres entidades siguen exactamente la misma forma. En `estudios` los métodos reciben las dos partes de la llave compuesta.

```mermaid
classDiagram
    direction LR
    class PersonasController {
        -IPersonaRepository _personas
        +Index() 
        +Details(id)
        +Create(persona)
        +Edit(id, persona)
        +DeleteConfirmed(id)
    }
    class PersonaApiController {
        -IPersonaRepository _personas
        +GetAll()
        +GetById(cc)
        +Create(dto)
        +Update(cc, dto)
        +Delete(cc)
    }
    class IPersonaRepository {
        <<interface>>
        +GetAllAsync() Task~IEnumerable~Persona~~
        +GetByIdAsync(cc) Task~Persona~
        +ExistsAsync(cc) Task~bool~
        +CreateAsync(persona) Task~Persona~
        +UpdateAsync(persona) Task~bool~
        +DeleteAsync(cc) Task~bool~
    }
    class PersonaRepository {
        -PersonaDbContext _context
    }
    class PersonaDbContext {
        +DbSet~Persona~ Personas
        +DbSet~Profesion~ Profesions
        +DbSet~Estudio~ Estudios
        +DbSet~Telefono~ Telefonos
    }
    class Persona {
        +int Cc
        +string Nombre
        +string Apellido
        +string Genero
        +int? Edad
    }
    class PersonaDto {
        +int Cc
        +string Nombre
        +string Apellido
        +string Genero
        +int? Edad
    }
    PersonasController --> IPersonaRepository : usa
    PersonaApiController --> IPersonaRepository : usa
    PersonaApiController ..> PersonaDto : convierte a
    PersonaRepository ..|> IPersonaRepository : implementa
    PersonaRepository --> PersonaDbContext
    PersonaDbContext o-- Persona
```

### Modelo de datos

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

## Despliegue

Con Docker Compose los tres contenedores corren en una red interna. Solo se publican al equipo los puertos 8080 (aplicación) y 1433 (SQL Server, para revisarlo con SSMS).

```mermaid
flowchart TB
    usuario["<b>Usuario / cliente REST</b><br/>[Persona]"]
    subgraph host ["Equipo anfitrión [Docker Desktop]"]
        subgraph red ["Red interna de Compose"]
            subgraph c1 ["personapi-app [aspnet:7.0]"]
                web["<b>Aplicación web</b><br/>[ASP.NET Core 7] · puerto 8080"]
            end
            subgraph c2 ["personapi-db-init [mssql/server:2022]"]
                init["<b>Inicializador de BD</b><br/>[sqlcmd] · corre una vez"]
            end
            subgraph c3 ["personapi-sqlserver [mssql/server:2022-latest]"]
                db[("<b>persona_db</b><br/>[SQL Server 2022 Express] · puerto 1433")]
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
