using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

/*
 * Las clases generadas por Scaffold-DbContext se sobrescriben cada vez que se regenera
 * el modelo. Para no perder las validaciones y nombres para mostrar, se declaran aquí
 * como clases "partial" con [ModelMetadataType], que ASP.NET Core MVC combina con las
 * entidades al validar formularios y al generar las etiquetas en las vistas.
 */

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
    [RegularExpression("^[MF]$", ErrorMessage = "El género debe ser 'M' o 'F'.")]
    public string Genero { get; set; } = null!;

    [Display(Name = "Edad")]
    [Range(0, 150, ErrorMessage = "La edad debe estar entre 0 y 150.")]
    public int? Edad { get; set; }

    [ValidateNever] public object? Estudios { get; set; }
    [ValidateNever] public object? Telefonos { get; set; }
}

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

[ModelMetadataType(typeof(TelefonoMetadata))]
public partial class Telefono { }

public class TelefonoMetadata
{
    [Display(Name = "Número")]
    [Required(ErrorMessage = "El número es obligatorio.")]
    [StringLength(15, ErrorMessage = "El número admite máximo 15 caracteres.")]
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
