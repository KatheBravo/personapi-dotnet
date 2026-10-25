using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>
/// Endpoints REST del CRUD de estudios. La llave es compuesta, por eso las rutas
/// llevan los dos valores: /api/estudios/{idProf}/{ccPer}.
/// </summary>
[ApiController]
[Route("api/estudios")]
[Produces("application/json")]
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

    /// <summary>Lista todos los estudios.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EstudioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EstudioDto>>> GetAll()
    {
        var estudios = await _estudios.GetAllAsync();
        return Ok(estudios.Select(e => e.ToDto()));
    }

    /// <summary>Obtiene un estudio por profesión y cédula.</summary>
    [HttpGet("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(typeof(EstudioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudioDto>> GetById(int idProf, int ccPer)
    {
        var estudio = await _estudios.GetByIdAsync(idProf, ccPer);
        return estudio is null ? NotFound() : Ok(estudio.ToDto());
    }

    /// <summary>Registra un estudio (la persona y la profesión deben existir).</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(EstudioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EstudioDto>> Create([FromBody] EstudioDto dto)
    {
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

    /// <summary>Actualiza fecha y universidad de un estudio.</summary>
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

    /// <summary>Elimina un estudio.</summary>
    [HttpDelete("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int idProf, int ccPer)
    {
        return await _estudios.DeleteAsync(idProf, ccPer) ? NoContent() : NotFound();
    }
}
