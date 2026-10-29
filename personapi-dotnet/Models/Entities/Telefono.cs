using System;
using System.Collections.Generic;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Representa una fila de la tabla <c>telefono</c> (generada con Scaffold-DbContext).
/// Cada teléfono pertenece a una sola persona.
/// </summary>
public partial class Telefono
{
    /// <summary>Número de teléfono. Es la llave primaria, por eso es texto (puede llevar '+').</summary>
    public string Num { get; set; } = null!;

    /// <summary>Operador: Claro, Movistar, Tigo, etc.</summary>
    public string Oper { get; set; } = null!;

    /// <summary>Cédula de la persona dueña (llave foránea a persona.cc).</summary>
    public int Duenio { get; set; }

    // Navegación hacia la persona dueña del número.
    public virtual Persona DuenioNavigation { get; set; } = null!;
}
