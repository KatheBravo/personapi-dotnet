using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Models.Repositories;

/// <summary>Implementación del DAO de Telefono usando Entity Framework Core.</summary>
public class TelefonoRepository : ITelefonoRepository
{
    private readonly PersonaDbContext _context;

    public TelefonoRepository(PersonaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Telefono>> GetAllAsync()
    {
        return await _context.Telefonos
            .AsNoTracking()
            .Include(t => t.DuenioNavigation)
            .OrderBy(t => t.Duenio).ThenBy(t => t.Num)
            .ToListAsync();
    }

    public async Task<IEnumerable<Telefono>> GetByPersonaAsync(int duenio)
    {
        return await _context.Telefonos
            .AsNoTracking()
            .Where(t => t.Duenio == duenio)
            .ToListAsync();
    }

    public async Task<Telefono?> GetByIdAsync(string num)
    {
        return await _context.Telefonos
            .AsNoTracking()
            .Include(t => t.DuenioNavigation)
            .FirstOrDefaultAsync(t => t.Num == num);
    }

    public Task<bool> ExistsAsync(string num)
    {
        return _context.Telefonos.AnyAsync(t => t.Num == num);
    }

    public async Task<Telefono> CreateAsync(Telefono telefono)
    {
        _context.Telefonos.Add(telefono);
        await _context.SaveChangesAsync();
        return telefono;
    }

    public async Task<bool> UpdateAsync(Telefono telefono)
    {
        var existente = await _context.Telefonos.FindAsync(telefono.Num);
        if (existente is null) return false;

        existente.Oper = telefono.Oper;
        existente.Duenio = telefono.Duenio;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string num)
    {
        var existente = await _context.Telefonos.FindAsync(num);
        if (existente is null) return false;

        _context.Telefonos.Remove(existente);
        await _context.SaveChangesAsync();
        return true;
    }
}
