using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>Contrato DAO para operaciones de persistencia de profesiones.</summary>
public interface IProfesionRepository
{
    Task<IEnumerable<Profesion>> GetAllAsync();
    Task<Profesion?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<Profesion> CreateAsync(Profesion profesion);
    Task<bool> UpdateAsync(Profesion profesion);
    Task<bool> DeleteAsync(int id);
}
