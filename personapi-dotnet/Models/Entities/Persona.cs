using System;
using System.Collections.Generic;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Representa una fila de la tabla <c>persona</c>.
/// Esta clase la generó Scaffold-DbContext leyendo la base de datos: cada propiedad
/// corresponde a una columna. Las validaciones de formulario no van aquí sino en
/// Metadata/EntityMetadata.cs, para no perderlas si se vuelve a generar el modelo.
/// </summary>
public partial class Persona
{
    /// <summary>Cédula de ciudadanía. Es la llave primaria (columna cc).</summary>
    public int Cc { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    /// <summary>"M" o "F". En la base es CHAR(1) con una restricción CHECK.</summary>
    public string Genero { get; set; } = null!;

    /// <summary>Opcional, por eso es int? (puede quedar vacío).</summary>
    public int? Edad { get; set; }

    // Propiedades de navegación: no son columnas, son las relaciones.
    // Una persona puede tener varios estudios y varios teléfonos.
    public virtual ICollection<Estudio> Estudios { get; set; } = new List<Estudio>();

    public virtual ICollection<Telefono> Telefonos { get; set; } = new List<Telefono>();
}
