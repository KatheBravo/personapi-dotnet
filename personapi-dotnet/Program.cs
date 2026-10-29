using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;
using personapi_dotnet.Models.Repositories;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Controladores MVC (vistas Razor) y controladores API REST
builder.Services.AddControllersWithViews(options =>
{
    // Solo valida atributos declarados explícitamente en los modelos
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Configuración de EF Core con SQL Server y estrategia de reintentos
var connectionString = builder.Configuration.GetConnectionString("PersonaDb")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'PersonaDb'.");

builder.Services.AddDbContext<PersonaDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null)));

// Inyección de dependencias para la capa DAO
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IProfesionRepository, ProfesionRepository>();
builder.Services.AddScoped<IEstudioRepository, EstudioRepository>();
builder.Services.AddScoped<ITelefonoRepository, TelefonoRepository>();

// Documentación OpenAPI 3 y Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "personapi-dotnet API",
        Version = "v1",
        Description = "API RESTful para la gestión del dominio de Personas, Profesiones, Estudios y Teléfonos (Laboratorio 1 - Monolito MVC + DAO en .NET 10 LTS con SQL Server 2025)."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Swagger UI disponible en todos los ambientes para pruebas y sustentación
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "personapi-dotnet v1");
    c.DocumentTitle = "personapi-dotnet | Documentación OpenAPI";
    c.DisplayRequestDuration();
    c.DocExpansion(DocExpansion.List);
    c.EnableFilter();
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Enrutamiento de API y vistas MVC
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
