using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>Contrato DAO para operaciones de persistencia de personas.</summary>
public interface IPersonaRepository
{
    Task<IEnumerable<Persona>> GetAllAsync();
    Task<Persona?> GetByIdAsync(int cc);
    Task<bool> ExistsAsync(int cc);
    Task<Persona> CreateAsync(Persona persona);
    Task<bool> UpdateAsync(Persona persona);
    Task<bool> DeleteAsync(int cc);
}
