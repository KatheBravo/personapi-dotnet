using System.ComponentModel.DataAnnotations;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.DTOs;

/*
 * DTO (Data Transfer Object): es la forma exacta que tienen los datos cuando viajan
 * como JSON por la API REST.
 *
 * ¿Por qué no devolver directamente las entidades? Porque las entidades tienen
 * propiedades de navegación (Persona -> Telefonos -> Persona -> ...) que al convertirse
 * a JSON forman un ciclo infinito, y además mostrarían más de lo necesario.
 * Con los DTO el "contrato" de la API queda limpio, estable y separado de la base de datos.
 *
 * Los atributos [Required], [StringLength], [Range]... los revisa [ApiController]
 * automáticamente: si el JSON no cumple, responde 400 Bad Request sin llegar al controlador.
 */

/// <summary>Persona tal como se expone en la API.</summary>
public class PersonaDto
{
    /// <summary>Cédula de ciudadanía (llave primaria).</summary>
    [Required, Range(1, int.MaxValue)]
    public int Cc { get; set; }

    [Required, StringLength(45)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(45)]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>'M' o 'F'.</summary>
    [Required, RegularExpression("^[MF]$")]
    public string Genero { get; set; } = string.Empty;

    [Range(0, 150)]
    public int? Edad { get; set; }
}

/// <summary>Profesión tal como se expone en la API.</summary>
public class ProfesionDto
{
    [Required, Range(1, 999999)]
    public int Id { get; set; }

    [Required, StringLength(90)]
    public string Nom { get; set; } = string.Empty;

    public string? Des { get; set; }
}

/// <summary>Estudio (persona + profesión) tal como se expone en la API.</summary>
public class EstudioDto
{
    [Required]
    public int IdProf { get; set; }

    [Required]
    public int CcPer { get; set; }

    /// <summary>Fecha de grado (yyyy-MM-dd).</summary>
    public DateOnly? Fecha { get; set; }

    [StringLength(50)]
    public string? Univer { get; set; }
}

/// <summary>Teléfono tal como se expone en la API.</summary>
public class TelefonoDto
{
    [Required, StringLength(15), RegularExpression(@"^\+?[0-9]{7,14}$")]
    public string Num { get; set; } = string.Empty;

    [Required, StringLength(45)]
    public string Oper { get; set; } = string.Empty;

    /// <summary>Cédula de la persona dueña del teléfono.</summary>
    [Required]
    public int Duenio { get; set; }
}

/// <summary>
/// Conversión entre entidades y DTO. Son métodos de extensión, por eso se pueden
/// escribir como persona.ToDto() o dto.ToEntity(). Se hace a mano porque son pocos
/// campos y así no dependemos de librerías extra como AutoMapper.
/// </summary>
public static class DtoMapper
{
    public static PersonaDto ToDto(this Persona p) => new()
    {
        Cc = p.Cc, Nombre = p.Nombre, Apellido = p.Apellido, Genero = p.Genero, Edad = p.Edad
    };

    // Trim() quita espacios sobrantes al inicio y al final antes de guardar.
    public static Persona ToEntity(this PersonaDto d) => new()
    {
        Cc = d.Cc, Nombre = d.Nombre.Trim(), Apellido = d.Apellido.Trim(), Genero = d.Genero, Edad = d.Edad
    };

    public static ProfesionDto ToDto(this Profesion p) => new()
    {
        Id = p.Id, Nom = p.Nom, Des = p.Des
    };

    public static Profesion ToEntity(this ProfesionDto d) => new()
    {
        Id = d.Id, Nom = d.Nom.Trim(), Des = d.Des
    };

    public static EstudioDto ToDto(this Estudio e) => new()
    {
        IdProf = e.IdProf, CcPer = e.CcPer, Fecha = e.Fecha, Univer = e.Univer
    };

    public static Estudio ToEntity(this EstudioDto d) => new()
    {
        IdProf = d.IdProf, CcPer = d.CcPer, Fecha = d.Fecha, Univer = d.Univer
    };

    public static TelefonoDto ToDto(this Telefono t) => new()
    {
        Num = t.Num, Oper = t.Oper, Duenio = t.Duenio
    };

    public static Telefono ToEntity(this TelefonoDto d) => new()
    {
        Num = d.Num.Trim(), Oper = d.Oper.Trim(), Duenio = d.Duenio
    };
}
