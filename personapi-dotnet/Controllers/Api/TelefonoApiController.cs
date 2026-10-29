using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>Endpoints REST del CRUD de teléfonos.</summary>
// [ApiController] activa el comportamiento típico de una API: valida el JSON contra el DTO
// y responde 400 automáticamente si algo está mal.
// [Route] fija la URL base de todos los endpoints de esta clase.
// [Produces] indica que siempre responde JSON (Swagger lo muestra así).
[ApiController]
[Route("api/telefonos")]
[Produces("application/json")]
// Hereda de ControllerBase (no de Controller) porque no necesita vistas: solo devuelve datos.
public class TelefonoApiController : ControllerBase
{
    private readonly ITelefonoRepository _telefonos;
    private readonly IPersonaRepository _personas;

    public TelefonoApiController(ITelefonoRepository telefonos, IPersonaRepository personas)
    {
        _telefonos = telefonos;
        _personas = personas;
    }

    // GET -> leer. Ok(...) produce un 200 con la lista en JSON.
    /// <summary>Lista todos los teléfonos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TelefonoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TelefonoDto>>> GetAll()
    {
        var telefonos = await _telefonos.GetAllAsync();
        return Ok(telefonos.Select(t => t.ToDto()));
    }

    /// <summary>Obtiene un teléfono por su número.</summary>
    // El número es texto, por eso la ruta no lleva la restricción :int como en las otras.
    [HttpGet("{num}")]
    [ProducesResponseType(typeof(TelefonoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TelefonoDto>> GetById(string num)
    {
        var telefono = await _telefonos.GetByIdAsync(num);
        return telefono is null ? NotFound() : Ok(telefono.ToDto());
    }

    /// <summary>Crea un teléfono (el dueño debe existir).</summary>
    // POST -> crear. Antes de guardar se revisa que la llave no exista (para responder 409
    // con un mensaje claro en vez de dejar que SQL Server lance un error de llave duplicada).
    // CreatedAtAction responde 201 Created con la URL del recurso nuevo en el encabezado Location.
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TelefonoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TelefonoDto>> Create([FromBody] TelefonoDto dto)
    {
        if (!await _personas.ExistsAsync(dto.Duenio))
            return BadRequest(new { mensaje = $"No existe la persona con cédula {dto.Duenio}." });
        if (await _telefonos.ExistsAsync(dto.Num))
            return Conflict(new { mensaje = $"El número {dto.Num} ya está registrado." });

        try
        {
            var creado = await _telefonos.CreateAsync(dto.ToEntity());
            return CreatedAtAction(nameof(GetById), new { num = creado.Num }, creado.ToDto());
        }
        // Red de seguridad: si aun así la base rechaza el cambio (por ejemplo, dos peticiones
        // iguales al mismo tiempo), se responde 409 en lugar de un error 500.
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensaje = "No se pudo guardar el teléfono.", detalle = ex.InnerException?.Message });
        }
    }

    /// <summary>Actualiza operador y dueño de un teléfono (el número no cambia).</summary>
    // PUT -> actualizar. La llave viaja en la URL y también en el cuerpo; si no coinciden es un error
    // del cliente (400). Si todo sale bien se responde 204 No Content: hecho, sin nada que devolver.
    [HttpPut("{num}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string num, [FromBody] TelefonoDto dto)
    {
        if (num != dto.Num)
            return BadRequest(new { mensaje = "El número de la URL y el del cuerpo deben coincidir." });
        if (!await _personas.ExistsAsync(dto.Duenio))
            return BadRequest(new { mensaje = $"No existe la persona con cédula {dto.Duenio}." });

        return await _telefonos.UpdateAsync(dto.ToEntity()) ? NoContent() : NotFound();
    }

    /// <summary>Elimina un teléfono.</summary>
    // DELETE -> eliminar. 204 si se borró, 404 si no existía.
    [HttpDelete("{num}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string num)
    {
        return await _telefonos.DeleteAsync(num) ? NoContent() : NotFound();
    }
}
