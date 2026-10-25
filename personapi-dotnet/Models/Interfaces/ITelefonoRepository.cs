using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>DAO de la entidad Telefono.</summary>
public interface ITelefonoRepository
{
    Task<IEnumerable<Telefono>> GetAllAsync();
    Task<IEnumerable<Telefono>> GetByPersonaAsync(int duenio);
    Task<Telefono?> GetByIdAsync(string num);
    Task<bool> ExistsAsync(string num);
    Task<Telefono> CreateAsync(Telefono telefono);
    Task<bool> UpdateAsync(Telefono telefono);
    Task<bool> DeleteAsync(string num);
}
