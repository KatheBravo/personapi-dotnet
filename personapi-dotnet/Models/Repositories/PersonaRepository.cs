using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Models.Repositories;

/// <summary>Implementación DAO de Persona mediante Entity Framework Core.</summary>
public class PersonaRepository : IPersonaRepository
{
    private readonly PersonaDbContext _context;

    public PersonaRepository(PersonaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Persona>> GetAllAsync()
    {
        return await _context.Personas
            .AsNoTracking()
            .OrderBy(p => p.Apellido).ThenBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Persona?> GetByIdAsync(int cc)
    {
        // Incluye colecciones hijas para vistas y detalles completos
        return await _context.Personas
            .AsNoTracking()
            .Include(p => p.Telefonos)
            .Include(p => p.Estudios).ThenInclude(e => e.IdProfNavigation)
            .FirstOrDefaultAsync(p => p.Cc == cc);
    }

    public Task<bool> ExistsAsync(int cc)
    {
        return _context.Personas.AnyAsync(p => p.Cc == cc);
    }

    public async Task<Persona> CreateAsync(Persona persona)
    {
        _context.Personas.Add(persona);
        await _context.SaveChangesAsync();
        return persona;
    }

    public async Task<bool> UpdateAsync(Persona persona)
    {
        var existente = await _context.Personas.FindAsync(persona.Cc);
        if (existente is null) return false;

        existente.Nombre = persona.Nombre;
        existente.Apellido = persona.Apellido;
        existente.Genero = persona.Genero;
        existente.Edad = persona.Edad;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int cc)
    {
        var persona = await _context.Personas.FindAsync(cc);
        if (persona is null) return false;

        _context.Personas.Remove(persona);
        await _context.SaveChangesAsync();
        return true;
    }
}
