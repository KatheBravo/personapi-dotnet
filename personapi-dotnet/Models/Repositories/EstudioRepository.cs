using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Models.Repositories;

/// <summary>Implementación del DAO de Estudio (llave compuesta id_prof + cc_per).</summary>
public class EstudioRepository : IEstudioRepository
{
    private readonly PersonaDbContext _context;

    public EstudioRepository(PersonaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Estudio>> GetAllAsync()
    {
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
        var existente = await _context.Estudios.FindAsync(estudio.IdProf, estudio.CcPer);
        if (existente is null) return false;

        // Solo cambian los atributos no llave: fecha y universidad.
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
