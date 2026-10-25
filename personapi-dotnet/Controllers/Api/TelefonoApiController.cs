using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>Endpoints REST del CRUD de teléfonos.</summary>
[ApiController]
[Route("api/telefonos")]
[Produces("application/json")]
public class TelefonoApiController : ControllerBase
{
    private readonly ITelefonoRepository _telefonos;
    private readonly IPersonaRepository _personas;

    public TelefonoApiController(ITelefonoRepository telefonos, IPersonaRepository personas)
    {
        _telefonos = telefonos;
        _personas = personas;
    }

    /// <summary>Lista todos los teléfonos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TelefonoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TelefonoDto>>> GetAll()
    {
        var telefonos = await _telefonos.GetAllAsync();
        return Ok(telefonos.Select(t => t.ToDto()));
    }

    /// <summary>Obtiene un teléfono por su número.</summary>
    [HttpGet("{num}")]
    [ProducesResponseType(typeof(TelefonoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TelefonoDto>> GetById(string num)
    {
        var telefono = await _telefonos.GetByIdAsync(num);
        return telefono is null ? NotFound() : Ok(telefono.ToDto());
    }

    /// <summary>Crea un teléfono (el dueño debe existir).</summary>
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
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensaje = "No se pudo guardar el teléfono.", detalle = ex.InnerException?.Message });
        }
    }

    /// <summary>Actualiza operador y dueño de un teléfono (el número no cambia).</summary>
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
    [HttpDelete("{num}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string num)
    {
        return await _telefonos.DeleteAsync(num) ? NoContent() : NotFound();
    }
}
