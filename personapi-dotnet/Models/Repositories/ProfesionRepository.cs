using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Models.Repositories;

/// <summary>Implementación DAO de Profesion mediante Entity Framework Core.</summary>
public class ProfesionRepository : IProfesionRepository
{
    private readonly PersonaDbContext _context;

    public ProfesionRepository(PersonaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Profesion>> GetAllAsync()
    {
        return await _context.Profesions
            .AsNoTracking()
            .OrderBy(p => p.Nom)
            .ToListAsync();
    }

    public async Task<Profesion?> GetByIdAsync(int id)
    {
        return await _context.Profesions
            .AsNoTracking()
            .Include(p => p.Estudios).ThenInclude(e => e.CcPerNavigation)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<bool> ExistsAsync(int id)
    {
        return _context.Profesions.AnyAsync(p => p.Id == id);
    }

    public async Task<Profesion> CreateAsync(Profesion profesion)
    {
        _context.Profesions.Add(profesion);
        await _context.SaveChangesAsync();
        return profesion;
    }

    public async Task<bool> UpdateAsync(Profesion profesion)
    {
        var existente = await _context.Profesions.FindAsync(profesion.Id);
        if (existente is null) return false;

        existente.Nom = profesion.Nom;
        existente.Des = profesion.Des;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var profesion = await _context.Profesions.FindAsync(id);
        if (profesion is null) return false;

        _context.Profesions.Remove(profesion);
        await _context.SaveChangesAsync();
        return true;
    }
}
