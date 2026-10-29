using System;
using System.Collections.Generic;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Representa una fila de la tabla <c>estudios</c>: une a una persona con una profesión.
/// Es la tabla intermedia de la relación muchos a muchos, por eso su llave primaria
/// es compuesta: (IdProf, CcPer). Una persona no puede tener la misma profesión dos veces.
/// </summary>
public partial class Estudio
{
    /// <summary>Profesión estudiada (llave foránea a profesion.id).</summary>
    public int IdProf { get; set; }

    /// <summary>Persona que estudió (llave foránea a persona.cc).</summary>
    public int CcPer { get; set; }

    /// <summary>Fecha de grado. En la base es DATE, sin hora.</summary>
    public DateTime? Fecha { get; set; }

    /// <summary>Universidad donde estudió.</summary>
    public string? Univer { get; set; }

    // Navegación: permiten llegar desde el estudio a la persona y a la profesión completas.
    public virtual Persona CcPerNavigation { get; set; } = null!;

    public virtual Profesion IdProfNavigation { get; set; } = null!;
}
