using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>
/// Endpoints REST del CRUD de estudios. La llave es compuesta, por eso las rutas
/// llevan los dos valores: /api/estudios/{idProf}/{ccPer}.
/// </summary>
// [ApiController] activa el comportamiento típico de una API: valida el JSON contra el DTO
// y responde 400 automáticamente si algo está mal.
// [Route] fija la URL base de todos los endpoints de esta clase.
// [Produces] indica que siempre responde JSON (Swagger lo muestra así).
[ApiController]
[Route("api/estudios")]
[Produces("application/json")]
// Hereda de ControllerBase (no de Controller) porque no necesita vistas: solo devuelve datos.
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

    // GET -> leer. Ok(...) produce un 200 con la lista en JSON.
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
    // POST -> crear. Antes de guardar se revisa que la llave no exista (para responder 409
    // con un mensaje claro en vez de dejar que SQL Server lance un error de llave duplicada).
    // CreatedAtAction responde 201 Created con la URL del recurso nuevo en el encabezado Location.
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(EstudioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EstudioDto>> Create([FromBody] EstudioDto dto)
    {
        // Validaciones de negocio antes de guardar: que existan la persona y la profesión
        // (si no, 400) y que el estudio no esté repetido (si lo está, 409).
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
        // Red de seguridad: si aun así la base rechaza el cambio (por ejemplo, dos peticiones
        // iguales al mismo tiempo), se responde 409 en lugar de un error 500.
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensaje = "No se pudo guardar el estudio.", detalle = ex.InnerException?.Message });
        }
    }

    /// <summary>Actualiza fecha y universidad de un estudio.</summary>
    // PUT -> actualizar. La llave viaja en la URL y también en el cuerpo; si no coinciden es un error
    // del cliente (400). Si todo sale bien se responde 204 No Content: hecho, sin nada que devolver.
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
    // DELETE -> eliminar. 204 si se borró, 404 si no existía.
    [HttpDelete("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int idProf, int ccPer)
    {
        return await _estudios.DeleteAsync(idProf, ccPer) ? NoContent() : NotFound();
    }
}
