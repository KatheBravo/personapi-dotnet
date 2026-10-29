using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>Endpoints REST del CRUD de profesiones.</summary>
// [ApiController] activa el comportamiento típico de una API: valida el JSON contra el DTO
// y responde 400 automáticamente si algo está mal.
// [Route] fija la URL base de todos los endpoints de esta clase.
// [Produces] indica que siempre responde JSON (Swagger lo muestra así).
[ApiController]
[Route("api/profesiones")]
[Produces("application/json")]
// Hereda de ControllerBase (no de Controller) porque no necesita vistas: solo devuelve datos.
public class ProfesionApiController : ControllerBase
{
    private readonly IProfesionRepository _profesiones;

    public ProfesionApiController(IProfesionRepository profesiones)
    {
        _profesiones = profesiones;
    }

    // GET -> leer. Ok(...) produce un 200 con la lista en JSON.
    /// <summary>Lista todas las profesiones.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProfesionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProfesionDto>>> GetAll()
    {
        var profesiones = await _profesiones.GetAllAsync();
        return Ok(profesiones.Select(p => p.ToDto()));
    }

    /// <summary>Obtiene una profesión por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProfesionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfesionDto>> GetById(int id)
    {
        var profesion = await _profesiones.GetByIdAsync(id);
        return profesion is null ? NotFound() : Ok(profesion.ToDto());
    }

    /// <summary>Crea una profesión.</summary>
    // POST -> crear. Antes de guardar se revisa que la llave no exista (para responder 409
    // con un mensaje claro en vez de dejar que SQL Server lance un error de llave duplicada).
    // CreatedAtAction responde 201 Created con la URL del recurso nuevo en el encabezado Location.
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProfesionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfesionDto>> Create([FromBody] ProfesionDto dto)
    {
        if (await _profesiones.ExistsAsync(dto.Id))
            return Conflict(new { mensaje = $"Ya existe una profesión con id {dto.Id}." });

        try
        {
            var creada = await _profesiones.CreateAsync(dto.ToEntity());
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada.ToDto());
        }
        // Red de seguridad: si aun así la base rechaza el cambio (por ejemplo, dos peticiones
        // iguales al mismo tiempo), se responde 409 en lugar de un error 500.
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensaje = "No se pudo guardar la profesión.", detalle = ex.InnerException?.Message });
        }
    }

    /// <summary>Actualiza una profesión (el id no cambia).</summary>
    // PUT -> actualizar. La llave viaja en la URL y también en el cuerpo; si no coinciden es un error
    // del cliente (400). Si todo sale bien se responde 204 No Content: hecho, sin nada que devolver.
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProfesionDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { mensaje = "El id de la URL y el del cuerpo deben coincidir." });

        return await _profesiones.UpdateAsync(dto.ToEntity()) ? NoContent() : NotFound();
    }

    /// <summary>Elimina una profesión (y los estudios asociados).</summary>
    // DELETE -> eliminar. 204 si se borró, 404 si no existía.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        return await _profesiones.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
