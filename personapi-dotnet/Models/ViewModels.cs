namespace personapi_dotnet.Models;

// Un "ViewModel" es una clase pensada solo para llevarle datos a una vista,
// cuando lo que la vista necesita no coincide con ninguna tabla.

/// <summary>Datos de la página de error (la plantilla estándar de ASP.NET Core).</summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

/// <summary>
/// Datos que muestra la página de inicio: cuántos registros hay en cada tabla
/// y si la conexión con la base de datos está funcionando.
/// </summary>
public class HomeViewModel
{
    public int Personas { get; set; }
    public int Profesiones { get; set; }
    public int Estudios { get; set; }
    public int Telefonos { get; set; }
    public bool BaseDeDatosDisponible { get; set; }
    public string? MensajeError { get; set; }
}
