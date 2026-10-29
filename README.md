# personapi-dotnet

> **Laboratorio 1 · Arquitectura de Software**  
> **Autora:** Katherine Sofía Bravo Herrera  
> **Implementación de Monolito con patrón MVC y DAO en ASP.NET Core 10 LTS y SQL Server 2025**

[![.NET 10](https://img.shields.io/badge/.NET-10.0%20LTS-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose%20v2-2496ED?logo=docker)](https://www.docker.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2025%20Express-CC292B?logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)
[![Swagger / OpenAPI](https://img.shields.io/badge/OpenAPI-3.0%20(Swagger)-85EA2D?logo=swagger)](http://localhost:8080/swagger)

Aplicación web monolítica que realiza el CRUD integral sobre cuatro tablas relacionadas — `persona`, `profesion`, `estudios` y `telefono` — mediante dos canales sincronizados:

- **Vistas Web (MVC):** Formularios Razor responsivos accesibles en `http://localhost:8080`.
- **API RESTful:** Endpoints JSON documentados interactivamente con **Swagger / OpenAPI 3** en `http://localhost:8080/swagger`.

---

## 1. Arquitectura y Modelo de Datos

La solución desacopla la presentación de la persistencia mediante el patrón **DAO (Data Access Object)** y la inyección de dependencias nativa de .NET.

### Diagramas de Arquitectura (Modelo C4) y Entidad-Relación

La documentación arquitectural completa con explicaciones detalladas se encuentra en [`docs/arquitectura-c4.md`](docs/arquitectura-c4.md). Cada diagrama está disponible en PNG de alta resolución:

| Diagrama | Descripción | Enlace |
|---|---|---|
| **Modelo ER** | Estructura relacional, llaves foráneas, cardinalidades y restricciones | [![Ver ER](docs/diagramas/diagrama-er.png)](docs/diagramas/diagrama-er.png) |
| **C4 - Nivel 1** | Contexto del sistema y actores involucrados | [![Ver Contexto](docs/diagramas/c4-nivel1-contexto.png)](docs/diagramas/c4-nivel1-contexto.png) |
| **C4 - Nivel 2** | Contenedores desplegables (Web, DB Init, SQL Server) | [![Ver Contenedores](docs/diagramas/c4-nivel2-contenedores.png)](docs/diagramas/c4-nivel2-contenedores.png) |
| **C4 - Nivel 3** | Componentes internos (Controllers, DTOs, DAO, DbContext) | [![Ver Componentes](docs/diagramas/c4-nivel3-componentes.png)](docs/diagramas/c4-nivel3-componentes.png) |
| **C4 - Nivel 4** | Diagrama de clases del patrón DAO (`Persona`) | [![Ver Código](docs/diagramas/c4-nivel4-codigo.png)](docs/diagramas/c4-nivel4-codigo.png) |
| **Despliegue** | Red interna y volúmenes en Docker Compose | [![Ver Despliegue](docs/diagramas/c4-despliegue.png)](docs/diagramas/c4-despliegue.png) |

---

## 2. Estructura del Repositorio

```text
personapi-dotnet/
├── docker-compose.yml          # Orquestación de SQL Server, db-init y app
├── .env.example                # Configuración de contraseñas y puertos (opcional)
├── personapi-dotnet.sln        # Solución principal
├── db/
│   ├── DDL.sql                 # Creación de persona_db, tablas, checks y foreign keys
│   └── DML.sql                 # Semilla de datos de prueba iniciales
├── docs/
│   ├── arquitectura-c4.md      # Detalle técnico del modelo C4 y diagramas Mermaid
│   ├── informe-laboratorio.md  # Informe formal del laboratorio (Portada, Marco, etc.)
│   ├── c4/workspace.dsl        # Definición formal Structurizr DSL
│   └── diagramas/              # Imágenes PNG exportadas en alta resolución
├── pruebas/
│   └── prueba-humo.ps1         # Suite automatizada de 89 pruebas de API y vistas
└── personapi-dotnet/           # Monolito ASP.NET Core MVC + API REST
    ├── Dockerfile              # Construcción multi-stage en SDK 10 y ASP.NET 10
    ├── Program.cs              # DI, configuración de Swagger, EF Core y pipeline
    ├── appsettings.json        # Cadena de conexión base
    ├── Controllers/            # Controladores MVC (Vistas Razor)
    │   └── Api/                # Controladores Web API (JSON RESTful)
    ├── Models/
    │   ├── Entities/           # Entidades EF Core y PersonaDbContext
    │   ├── Interfaces/         # Contratos de repositorios DAO
    │   ├── Repositories/       # Implementaciones concretas de acceso a datos
    │   └── DTOs/               # Objetos de transferencia y mapeadores
    └── Views/                  # Vistas Razor estructuradas por entidad
```

---

## 3. Despliegue con Docker Compose

El despliegue está 100% estandarizado en contenedores Docker. No se requiere instalar localmente SDKs de .NET ni instancias de SQL Server.

### Requisitos Previos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows / macOS) o Docker Engine + Compose v2 (Linux) en ejecución.
- Puertos disponibles en la máquina anfitriona: `8080` (Web / Swagger) y `1433` (SQL Server).

### Ejecución en un solo paso

```bash
# 1. Clonar el repositorio y ubicarse en la raíz
cd personapi-dotnet

# 2. Construir y levantar todos los servicios
docker compose up -d --build

# 3. Monitorear la inicialización de la base de datos
docker compose logs -f db-init
```

Cuando `db-init` finalice con el mensaje `Base de datos persona_db lista.`, todos los servicios estarán operativos:

| Servicio | URL / Conexión | Descripción |
|---|---|---|
| **Aplicación Web (MVC)** | `http://localhost:8080` | Interfaz gráfica para navegación y CRUD |
| **Documentación Swagger UI** | `http://localhost:8080/swagger` | Consola interactiva OpenAPI 3 |
| **Especificación OpenAPI** | `http://localhost:8080/swagger/v1/swagger.json` | JSON Schema del contrato de la API |
| **SQL Server 2025 Express** | `localhost:1433` | Usuario: `sa` · Clave: `Lab1-Persona2026` |

### Comandos de Mantenimiento

```bash
# Detener los contenedores conservando los datos
docker compose down

# Detener los contenedores y BORRAR el volumen de la base de datos (reinicio limpio)
docker compose down -v

# Reconstruir únicamente la aplicación tras modificar código C#
docker compose up -d --build app
```

---

## 4. Endpoints de la API REST

Todos los endpoints producen y consumen `application/json` con respuestas tipadas y códigos HTTP estándar (`200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `404 Not Found`, `409 Conflict`):

| Módulo | Método | Ruta | Descripción |
|---|---|---|---|
| **Personas** | `GET` | `/api/personas` | Lista todas las personas |
| | `GET` | `/api/personas/{cc}` | Obtiene una persona por cédula |
| | `GET` | `/api/personas/{cc}/telefonos` | Teléfonos de una persona |
| | `GET` | `/api/personas/{cc}/estudios` | Estudios cursados por una persona |
| | `POST` | `/api/personas` | Registra una nueva persona |
| | `PUT` | `/api/personas/{cc}` | Actualiza datos de una persona |
| | `DELETE` | `/api/personas/{cc}` | Elimina persona y dependencias en cascada |
| **Profesiones** | `GET` | `/api/profesiones` | Lista todas las profesiones |
| | `GET` | `/api/profesiones/{id}` | Obtiene profesión por id |
| | `POST` | `/api/profesiones` | Crea una profesión |
| | `PUT` | `/api/profesiones/{id}` | Actualiza una profesión |
| | `DELETE` | `/api/profesiones/{id}` | Elimina profesión y estudios asociados |
| **Estudios** | `GET` | `/api/estudios` | Lista todos los estudios |
| | `GET` | `/api/estudios/{idProf}/{ccPer}` | Obtiene estudio por llave compuesta |
| | `POST` | `/api/estudios` | Registra un nuevo estudio |
| | `PUT` | `/api/estudios/{idProf}/{ccPer}` | Actualiza fecha o universidad |
| | `DELETE` | `/api/estudios/{idProf}/{ccPer}` | Elimina registro de estudio |
| **Teléfonos** | `GET` | `/api/telefonos` | Lista todos los teléfonos |
| | `GET` | `/api/telefonos/{num}` | Obtiene teléfono por número |
| | `POST` | `/api/telefonos` | Asocia un teléfono a un dueño |
| | `PUT` | `/api/telefonos/{num}` | Actualiza operador o dueño |
| | `DELETE` | `/api/telefonos/{num}` | Elimina un teléfono |

---

## 5. Pruebas Automáticas de Calidad

El script `pruebas/prueba-humo.ps1` ejecuta una batería de **89 verificaciones automáticas** cubriendo:
- Respuestas HTTP de la API REST (`200`, `201`, `204`, `400`, `404`, `409`).
- Navegación, renderizado y envío de formularios MVC (protección CSRF y redirecciones 302).
- Validaciones de llaves foráneas inexistentes y unicidad de llaves primarias.
- Eliminación en cascada (`ON DELETE CASCADE`).

```powershell
powershell -ExecutionPolicy Bypass -File pruebas\prueba-humo.ps1
```

Resultado esperado:
```text
==========================================================
RESULTADO: 89 pruebas OK, 0 fallidas.
==========================================================
```

---

## 6. Documento Formal del Laboratorio y Wiki

- El informe académico formal requerido por el enunciado (Portada, Marco Conceptual, Diseño, Procedimiento, Conclusiones y Referencias) se encuentra en [`docs/informe-laboratorio.md`](docs/informe-laboratorio.md).
- Toda la documentación está publicada y sincronizada en la Wiki oficial del proyecto: [Wiki personapi-dotnet](https://github.com/KatheBravo/personapi-dotnet/wiki).

---

## 7. Entrega y TAG

- **TAG oficial:** `v1.0`
- **Release:** [Release v1.0](https://github.com/KatheBravo/personapi-dotnet/releases/tag/v1.0)
- **URL del TAG:** `https://github.com/KatheBravo/personapi-dotnet/tree/v1.0`
