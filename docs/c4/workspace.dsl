/*
 * Modelo C4 de personapi-dotnet en Structurizr DSL.
 * Para verlo: copiar este archivo en https://structurizr.com/dsl (o usar Structurizr Lite)
 * y se generan los diagramas de contexto, contenedores, componentes y despliegue.
 */
workspace "personapi-dotnet" "Laboratorio 1 - Monolito con patrón MVC y DAO" {

    model {
        usuario = person "Usuario administrativo" "Registra y consulta personas, profesiones, estudios y teléfonos desde el navegador."
        cliente = person "Desarrollador / sistema cliente" "Integra o prueba la API REST (Swagger UI, Postman u otro sistema)." "Externo"

        personapi = softwareSystem "personapi-dotnet" "Monolito que gestiona la información de personas con CRUD web y API REST." {

            web = container "Aplicación web" "Vistas Razor para el CRUD, API REST en /api y documentación OpenAPI 3 en /swagger." "ASP.NET Core 10 MVC, C#, EF Core 10" {
                mvc = component "Controladores MVC" "PersonasController, ProfesionesController, EstudiosController, TelefonosController, HomeController." "ASP.NET Core Controller"
                vistas = component "Vistas Razor" "Index, Create, Edit, Details y Delete por entidad, más el layout común." ".cshtml"
                api = component "Controladores REST" "PersonaApi, ProfesionApi, EstudioApi y TelefonoApi bajo /api." "ASP.NET Core ApiController"
                dto = component "DTO y mapeador" "Contrato JSON de la API, sin propiedades de navegación." "Clases C#"
                swagger = component "Generador OpenAPI" "Produce swagger.json (OpenAPI 3) y Swagger UI." "Swashbuckle 10"
                idao = component "Interfaces DAO" "IPersonaRepository, IProfesionRepository, IEstudioRepository, ITelefonoRepository." "Interfaces C#"
                repo = component "Repositorios DAO" "Implementan las operaciones CRUD de cada tabla." "Clases C# + EF Core"
                ctx = component "PersonaDbContext + entidades" "Generado con Scaffold-DbContext; mapea las cuatro tablas." "Entity Framework Core 10"
            }

            init = container "Inicializador de BD" "Ejecuta DDL.sql y DML.sql una sola vez al desplegar." "sqlcmd"
            db = container "persona_db" "Tablas persona, profesion, estudios y telefono." "Microsoft SQL Server 2025 Express" "Base de datos"
        }

        # Relaciones de nivel 1 y 2
        usuario -> mvc "Navega y envía formularios" "HTTP :8080, HTML"
        cliente -> api "Llama endpoints /api/*" "HTTP :8080, JSON"
        cliente -> swagger "Lee la documentación" "HTTP :8080"
        init -> db "Crea el esquema y los datos de prueba" "TDS :1433"

        # Relaciones de nivel 3
        mvc -> vistas "Entrega el modelo a"
        api -> dto "Convierte entidades con"
        swagger -> api "Describe"
        mvc -> idao "Usa (inyección de dependencias)"
        api -> idao "Usa (inyección de dependencias)"
        repo -> idao "Implementa"
        repo -> ctx "Consulta y guarda con LINQ"
        ctx -> db "Lee y escribe datos" "EF Core / ADO.NET, TDS :1433"

        produccion = deploymentEnvironment "Docker Compose" {
            deploymentNode "Equipo anfitrión" "Windows, Linux o macOS con Docker Desktop" "Docker" {
                deploymentNode "personapi-app" "" "Contenedor Docker, aspnet:10.0" {
                    containerInstance web
                }
                deploymentNode "personapi-db-init" "" "Contenedor Docker, mssql/server:2025" {
                    containerInstance init
                }
                deploymentNode "personapi-sqlserver" "" "Contenedor Docker, mssql/server:2025-latest" {
                    containerInstance db
                }
            }
        }
    }

    views {
        systemContext personapi "Contexto" {
            include *
            autolayout tb
        }
        container personapi "Contenedores" {
            include *
            autolayout tb
        }
        component web "Componentes" {
            include *
            autolayout tb
        }
        deployment personapi produccion "Despliegue" {
            include *
            autolayout lr
        }

        styles {
            element "Person" {
                shape Person
                background #08427B
                color #ffffff
            }
            element "Externo" {
                background #8C8C8C
            }
            element "Software System" {
                background #1168BD
                color #ffffff
            }
            element "Container" {
                background #438DD5
                color #ffffff
            }
            element "Base de datos" {
                shape Cylinder
            }
            element "Component" {
                background #85BBF0
                color #000000
            }
        }
    }
}
