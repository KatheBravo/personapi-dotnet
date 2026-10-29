using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>
/// DAO de la tabla telefono. El teléfono se identifica por su número (texto).
/// </summary>
public interface ITelefonoRepository
{
    /// <summary>Todos los teléfonos, con el nombre del dueño.</summary>
    Task<IEnumerable<Telefono>> GetAllAsync();

    /// <summary>Los teléfonos de una persona.</summary>
    Task<IEnumerable<Telefono>> GetByPersonaAsync(int duenio);

    /// <summary>Un teléfono por su número. Null si no existe.</summary>
    Task<Telefono?> GetByIdAsync(string num);

    /// <summary>¿Ese número ya está registrado?</summary>
    Task<bool> ExistsAsync(string num);

    /// <summary>Guarda un teléfono nuevo.</summary>
    Task<Telefono> CreateAsync(Telefono telefono);

    /// <summary>Cambia operador y dueño (el número no cambia). False si no existe.</summary>
    Task<bool> UpdateAsync(Telefono telefono);

    /// <summary>Elimina un teléfono. False si no existía.</summary>
    Task<bool> DeleteAsync(string num);
}
