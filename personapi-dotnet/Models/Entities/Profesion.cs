using System;
using System.Collections.Generic;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Representa una fila de la tabla <c>profesion</c> (generada con Scaffold-DbContext).
/// </summary>
public partial class Profesion
{
    /// <summary>Identificador de la profesión. Llave primaria; lo escribe el usuario, no es autoincremental.</summary>
    public int Id { get; set; }

    /// <summary>Nombre de la profesión (columna nom).</summary>
    public string Nom { get; set; } = null!;

    /// <summary>Descripción opcional (columna des, VARCHAR(MAX)).</summary>
    public string? Des { get; set; }

    // Todas las personas que estudiaron esta profesión, a través de la tabla estudios.
    public virtual ICollection<Estudio> Estudios { get; set; } = new List<Estudio>();
}
