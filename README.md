# personapi-dotnet

> Laboratorio 1 · Arquitectura de Software  
> Integrantes (Grupo 3): Katherine Sofía Bravo Herrera · Daniel Avila Medina · Juan Sebastián Alvarez  
> Monolito con patrón MVC y DAO en ASP.NET Core 10 LTS y SQL Server 2025 Express

Aplicación web monolítica que implementa el ciclo completo de operaciones CRUD sobre las entidades `persona`, `profesion`, `estudios` y `telefono`, exponiendo dos canales de interacción desacoplados de la persistencia mediante el patrón **DAO (Data Access Object)**:

- **Interfaz Web (MVC):** Formularios Razor en `http://localhost:8080`.
- **API RESTful:** Endpoints JSON documentados en Swagger / OpenAPI 3 en `http://localhost:8080/swagger`.

---

## Stack Tecnológico

| Componente | Tecnología | Versión |
|---|---|---|
| Lenguaje | C# | 14 |
| Framework | .NET (LTS) / ASP.NET Core MVC & Web API | 10.0 |
| Acceso a Datos | Entity Framework Core (Database-First) | 10.0 |
| Base de Datos | Microsoft SQL Server (edición Express) | 2025 |
| Documentación API | OpenAPI 3.0 / Swashbuckle | 10.2 |
| Orquestación | Docker & Docker Compose | v2 |

---

## Estructura del Repositorio

```text
personapi-dotnet/
├── docker-compose.yml          # Orquestación de SQL Server, inicializador y aplicación
├── .env.example                # Variables de entorno (puertos y credenciales)
├── personapi-dotnet.sln        # Solución de .NET
├── db/
│   ├── DDL.sql                 # Definición de esquema persona_db, tablas, checks y FKs
│   └── DML.sql                 # Datos iniciales de prueba
├── docs/
│   ├── arquitectura-c4.md      # Documentación detallada de arquitectura (Modelo C4 y ER)
│   ├── informe-laboratorio.md  # Informe formal del laboratorio (Portada, Marco, etc.)
│   ├── c4/workspace.dsl        # Modelo formal en Structurizr DSL
│   └── diagramas/              # Diagramas exportados en formato PNG
│       ├── diagrama-er.png
│       ├── c4-nivel1-contexto.png
│       ├── c4-nivel2-contenedores.png
│       ├── c4-nivel3-componentes.png
│       ├── c4-nivel4-codigo.png
│       ├── c4-despliegue.png
│       ├── secuencia-crud-create.png
│       ├── secuencia-crud-read.png
│       ├── secuencia-crud-update.png
│       └── secuencia-crud-delete.png
├── pruebas/
│   └── prueba-humo.ps1         # Suite de 89 pruebas automáticas (API y vistas MVC)
└── personapi-dotnet/           # Código fuente del monolito
    ├── Dockerfile              # Construcción multi-stage (SDK 10 y ASP.NET 10)
    ├── Program.cs              # DI, Swagger UI, EF Core y middleware
    ├── Controllers/            # Controladores MVC (vistas Razor)
    │   └── Api/                # Controladores Web API (REST JSON)
    ├── Models/
    │   ├── Entities/           # Entidades EF Core y DbContext
    │   ├── Interfaces/         # Contratos de repositorios DAO
    │   ├── Repositories/       # Implementaciones DAO con EF Core
    │   └── DTOs/               # Contratos de datos de la API y mapeadores
    └── Views/                  # Vistas Razor por entidad y layout compartido
```

---

## Despliegue con Docker Compose

Todo el ambiente está contenerizado. No es necesario tener instalados SDKs locales de .NET ni instancias de SQL Server en la máquina anfitriona.

### 1. Iniciar los servicios

```bash
docker compose up -d --build
```

Compose levantará en orden coordinado:
1. `personapi-sqlserver`: Motor de base de datos con healthcheck activo.
2. `personapi-db-init`: Ejecuta `DDL.sql` y `DML.sql` una vez que SQL Server responde.
3. `personapi-app`: Compila y expone la aplicación web y la API en el puerto 8080.

### 2. Puntos de acceso

| Servicio | URL / Host | Credenciales / Detalle |
|---|---|---|
| Interfaz Web MVC | http://localhost:8080 | Navegación y formularios Razor |
| Swagger UI | http://localhost:8080/swagger | Consola interactiva OpenAPI 3 |
| Especificación OpenAPI | http://localhost:8080/swagger/v1/swagger.json | JSON Schema de la API |
| SQL Server | `localhost:1433` | Usuario: `sa` · Contraseña: `Lab1-Persona2026` |

### 3. Comandos útiles

```bash
# Ver estado de los contenedores
docker compose ps

# Ver registros del inicializador de BD
docker compose logs -f db-init

# Detener los servicios conservando datos
docker compose down

# Detener y reiniciar limpiamente (borra volumen y re-ejecuta DDL/DML)
docker compose down -v
docker compose up -d --build
```

---

## Catálogo de Endpoints (API REST)

Los endpoints responden y consumen `application/json` con códigos de estado HTTP estándar (`200`, `201`, `204`, `400`, `404`, `409`):

| Módulo | Método | Endpoint | Acción |
|---|---|---|---|
| **Personas** | `GET` | `/api/personas` | Lista general de personas |
| | `GET` | `/api/personas/{cc}` | Consulta persona por cédula |
| | `GET` | `/api/personas/{cc}/telefonos` | Teléfonos asociados a la persona |
| | `GET` | `/api/personas/{cc}/estudios` | Estudios asociados a la persona |
| | `POST` | `/api/personas` | Creación de persona |
| | `PUT` | `/api/personas/{cc}` | Actualización de persona |
| | `DELETE` | `/api/personas/{cc}` | Eliminación en cascada |
| **Profesiones** | `GET` | `/api/profesiones` | Lista de profesiones |
| | `GET` | `/api/profesiones/{id}` | Consulta profesión por id |
| | `POST` | `/api/profesiones` | Creación de profesión |
| | `PUT` | `/api/profesiones/{id}` | Actualización de profesión |
| | `DELETE` | `/api/profesiones/{id}` | Eliminación en cascada |
| **Estudios** | `GET` | `/api/estudios` | Lista general de estudios |
| | `GET` | `/api/estudios/{idProf}/{ccPer}` | Consulta por clave compuesta |
| | `POST` | `/api/estudios` | Registro de nuevo estudio |
| | `PUT` | `/api/estudios/{idProf}/{ccPer}` | Actualización de fecha/universidad |
| | `DELETE` | `/api/estudios/{idProf}/{ccPer}` | Eliminación de estudio |
| **Teléfonos** | `GET` | `/api/telefonos` | Lista general de teléfonos |
| | `GET` | `/api/telefonos/{num}` | Consulta teléfono por número |
| | `POST` | `/api/telefonos` | Registro y asociación a dueño |
| | `PUT` | `/api/telefonos/{num}` | Actualización de operador/dueño |
| | `DELETE` | `/api/telefonos/{num}` | Eliminación de teléfono |

---

## Pruebas Automáticas

La suite automatizada valida 89 aserciones sobre el CRUD de la API, restricciones de unicidad, llaves foráneas, borrado en cascada y flujos de vistas MVC:

```powershell
powershell -ExecutionPolicy Bypass -File pruebas\prueba-humo.ps1
```

Resultado:
```text
RESULTADO: 89 pruebas OK, 0 fallidas
```

---

## Documentación y Entregables

- **Diagramas de Arquitectura y Secuencia UML (PNG):** Organizados en la carpeta [`docs/diagramas/`](docs/diagramas/):
  - [Diagrama Entidad-Relación](docs/diagramas/diagrama-er.png)
  - [C4 Nivel 1 - Contexto](docs/diagramas/c4-nivel1-contexto.png)
  - [C4 Nivel 2 - Contenedores](docs/diagramas/c4-nivel2-contenedores.png)
  - [C4 Nivel 3 - Componentes](docs/diagramas/c4-nivel3-componentes.png)
  - [C4 Nivel 4 - Código (Patrón DAO)](docs/diagramas/c4-nivel4-codigo.png)
  - [C4 Despliegue en Docker](docs/diagramas/c4-despliegue.png)
  - [Secuencia CRUD - CREATE](docs/diagramas/secuencia-crud-create.png)
  - [Secuencia CRUD - READ](docs/diagramas/secuencia-crud-read.png)
  - [Secuencia CRUD - UPDATE](docs/diagramas/secuencia-crud-update.png)
  - [Secuencia CRUD - DELETE](docs/diagramas/secuencia-crud-delete.png)
- **Documento de Arquitectura C4:** [`docs/arquitectura-c4.md`](docs/arquitectura-c4.md)
- **Informe Académico del Laboratorio:** [`docs/informe-laboratorio.md`](docs/informe-laboratorio.md)
- **Wiki del Proyecto:** [https://github.com/KatheBravo/personapi-dotnet/wiki](https://github.com/KatheBravo/personapi-dotnet/wiki)
- **TAG de Entrega:** [`v1.2`](https://github.com/KatheBravo/personapi-dotnet/tree/v1.2)
- **Release:** [Release v1.2](https://github.com/KatheBravo/personapi-dotnet/releases/tag/v1.2)
