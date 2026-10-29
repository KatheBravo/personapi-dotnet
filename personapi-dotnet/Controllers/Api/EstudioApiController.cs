using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>Endpoints REST del CRUD de estudios (relación Persona - Profesión).</summary>
[ApiController]
[Route("api/estudios")]
[Produces("application/json")]
[Tags("Estudios")]
public class EstudioApiController : ControllerBase
{
    private readonly IEstudioRepository _estudios;
    private readonly IPersonaRepository _personas;
    private readonly IProfesionRepository _profesiones;

    public EstudioApiController(IEstudioRepository estudios, IPersonaRepository personas, IProfesionRepository profesiones)
    {
        _estudios = estudios;
        _personas = personas;
        _profesiones = profesiones;
    }

    /// <summary>Lista todos los registros de estudios.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EstudioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EstudioDto>>> GetAll()
    {
        var estudios = await _estudios.GetAllAsync();
        return Ok(estudios.Select(e => e.ToDto()));
    }

    /// <summary>Obtiene un estudio por su llave compuesta (idProf / ccPer).</summary>
    /// <param name="idProf">Identificador de la profesión.</param>
    /// <param name="ccPer">Cédula de la persona.</param>
    [HttpGet("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(typeof(EstudioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudioDto>> GetById(int idProf, int ccPer)
    {
        var estudio = await _estudios.GetByIdAsync(idProf, ccPer);
        return estudio is null ? NotFound() : Ok(estudio.ToDto());
    }

    /// <summary>Registra un nuevo estudio para una persona y profesión existentes.</summary>
    /// <param name="dto">Datos del estudio a registrar.</param>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(EstudioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EstudioDto>> Create([FromBody] EstudioDto dto)
    {
        // Validamos existencia de las llaves foráneas y unicidad del registro compuesto
        if (!await _personas.ExistsAsync(dto.CcPer))
            return BadRequest(new { mensaje = $"No existe la persona con cédula {dto.CcPer}." });
        if (!await _profesiones.ExistsAsync(dto.IdProf))
            return BadRequest(new { mensaje = $"No existe la profesión con id {dto.IdProf}." });
        if (await _estudios.ExistsAsync(dto.IdProf, dto.CcPer))
            return Conflict(new { mensaje = "Esa persona ya tiene registrada esa profesión." });

        try
        {
            var creado = await _estudios.CreateAsync(dto.ToEntity());
            return CreatedAtAction(nameof(GetById),
                new { idProf = creado.IdProf, ccPer = creado.CcPer }, creado.ToDto());
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensaje = "No se pudo guardar el estudio.", detalle = ex.InnerException?.Message });
        }
    }

    /// <summary>Actualiza los datos complementarios de un estudio (fecha y universidad).</summary>
    /// <param name="idProf">Identificador de la profesión en URL.</param>
    /// <param name="ccPer">Cédula de la persona en URL.</param>
    /// <param name="dto">Datos actualizados.</param>
    [HttpPut("{idProf:int}/{ccPer:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int idProf, int ccPer, [FromBody] EstudioDto dto)
    {
        if (idProf != dto.IdProf || ccPer != dto.CcPer)
            return BadRequest(new { mensaje = "La llave de la URL y la del cuerpo deben coincidir." });

        return await _estudios.UpdateAsync(dto.ToEntity()) ? NoContent() : NotFound();
    }

    /// <summary>Elimina el registro de un estudio por su llave compuesta.</summary>
    /// <param name="idProf">Identificador de la profesión.</param>
    /// <param name="ccPer">Cédula de la persona.</param>
    [HttpDelete("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int idProf, int ccPer)
    {
        return await _estudios.DeleteAsync(idProf, ccPer) ? NoContent() : NotFound();
    }
}
