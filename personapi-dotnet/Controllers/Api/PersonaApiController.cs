using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>Endpoints REST del CRUD de personas.</summary>
[ApiController]
[Route("api/personas")]
[Produces("application/json")]
[Tags("Personas")]
public class PersonaApiController : ControllerBase
{
    private readonly IPersonaRepository _personas;
    private readonly IEstudioRepository _estudios;
    private readonly ITelefonoRepository _telefonos;

    public PersonaApiController(IPersonaRepository personas, IEstudioRepository estudios, ITelefonoRepository telefonos)
    {
        _personas = personas;
        _estudios = estudios;
        _telefonos = telefonos;
    }

    /// <summary>Lista todas las personas registradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PersonaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PersonaDto>>> GetAll()
    {
        var personas = await _personas.GetAllAsync();
        return Ok(personas.Select(p => p.ToDto()));
    }

    /// <summary>Obtiene una persona por su cédula.</summary>
    /// <param name="cc">Cédula de ciudadanía.</param>
    [HttpGet("{cc:int}")]
    [ProducesResponseType(typeof(PersonaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonaDto>> GetById(int cc)
    {
        var persona = await _personas.GetByIdAsync(cc);
        return persona is null ? NotFound() : Ok(persona.ToDto());
    }

    /// <summary>Lista los teléfonos asociados a una persona.</summary>
    /// <param name="cc">Cédula de la persona.</param>
    [HttpGet("{cc:int}/telefonos")]
    [ProducesResponseType(typeof(IEnumerable<TelefonoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TelefonoDto>>> GetTelefonos(int cc)
    {
        if (!await _personas.ExistsAsync(cc)) return NotFound();
        var telefonos = await _telefonos.GetByPersonaAsync(cc);
        return Ok(telefonos.Select(t => t.ToDto()));
    }

    /// <summary>Lista los estudios cursados por una persona.</summary>
    /// <param name="cc">Cédula de la persona.</param>
    [HttpGet("{cc:int}/estudios")]
    [ProducesResponseType(typeof(IEnumerable<EstudioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<EstudioDto>>> GetEstudios(int cc)
    {
        if (!await _personas.ExistsAsync(cc)) return NotFound();
        var estudios = await _estudios.GetByPersonaAsync(cc);
        return Ok(estudios.Select(e => e.ToDto()));
    }

    /// <summary>Crea una nueva persona.</summary>
    /// <param name="dto">Datos de la persona a registrar.</param>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(PersonaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonaDto>> Create([FromBody] PersonaDto dto)
    {
        // Evitamos insertar si la cédula ya existe en BD
        if (await _personas.ExistsAsync(dto.Cc))
            return Conflict(new { mensaje = $"Ya existe una persona con cédula {dto.Cc}." });

        try
        {
            var creada = await _personas.CreateAsync(dto.ToEntity());
            return CreatedAtAction(nameof(GetById), new { cc = creada.Cc }, creada.ToDto());
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensaje = "No se pudo guardar la persona.", detalle = ex.InnerException?.Message });
        }
    }

    /// <summary>Actualiza los datos de una persona existente.</summary>
    /// <param name="cc">Cédula de la persona a actualizar.</param>
    /// <param name="dto">Datos actualizados.</param>
    [HttpPut("{cc:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int cc, [FromBody] PersonaDto dto)
    {
        if (cc != dto.Cc)
            return BadRequest(new { mensaje = "La cédula de la URL y la del cuerpo deben coincidir." });

        return await _personas.UpdateAsync(dto.ToEntity()) ? NoContent() : NotFound();
    }

    /// <summary>Elimina una persona y sus dependencias (teléfonos y estudios en cascada).</summary>
    /// <param name="cc">Cédula de la persona a eliminar.</param>
    [HttpDelete("{cc:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int cc)
    {
        return await _personas.DeleteAsync(cc) ? NoContent() : NotFound();
    }
}
