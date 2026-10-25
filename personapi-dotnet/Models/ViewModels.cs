namespace personapi_dotnet.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

/// <summary>Datos que muestra la página de inicio.</summary>
public class HomeViewModel
{
    public int Personas { get; set; }
    public int Profesiones { get; set; }
    public int Estudios { get; set; }
    public int Telefonos { get; set; }
    public bool BaseDeDatosDisponible { get; set; }
    public string? MensajeError { get; set; }
}
