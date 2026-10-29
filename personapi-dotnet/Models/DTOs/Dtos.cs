using System.ComponentModel.DataAnnotations;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.DTOs;

/// <summary>Contrato de datos de Persona para la API REST.</summary>
public class PersonaDto
{
    /// <summary>Cédula de ciudadanía (llave primaria).</summary>
    [Required, Range(1, int.MaxValue)]
    public int Cc { get; set; }

    /// <summary>Nombres de la persona.</summary>
    [Required, StringLength(45)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Apellidos de la persona.</summary>
    [Required, StringLength(45)]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>Género biológico ('M' o 'F').</summary>
    [Required, RegularExpression("^[MF]$")]
    public string Genero { get; set; } = string.Empty;

    /// <summary>Edad en años (0 - 150).</summary>
    [Range(0, 150)]
    public int? Edad { get; set; }
}

/// <summary>Contrato de datos de Profesión para la API REST.</summary>
public class ProfesionDto
{
    /// <summary>Identificador numérico de la profesión.</summary>
    [Required, Range(1, 999999)]
    public int Id { get; set; }

    /// <summary>Nombre de la profesión.</summary>
    [Required, StringLength(90)]
    public string Nom { get; set; } = string.Empty;

    /// <summary>Descripción detallada de la profesión.</summary>
    public string? Des { get; set; }
}

/// <summary>Contrato de datos de Estudio (Persona - Profesión) para la API REST.</summary>
public class EstudioDto
{
    /// <summary>Identificador de la profesión asociada.</summary>
    [Required]
    public int IdProf { get; set; }

    /// <summary>Cédula de la persona asociada.</summary>
    [Required]
    public int CcPer { get; set; }

    /// <summary>Fecha de graduación (formato yyyy-MM-dd).</summary>
    public DateOnly? Fecha { get; set; }

    /// <summary>Universidad o institución educativa.</summary>
    [StringLength(50)]
    public string? Univer { get; set; }
}

/// <summary>Contrato de datos de Teléfono para la API REST.</summary>
public class TelefonoDto
{
    /// <summary>Número telefónico (entre 7 y 14 dígitos).</summary>
    [Required, StringLength(15), RegularExpression(@"^\+?[0-9]{7,14}$")]
    public string Num { get; set; } = string.Empty;

    /// <summary>Operador de telefonía.</summary>
    [Required, StringLength(45)]
    public string Oper { get; set; } = string.Empty;

    /// <summary>Cédula de la persona propietaria del teléfono.</summary>
    [Required]
    public int Duenio { get; set; }
}

/// <summary>Mapeador entre entidades de base de datos y contratos DTO.</summary>
public static class DtoMapper
{
    public static PersonaDto ToDto(this Persona p) => new()
    {
        Cc = p.Cc,
        Nombre = p.Nombre,
        Apellido = p.Apellido,
        Genero = p.Genero,
        Edad = p.Edad
    };

    public static Persona ToEntity(this PersonaDto d) => new()
    {
        Cc = d.Cc,
        Nombre = d.Nombre.Trim(),
        Apellido = d.Apellido.Trim(),
        Genero = d.Genero,
        Edad = d.Edad
    };

    public static ProfesionDto ToDto(this Profesion p) => new()
    {
        Id = p.Id,
        Nom = p.Nom,
        Des = p.Des
    };

    public static Profesion ToEntity(this ProfesionDto d) => new()
    {
        Id = d.Id,
        Nom = d.Nom.Trim(),
        Des = string.IsNullOrWhiteSpace(d.Des) ? null : d.Des.Trim()
    };

    public static EstudioDto ToDto(this Estudio e) => new()
    {
        IdProf = e.IdProf,
        CcPer = e.CcPer,
        Fecha = e.Fecha,
        Univer = e.Univer
    };

    public static Estudio ToEntity(this EstudioDto d) => new()
    {
        IdProf = d.IdProf,
        CcPer = d.CcPer,
        Fecha = d.Fecha,
        Univer = string.IsNullOrWhiteSpace(d.Univer) ? null : d.Univer.Trim()
    };

    public static TelefonoDto ToDto(this Telefono t) => new()
    {
        Num = t.Num,
        Oper = t.Oper,
        Duenio = t.Duenio
    };

    public static Telefono ToEntity(this TelefonoDto d) => new()
    {
        Num = d.Num.Trim(),
        Oper = d.Oper.Trim(),
        Duenio = d.Duenio
    };
}
