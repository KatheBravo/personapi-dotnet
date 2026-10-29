# personapi-dotnet

> Laboratorio 1 · Grupo 3 · Katherine Sofía Bravo Herrera

Laboratorio 1 — Implementación de un **monolito** con patrón **MVC** y **DAO** en ASP.NET Core 7.

Aplicación web que hace el CRUD (crear, leer, actualizar y eliminar) de cuatro tablas — `persona`, `profesion`, `estudios` y `telefono` — de dos formas:

- **Vistas web (MVC):** formularios Razor en `http://localhost:8080`.
- **API REST:** endpoints JSON bajo `/api/...`, documentados con **Swagger / OpenAPI 3** en `/swagger`.

| Componente | Tecnología |
|---|---|
| Lenguaje / framework | C# · .NET 7 · ASP.NET Core MVC |
| Acceso a datos | Entity Framework Core 7 (database-first con `Scaffold-DbContext`) |
| Base de datos | Microsoft SQL Server 2022 (Express) |
| Documentación API | Swashbuckle.AspNetCore 6.5 (OpenAPI 3.0) |
| Contenedores | Docker + Docker Compose |

---

## 1. Modelo de datos

```
profesion (id PK, nom, des)
    1 ──< estudios (id_prof PK/FK, cc_per PK/FK, fecha, univer) >── 1
                                                                  persona (cc PK, nombre, apellido, genero, edad)
                                                                      1 ──< telefono (num PK, oper, duenio FK)
```

- Una **persona** puede tener muchos **teléfonos** (`telefono.duenio → persona.cc`).
- **estudios** es la tabla intermedia de la relación muchos a muchos entre **persona** y **profesion**; su llave primaria es compuesta (`id_prof`, `cc_per`).
- Al eliminar una persona o una profesión se eliminan en cascada sus estudios/teléfonos.

Equivalencias del modelo original (MySQL Workbench) a SQL Server:

| Modelo original | SQL Server | Motivo |
|---|---|---|
| `INT(6)`, `INT(15)` | `INT` | SQL Server no usa ancho de visualización en `INT` |
| `TEXT` | `VARCHAR(MAX)` | `TEXT` está obsoleto en SQL Server |
| `ENUM('M','F')` | `CHAR(1)` + `CHECK (genero IN ('M','F'))` | SQL Server no tiene `ENUM` |

---

## 2. Estructura del repositorio

La arquitectura está documentada con el modelo C4 en [docs/arquitectura-c4.md](docs/arquitectura-c4.md). Todo el código tiene comentarios que explican qué hace cada parte y por qué.

```
personapi-dotnet/
├── personapi-dotnet.sln
├── docker-compose.yml          # SQL Server + inicialización de BD + app
├── .env.example                # contraseña y puertos (opcional)
├── db/
│   ├── DDL.sql                 # crea persona_db (dueño sa) y las tablas
│   └── DML.sql                 # datos de prueba
├── docs/
│   ├── arquitectura-c4.md    # diagramas C4 (contexto, contenedores, componentes, código, despliegue)
│   └── c4/workspace.dsl      # el mismo modelo C4 en Structurizr DSL
├── pruebas/
│   └── prueba-humo.ps1       # pruebas automáticas de la API y de las vistas
├── INICIAR.bat / DETENER.bat  # arranque y parada rápida en Windows
└── personapi-dotnet/           # proyecto ASP.NET Core MVC
    ├── Program.cs              # DI, EF Core, Swagger, rutas
    ├── appsettings.json        # cadena de conexión
    ├── Dockerfile
    ├── Controllers/            # controladores MVC (vistas)
    │   └── Api/                # controladores REST (JSON)
    ├── Models/
    │   ├── Entities/           # entidades + PersonaDbContext (Scaffold-DbContext)
    │   ├── Interfaces/         # contratos DAO (IPersonaRepository, ...)
    │   ├── Repositories/       # implementaciones DAO con EF Core
    │   └── DTOs/               # objetos que viajan por la API
    ├── Views/                  # vistas Razor (Index, Create, Edit, Details, Delete)
    └── wwwroot/                # CSS
```

---

## 3. Opción A — Despliegue con Docker (recomendado)

### Requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/macOS) o Docker Engine + Compose v2 (Linux).
- Puertos libres: `8080` (app) y `1433` (SQL Server).

> En equipos Apple Silicon (M1/M2/M3) la imagen de SQL Server corre emulada (`platform: linux/amd64`); active *“Use Rosetta for x86/amd64 emulation”* en Docker Desktop.

### Pasos

```bash
# 1. Ubicarse en la raíz del repositorio
cd personapi-dotnet

# 2. (Opcional) cambiar contraseña o puertos
cp .env.example .env

# 3. Construir y levantar todo
docker compose up -d --build

# 4. Ver el progreso (db-init debe terminar con "Base de datos persona_db lista.")
docker compose logs -f db-init
docker compose ps
```

Cuando los tres servicios estén arriba:

| Qué | URL |
|---|---|
| Aplicación web (MVC) | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| Especificación OpenAPI 3 | http://localhost:8080/swagger/v1/swagger.json |
| SQL Server (desde SSMS) | `localhost,1433` · usuario `sa` · contraseña `Lab1-Persona2026` |

Comandos útiles:

```bash
docker compose down            # detiene los contenedores (conserva los datos)
docker compose down -v         # detiene y BORRA el volumen de la base de datos
docker compose up -d --build app   # recompila solo la aplicación tras cambiar código
```

---

## 4. Opción B — Ambiente local (Windows + Visual Studio)

### 4.1 Software

1. **SQL Server 2019/2022 Express** — instalación *Básica*. Crea la instancia `localhost\SQLEXPRESS`.
2. **SQL Server Management Studio (SSMS)** 18 o superior.
3. **Visual Studio Community 2022** con las cargas de trabajo:
   - Desarrollo de ASP.NET y web
   - Almacenamiento y procesamiento de datos
   - Componentes individuales: *Plantillas de proyecto y elementos de .NET Framework* y *Características avanzadas de ASP.NET*
4. **SDK de .NET 7** (`dotnet --list-sdks` debe mostrar una versión `7.0.x`).

### 4.2 Base de datos

1. Abrir SSMS y conectarse a `localhost\SQLEXPRESS` (Autenticación de Windows).
2. Abrir y ejecutar `db/DDL.sql` (F5). Crea `persona_db`, la deja a nombre de `sa` y crea las cuatro tablas.
3. Abrir y ejecutar `db/DML.sql` para cargar los datos de prueba.

### 4.3 Cadena de conexión

`personapi-dotnet/appsettings.json`:

```json
"ConnectionStrings": {
  "PersonaDb": "Server=localhost\\SQLEXPRESS;Database=persona_db;Trusted_Connection=True;TrustServerCertificate=true"
}
```

Si usa autenticación SQL en lugar de Windows:

```
Server=localhost\SQLEXPRESS;Database=persona_db;User Id=sa;Password=<su clave>;TrustServerCertificate=true
```

### 4.4 Compilar y ejecutar

Desde Visual Studio: abrir `personapi-dotnet.sln` y pulsar **F5** (perfil `personapi-dotnet` o `Swagger`).

Desde la terminal:

```bash
cd personapi-dotnet
dotnet restore
dotnet build -c Release
dotnet run                 # http://localhost:5000  y  http://localhost:5000/swagger
```

Publicar para despliegue:

```bash
dotnet publish -c Release -o ./publish
dotnet ./publish/personapi-dotnet.dll
```

### 4.5 (Referencia) Cómo se generaron las entidades

En la *Consola del Administrador de paquetes* de Visual Studio, con los paquetes `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer` y `Microsoft.EntityFrameworkCore.Tools` instalados:

```powershell
Scaffold-DbContext "Server=localhost\SQLEXPRESS;Database=persona_db;Trusted_Connection=True;TrustServerCertificate=true" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models/Entities
```

Luego se retiró del `PersonaDbContext` el método `OnConfiguring` (que trae la cadena “quemada”) y la conexión se inyecta desde `Program.cs`. Las validaciones de formulario están en `Models/Entities/Metadata/EntityMetadata.cs` para que no se pierdan si se vuelve a ejecutar el scaffold (agregue `-Force` para sobreescribir).

---

## 5. Endpoints de la API REST

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/personas` | Lista personas |
| GET | `/api/personas/{cc}` | Obtiene una persona |
| GET | `/api/personas/{cc}/telefonos` | Teléfonos de una persona |
| GET | `/api/personas/{cc}/estudios` | Estudios de una persona |
| POST | `/api/personas` | Crea una persona |
| PUT | `/api/personas/{cc}` | Actualiza una persona |
| DELETE | `/api/personas/{cc}` | Elimina una persona (y sus teléfonos/estudios) |
| GET | `/api/profesiones` · `/api/profesiones/{id}` | Lista / obtiene profesiones |
| POST · PUT · DELETE | `/api/profesiones` · `/api/profesiones/{id}` | Crea / actualiza / elimina |
| GET | `/api/estudios` · `/api/estudios/{idProf}/{ccPer}` | Lista / obtiene estudios |
| POST · PUT · DELETE | `/api/estudios` · `/api/estudios/{idProf}/{ccPer}` | Crea / actualiza / elimina |
| GET | `/api/telefonos` · `/api/telefonos/{num}` | Lista / obtiene teléfonos |
| POST · PUT · DELETE | `/api/telefonos` · `/api/telefonos/{num}` | Crea / actualiza / elimina |

Códigos de respuesta: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request` (validación o referencia inexistente), `404 Not Found`, `409 Conflict` (llave duplicada).

Ejemplo con `curl`:

```bash
curl -X POST http://localhost:8080/api/personas \
     -H "Content-Type: application/json" \
     -d '{"cc":1060606060,"nombre":"Sofia","apellido":"Ramirez","genero":"F","edad":30}'

curl http://localhost:8080/api/personas/1060606060
```

---

## 6. Control de versiones y entrega (TAG)

El repositorio remoto debe llamarse **`personapi-dotnet`** y ser **público**.

```bash
cd personapi-dotnet
git init -b main
git add .
git commit -m "Laboratorio 1: monolito MVC + DAO con .NET 7, SQL Server 2022, REST y Swagger"
git remote add origin https://github.com/KatheBravo/personapi-dotnet.git
git push -u origin main

# TAG de entrega
git tag -a v1.0.0 -m "Entrega Laboratorio 1"
git push origin v1.0.0
```

URL del TAG a entregar: `https://github.com/KatheBravo/personapi-dotnet/tree/v1.0.0`

---

## 7. Pruebas automáticas

`pruebas/prueba-humo.ps1` recorre el CRUD completo de las cuatro tablas por la API REST y por los formularios web (89 verificaciones: códigos HTTP, validaciones, llaves duplicadas, llaves foráneas inexistentes, borrado en cascada y Swagger). Usa sus propios datos de prueba y los borra al terminar.

Con la aplicación corriendo:

```powershell
powershell -ExecutionPolicy Bypass -File pruebas\prueba-humo.ps1                                   # Docker (puerto 8080)
powershell -ExecutionPolicy Bypass -File pruebas\prueba-humo.ps1 -BaseUrl http://localhost:5000    # dotnet run
```

Al final muestra `RESULTADO: N pruebas OK, 0 fallidas`.

---

## 8. Solución de problemas

| Síntoma | Causa probable / solución |
|---|---|
| La página de inicio dice *“No hay conexión con la base de datos”* | Revise la cadena `PersonaDb`, que el servicio SQL Server esté encendido y que exista `persona_db`. |
| `db-init` termina con error de login | La contraseña de `sa` no cumple la política (mín. 8 caracteres, mayúsculas, minúsculas, números y símbolos) o se cambió `.env` después de crear el volumen: ejecute `docker compose down -v` y vuelva a levantar. |
| *A network-related or instance-specific error* en local | Active el protocolo TCP/IP y el servicio *SQL Server Browser* en el *SQL Server Configuration Manager*. |
| *The certificate chain was issued by an authority that is not trusted* | Falta `TrustServerCertificate=true` en la cadena de conexión. |
| Puerto 1433 u 8080 ocupado | Cambie `SQL_PORT` o `APP_PORT` en `.env`. |
