using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

/*
 * ¿Por qué existe este archivo?
 * Las entidades (Persona, Profesion, ...) las genera Scaffold-DbContext, y si mañana
 * se vuelve a ejecutar ese comando, sobrescribe esos archivos. Si pusiéramos las
 * validaciones allí, se perderían.
 *
 * La solución es usar "clases parciales": C# permite repartir una misma clase en varios
 * archivos. Aquí abrimos otra parte de cada entidad y le pegamos [ModelMetadataType],
 * que le dice a ASP.NET Core: "las reglas de validación y los nombres para mostrar de
 * esta clase están en esta otra clase de metadatos".
 *
 * Esas reglas se usan en dos lugares: al validar los formularios en el servidor y al
 * generar la validación en el navegador (jQuery Validation).
 */

// ---------------------------------------------------------------- Persona
[ModelMetadataType(typeof(PersonaMetadata))]
public partial class Persona { }

public class PersonaMetadata
{
    [Display(Name = "Cédula")]
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cédula debe ser un número positivo.")]
    public int Cc { get; set; }

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre admite máximo 45 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Apellido")]
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(45, ErrorMessage = "El apellido admite máximo 45 caracteres.")]
    public string Apellido { get; set; } = null!;

    [Display(Name = "Género")]
    [Required(ErrorMessage = "El género es obligatorio.")]
    // Misma regla que el CHECK de la base de datos: solo M o F.
    [RegularExpression("^[MF]$", ErrorMessage = "El género debe ser 'M' o 'F'.")]
    public string Genero { get; set; } = null!;

    [Display(Name = "Edad")]
    [Range(0, 150, ErrorMessage = "La edad debe estar entre 0 y 150.")]
    public int? Edad { get; set; }

    // Las colecciones de navegación no vienen en el formulario, así que no se validan.
    [ValidateNever] public object? Estudios { get; set; }
    [ValidateNever] public object? Telefonos { get; set; }
}

// ---------------------------------------------------------------- Profesion
[ModelMetadataType(typeof(ProfesionMetadata))]
public partial class Profesion { }

public class ProfesionMetadata
{
    [Display(Name = "Id")]
    [Required(ErrorMessage = "El id es obligatorio.")]
    [Range(1, 999999, ErrorMessage = "El id debe estar entre 1 y 999999.")]
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(90, ErrorMessage = "El nombre admite máximo 90 caracteres.")]
    public string Nom { get; set; } = null!;

    [Display(Name = "Descripción")]
    [DataType(DataType.MultilineText)]
    public string? Des { get; set; }

    [ValidateNever] public object? Estudios { get; set; }
}

// ---------------------------------------------------------------- Estudio
[ModelMetadataType(typeof(EstudioMetadata))]
public partial class Estudio { }

public class EstudioMetadata
{
    [Display(Name = "Profesión")]
    [Required(ErrorMessage = "La profesión es obligatoria.")]
    public int IdProf { get; set; }

    [Display(Name = "Persona")]
    [Required(ErrorMessage = "La persona es obligatoria.")]
    public int CcPer { get; set; }

    [Display(Name = "Fecha de grado")]
    // DataType.Date hace que el formulario muestre un selector de fecha en lugar de texto libre.
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Fecha { get; set; }

    [Display(Name = "Universidad")]
    [StringLength(50, ErrorMessage = "La universidad admite máximo 50 caracteres.")]
    public string? Univer { get; set; }

    [Display(Name = "Persona")]
    [ValidateNever] public object? CcPerNavigation { get; set; }

    [Display(Name = "Profesión")]
    [ValidateNever] public object? IdProfNavigation { get; set; }
}

// ---------------------------------------------------------------- Telefono
[ModelMetadataType(typeof(TelefonoMetadata))]
public partial class Telefono { }

public class TelefonoMetadata
{
    [Display(Name = "Número")]
    [Required(ErrorMessage = "El número es obligatorio.")]
    [StringLength(15, ErrorMessage = "El número admite máximo 15 caracteres.")]
    // Solo dígitos (de 7 a 14), con un '+' opcional al inicio para números internacionales.
    [RegularExpression(@"^\+?[0-9]{7,14}$", ErrorMessage = "Solo dígitos (7 a 14), opcionalmente con '+' al inicio.")]
    public string Num { get; set; } = null!;

    [Display(Name = "Operador")]
    [Required(ErrorMessage = "El operador es obligatorio.")]
    [StringLength(45, ErrorMessage = "El operador admite máximo 45 caracteres.")]
    public string Oper { get; set; } = null!;

    [Display(Name = "Dueño")]
    [Required(ErrorMessage = "El dueño es obligatorio.")]
    public int Duenio { get; set; }

    [Display(Name = "Dueño")]
    [ValidateNever] public object? DuenioNavigation { get; set; }
}
