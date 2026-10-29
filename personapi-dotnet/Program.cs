// Program.cs es el punto de entrada de la aplicación.
// Aquí se hacen dos cosas, en este orden:
//   1. Registrar los "servicios" que la app va a necesitar (MVC, base de datos, repositorios, Swagger).
//   2. Armar el "pipeline", es decir, el camino que recorre cada petición HTTP hasta llegar a un controlador.

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;   // en Swashbuckle 10 (Microsoft.OpenApi 2.x) los modelos están en este espacio de nombres
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;
using personapi_dotnet.Models.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. MVC: controladores con vistas Razor (las páginas web) y controladores de API.
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    // Por defecto ASP.NET Core vuelve obligatoria cualquier propiedad que no admita null.
    // Eso haría fallar los formularios, porque propiedades como Persona.Telefonos no vienen
    // en el formulario (solo se envían las llaves). Con esto solo valida lo que marcamos a mano.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// ---------------------------------------------------------------------------
// 2. Entity Framework Core con SQL Server.
//    La cadena de conexión se lee de appsettings.json (ConnectionStrings:PersonaDb).
//    En Docker la reemplazamos con la variable de entorno ConnectionStrings__PersonaDb,
//    así el mismo código sirve en el equipo local y en el contenedor sin tocar nada.
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("PersonaDb")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'PersonaDb'.");

builder.Services.AddDbContext<PersonaDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(
        // Si SQL Server todavía está arrancando, reintenta en vez de fallar a la primera.
        maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

// ---------------------------------------------------------------------------
// 3. Capa DAO: le decimos al contenedor de dependencias qué clase entregar
//    cada vez que alguien pida una interfaz. Los controladores solo conocen la
//    interfaz (IPersonaRepository) y nunca crean el repositorio con "new".
//    AddScoped = una instancia por petición HTTP, igual que el DbContext.
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IProfesionRepository, ProfesionRepository>();
builder.Services.AddScoped<IEstudioRepository, EstudioRepository>();
builder.Services.AddScoped<ITelefonoRepository, TelefonoRepository>();

// ---------------------------------------------------------------------------
// 4. Swagger: genera la documentación OpenAPI 3 de la API REST y la página para probarla.
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "personapi-dotnet",
        Version = "v1",
        Description = "API REST para el CRUD de persona, profesion, estudios y telefono " +
                      "(Laboratorio 1 - Monolito MVC + DAO, .NET 10 y SQL Server 2025)."
    });

    // Los comentarios /// de los controladores se compilan a un XML (ver el .csproj).
    // Al cargarlo aquí, esas descripciones aparecen en la página de Swagger.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline HTTP: cada petición pasa por estos pasos en orden.
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    // En producción no mostramos el detalle técnico del error, sino una página amable.
    app.UseExceptionHandler("/Home/Error");
}

// Swagger queda activo en todos los ambientes para poder probar la API ya desplegada.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "personapi-dotnet v1");
    c.DocumentTitle = "personapi-dotnet - Swagger";
});

app.UseStaticFiles();   // sirve lo que está en wwwroot (CSS)
app.UseRouting();       // decide qué controlador atiende cada URL
app.UseAuthorization(); // el laboratorio pide la app sin autenticación, pero se deja el paso estándar

// Los controladores REST definen su ruta con atributos, por ejemplo [Route("api/personas")].
app.MapControllers();

// Los controladores de vistas usan la ruta convencional: /Controlador/Accion/id
// Si no se indica nada, se abre Home/Index (la página de inicio).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
