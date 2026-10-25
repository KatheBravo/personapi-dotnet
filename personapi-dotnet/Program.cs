using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;
using personapi_dotnet.Models.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. MVC (controladores + vistas Razor) y controladores REST
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    // Las propiedades de navegación (no anulables) no deben volverse "obligatorias"
    // en los formularios: solo se envían las llaves foráneas.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// ---------------------------------------------------------------------------
// 2. Entity Framework Core + SQL Server
//    La cadena se toma de appsettings.json (ConnectionStrings:PersonaDb) y puede
//    sobreescribirse con la variable de entorno ConnectionStrings__PersonaDb (Docker).
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("PersonaDb")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'PersonaDb'.");

builder.Services.AddDbContext<PersonaDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(
        maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

// ---------------------------------------------------------------------------
// 3. Inyección de dependencias de la capa DAO (interfaz -> repositorio)
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IProfesionRepository, ProfesionRepository>();
builder.Services.AddScoped<IEstudioRepository, EstudioRepository>();
builder.Services.AddScoped<ITelefonoRepository, TelefonoRepository>();

// ---------------------------------------------------------------------------
// 4. Swagger / OpenAPI 3
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "personapi-dotnet",
        Version = "v1",
        Description = "API REST para el CRUD de persona, profesion, estudios y telefono " +
                      "(Laboratorio 1 - Monolito MVC + DAO, .NET 7 y SQL Server 2022)."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline HTTP
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Swagger habilitado en todos los ambientes para poder probar la API desplegada.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "personapi-dotnet v1");
    c.DocumentTitle = "personapi-dotnet - Swagger";
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Rutas por atributo de los controladores REST (/api/...)
app.MapControllers();

// Rutas convencionales de los controladores MVC con vistas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
