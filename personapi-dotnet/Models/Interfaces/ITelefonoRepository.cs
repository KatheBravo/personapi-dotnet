using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>Contrato DAO para operaciones de persistencia de teléfonos.</summary>
public interface ITelefonoRepository
{
    Task<IEnumerable<Telefono>> GetAllAsync();
    Task<Telefono?> GetByIdAsync(string num);
    Task<IEnumerable<Telefono>> GetByPersonaAsync(int ccPer);
    Task<bool> ExistsAsync(string num);
    Task<Telefono> CreateAsync(Telefono telefono);
    Task<bool> UpdateAsync(Telefono telefono);
    Task<bool> DeleteAsync(string num);
}
