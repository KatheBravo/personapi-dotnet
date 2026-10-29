using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>Endpoints REST del CRUD de profesiones.</summary>
[ApiController]
[Route("api/profesiones")]
[Produces("application/json")]
[Tags("Profesiones")]
public class ProfesionApiController : ControllerBase
{
    private readonly IProfesionRepository _profesiones;

    public ProfesionApiController(IProfesionRepository profesiones)
    {
        _profesiones = profesiones;
    }

    /// <summary>Lista todas las profesiones registradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProfesionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProfesionDto>>> GetAll()
    {
        var profesiones = await _profesiones.GetAllAsync();
        return Ok(profesiones.Select(p => p.ToDto()));
    }

    /// <summary>Obtiene una profesión por su identificador.</summary>
    /// <param name="id">Identificador numérico de la profesión.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProfesionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfesionDto>> GetById(int id)
    {
        var profesion = await _profesiones.GetByIdAsync(id);
        return profesion is null ? NotFound() : Ok(profesion.ToDto());
    }

    /// <summary>Crea una nueva profesión.</summary>
    /// <param name="dto">Datos de la profesión a registrar.</param>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProfesionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfesionDto>> Create([FromBody] ProfesionDto dto)
    {
        // Validación de identificador único
        if (await _profesiones.ExistsAsync(dto.Id))
            return Conflict(new { mensaje = $"Ya existe una profesión con id {dto.Id}." });

        try
        {
            var creada = await _profesiones.CreateAsync(dto.ToEntity());
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada.ToDto());
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensaje = "No se pudo guardar la profesión.", detalle = ex.InnerException?.Message });
        }
    }

    /// <summary>Actualiza una profesión existente.</summary>
    /// <param name="id">Identificador de la profesión.</param>
    /// <param name="dto">Datos actualizados.</param>
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

    /// <summary>Elimina una profesión y sus estudios asociados en cascada.</summary>
    /// <param name="id">Identificador de la profesión.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        return await _profesiones.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
