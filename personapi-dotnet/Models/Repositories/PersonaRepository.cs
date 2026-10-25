using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Models.Repositories;

/// <summary>Implementación del DAO de Persona usando Entity Framework Core.</summary>
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

        // Copia solo las columnas escalares; la llave (cc) no cambia.
        _context.Entry(existente).CurrentValues.SetValues(persona);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int cc)
    {
        var existente = await _context.Personas.FindAsync(cc);
        if (existente is null) return false;

        // Los estudios y teléfonos de la persona se borran por ON DELETE CASCADE.
        _context.Personas.Remove(existente);
        await _context.SaveChangesAsync();
        return true;
    }
}
