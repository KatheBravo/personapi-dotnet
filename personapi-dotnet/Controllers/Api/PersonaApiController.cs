using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.DTOs;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers.Api;

/// <summary>Endpoints REST del CRUD de personas.</summary>
// [ApiController] activa el comportamiento típico de una API: valida el JSON contra el DTO
// y responde 400 automáticamente si algo está mal.
// [Route] fija la URL base de todos los endpoints de esta clase.
// [Produces] indica que siempre responde JSON (Swagger lo muestra así).
[ApiController]
[Route("api/personas")]
[Produces("application/json")]
// Hereda de ControllerBase (no de Controller) porque no necesita vistas: solo devuelve datos.
public class PersonaApiController : ControllerBase
{
    private readonly IPersonaRepository _personas;
    private readonly IEstudioRepository _estudios;
    private readonly ITelefonoRepository _telefonos;

    // El constructor recibe las interfaces DAO; el contenedor de dependencias
    // (configurado en Program.cs) entrega automáticamente los repositorios reales.
    public PersonaApiController(IPersonaRepository personas, IEstudioRepository estudios, ITelefonoRepository telefonos)
    {
        _personas = personas;
        _estudios = estudios;
        _telefonos = telefonos;
    }

    // GET -> leer. Ok(...) produce un 200 con la lista en JSON.
    /// <summary>Lista todas las personas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PersonaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PersonaDto>>> GetAll()
    {
        var personas = await _personas.GetAllAsync();
        return Ok(personas.Select(p => p.ToDto()));
    }

    /// <summary>Obtiene una persona por su cédula.</summary>
    [HttpGet("{cc:int}")]
    [ProducesResponseType(typeof(PersonaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonaDto>> GetById(int cc)
    {
        var persona = await _personas.GetByIdAsync(cc);
        // Si no existe respondemos 404; si existe, la convertimos a DTO y respondemos 200.
        return persona is null ? NotFound() : Ok(persona.ToDto());
    }

    /// <summary>Lista los teléfonos de una persona.</summary>
    [HttpGet("{cc:int}/telefonos")]
    [ProducesResponseType(typeof(IEnumerable<TelefonoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TelefonoDto>>> GetTelefonos(int cc)
    {
        if (!await _personas.ExistsAsync(cc)) return NotFound();
        var telefonos = await _telefonos.GetByPersonaAsync(cc);
        return Ok(telefonos.Select(t => t.ToDto()));
    }

    /// <summary>Lista los estudios de una persona.</summary>
    [HttpGet("{cc:int}/estudios")]
    [ProducesResponseType(typeof(IEnumerable<EstudioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<EstudioDto>>> GetEstudios(int cc)
    {
        if (!await _personas.ExistsAsync(cc)) return NotFound();
        var estudios = await _estudios.GetByPersonaAsync(cc);
        return Ok(estudios.Select(e => e.ToDto()));
    }

    /// <summary>Crea una persona.</summary>
    // POST -> crear. Antes de guardar se revisa que la llave no exista (para responder 409
    // con un mensaje claro en vez de dejar que SQL Server lance un error de llave duplicada).
    // CreatedAtAction responde 201 Created con la URL del recurso nuevo en el encabezado Location.
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(PersonaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonaDto>> Create([FromBody] PersonaDto dto)
    {
        if (await _personas.ExistsAsync(dto.Cc))
            return Conflict(new { mensaje = $"Ya existe una persona con cédula {dto.Cc}." });

        try
        {
            var creada = await _personas.CreateAsync(dto.ToEntity());
            return CreatedAtAction(nameof(GetById), new { cc = creada.Cc }, creada.ToDto());
        }
        // Red de seguridad: si aun así la base rechaza el cambio (por ejemplo, dos peticiones
        // iguales al mismo tiempo), se responde 409 en lugar de un error 500.
        catch (DbUpdateException ex)
        {
            return Conflict(new { mensaje = "No se pudo guardar la persona.", detalle = ex.InnerException?.Message });
        }
    }

    /// <summary>Actualiza una persona (la cédula no cambia).</summary>
    // PUT -> actualizar. La llave viaja en la URL y también en el cuerpo; si no coinciden es un error
    // del cliente (400). Si todo sale bien se responde 204 No Content: hecho, sin nada que devolver.
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

    /// <summary>Elimina una persona junto con sus estudios y teléfonos.</summary>
    // DELETE -> eliminar. 204 si se borró, 404 si no existía.
    [HttpDelete("{cc:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int cc)
    {
        return await _personas.DeleteAsync(cc) ? NoContent() : NotFound();
    }
}
