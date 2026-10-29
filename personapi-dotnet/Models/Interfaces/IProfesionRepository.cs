using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>
/// DAO de la tabla profesion. Mismo contrato CRUD que el de personas;
/// la implementación está en ProfesionRepository.
/// </summary>
public interface IProfesionRepository
{
    /// <summary>Todas las profesiones, ordenadas por id.</summary>
    Task<IEnumerable<Profesion>> GetAllAsync();

    /// <summary>Una profesión por id, con las personas que la estudiaron. Null si no existe.</summary>
    Task<Profesion?> GetByIdAsync(int id);

    /// <summary>¿Ya existe una profesión con ese id?</summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>Guarda una profesión nueva.</summary>
    Task<Profesion> CreateAsync(Profesion profesion);

    /// <summary>Cambia nombre y descripción. False si no existe.</summary>
    Task<bool> UpdateAsync(Profesion profesion);

    /// <summary>Elimina la profesión (y en cascada los estudios que la usan). False si no existía.</summary>
    Task<bool> DeleteAsync(int id);
}
