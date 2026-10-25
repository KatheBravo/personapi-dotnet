using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Models.Repositories;

/// <summary>Implementación del DAO de Profesion usando Entity Framework Core.</summary>
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
            .OrderBy(p => p.Id)
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

        _context.Entry(existente).CurrentValues.SetValues(profesion);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existente = await _context.Profesions.FindAsync(id);
        if (existente is null) return false;

        _context.Profesions.Remove(existente);
        await _context.SaveChangesAsync();
        return true;
    }
}
