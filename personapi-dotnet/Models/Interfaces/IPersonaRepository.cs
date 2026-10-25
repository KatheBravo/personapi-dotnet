using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>
/// DAO (Data Access Object) de la entidad Persona. Define las operaciones CRUD
/// sin exponer cómo se accede a la base de datos.
/// </summary>
public interface IPersonaRepository
{
    Task<IEnumerable<Persona>> GetAllAsync();
    Task<Persona?> GetByIdAsync(int cc);
    Task<bool> ExistsAsync(int cc);
    Task<Persona> CreateAsync(Persona persona);
    Task<bool> UpdateAsync(Persona persona);
    Task<bool> DeleteAsync(int cc);
}
