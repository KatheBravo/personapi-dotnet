using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>
/// DAO (Data Access Object) de la tabla persona.
///
/// Una interfaz es un contrato: dice QUÉ se puede hacer con las personas, pero no
/// CÓMO se hace. Los controladores dependen solo de este contrato; quien realmente
/// habla con SQL Server es PersonaRepository. Así, si mañana cambiamos la forma de
/// guardar los datos, los controladores no se enteran.
///
/// Todos los métodos son asíncronos (Task) para que el servidor no se quede bloqueado
/// esperando a la base de datos mientras atiende otras peticiones.
/// </summary>
public interface IPersonaRepository
{
    /// <summary>Devuelve todas las personas, ordenadas por apellido y nombre.</summary>
    Task<IEnumerable<Persona>> GetAllAsync();

    /// <summary>Busca una persona por cédula, con sus teléfonos y estudios. Devuelve null si no existe.</summary>
    Task<Persona?> GetByIdAsync(int cc);

    /// <summary>Dice si ya existe una persona con esa cédula (útil para evitar duplicados).</summary>
    Task<bool> ExistsAsync(int cc);

    /// <summary>Guarda una persona nueva y la devuelve.</summary>
    Task<Persona> CreateAsync(Persona persona);

    /// <summary>Actualiza los datos de una persona existente. Devuelve false si no la encontró.</summary>
    Task<bool> UpdateAsync(Persona persona);

    /// <summary>Elimina la persona (y en cascada sus teléfonos y estudios). Devuelve false si no existía.</summary>
    Task<bool> DeleteAsync(int cc);
}
