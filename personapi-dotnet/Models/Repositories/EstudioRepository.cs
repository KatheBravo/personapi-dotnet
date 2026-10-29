using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Models.Repositories;

/// <summary>
/// Implementación del DAO de estudios. Lo particular aquí es la llave compuesta:
/// para encontrar un estudio siempre se necesitan la profesión (idProf) y la cédula (ccPer).
/// </summary>
public class EstudioRepository : IEstudioRepository
{
    private readonly PersonaDbContext _context;

    public EstudioRepository(PersonaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Estudio>> GetAllAsync()
    {
        // Incluimos persona y profesión para que la lista muestre nombres y no solo números.
        return await _context.Estudios
            .AsNoTracking()
            .Include(e => e.CcPerNavigation)
            .Include(e => e.IdProfNavigation)
            .OrderBy(e => e.CcPer).ThenBy(e => e.IdProf)
            .ToListAsync();
    }

    public async Task<IEnumerable<Estudio>> GetByPersonaAsync(int ccPer)
    {
        return await _context.Estudios
            .AsNoTracking()
            .Include(e => e.IdProfNavigation)
            .Where(e => e.CcPer == ccPer)
            .ToListAsync();
    }

    public async Task<Estudio?> GetByIdAsync(int idProf, int ccPer)
    {
        return await _context.Estudios
            .AsNoTracking()
            .Include(e => e.CcPerNavigation)
            .Include(e => e.IdProfNavigation)
            .FirstOrDefaultAsync(e => e.IdProf == idProf && e.CcPer == ccPer);
    }

    public Task<bool> ExistsAsync(int idProf, int ccPer)
    {
        return _context.Estudios.AnyAsync(e => e.IdProf == idProf && e.CcPer == ccPer);
    }

    public async Task<Estudio> CreateAsync(Estudio estudio)
    {
        _context.Estudios.Add(estudio);
        await _context.SaveChangesAsync();
        return estudio;
    }

    public async Task<bool> UpdateAsync(Estudio estudio)
    {
        // FindAsync recibe los valores de la llave en el mismo orden en que se definió
        // en el DbContext: primero IdProf y luego CcPer.
        var existente = await _context.Estudios.FindAsync(estudio.IdProf, estudio.CcPer);
        if (existente is null) return false;

        // Solo se pueden cambiar los datos que no son llave.
        existente.Fecha = estudio.Fecha;
        existente.Univer = estudio.Univer;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int idProf, int ccPer)
    {
        var existente = await _context.Estudios.FindAsync(idProf, ccPer);
        if (existente is null) return false;

        _context.Estudios.Remove(existente);
        await _context.SaveChangesAsync();
        return true;
    }
}
