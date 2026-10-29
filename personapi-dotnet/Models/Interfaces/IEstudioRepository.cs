using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>
/// DAO de la tabla estudios.
/// La diferencia con los demás es que un estudio se identifica con DOS valores
/// (la profesión y la cédula), por eso buscar, comprobar y borrar reciben ambos.
/// </summary>
public interface IEstudioRepository
{
    /// <summary>Todos los estudios, con el nombre de la persona y de la profesión.</summary>
    Task<IEnumerable<Estudio>> GetAllAsync();

    /// <summary>Los estudios de una persona en particular.</summary>
    Task<IEnumerable<Estudio>> GetByPersonaAsync(int ccPer);

    /// <summary>Un estudio por su llave compuesta. Null si no existe.</summary>
    Task<Estudio?> GetByIdAsync(int idProf, int ccPer);

    /// <summary>¿Esa persona ya tiene registrada esa profesión?</summary>
    Task<bool> ExistsAsync(int idProf, int ccPer);

    /// <summary>Registra un estudio nuevo.</summary>
    Task<Estudio> CreateAsync(Estudio estudio);

    /// <summary>Cambia fecha y universidad (la llave no se puede cambiar). False si no existe.</summary>
    Task<bool> UpdateAsync(Estudio estudio);

    /// <summary>Elimina un estudio. False si no existía.</summary>
    Task<bool> DeleteAsync(int idProf, int ccPer);
}
