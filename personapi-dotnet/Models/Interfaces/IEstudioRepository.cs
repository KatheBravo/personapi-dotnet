using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>
/// DAO de la entidad Estudio. Su llave primaria es compuesta (id_prof, cc_per),
/// por eso las búsquedas reciben los dos valores.
/// </summary>
public interface IEstudioRepository
{
    Task<IEnumerable<Estudio>> GetAllAsync();
    Task<IEnumerable<Estudio>> GetByPersonaAsync(int ccPer);
    Task<Estudio?> GetByIdAsync(int idProf, int ccPer);
    Task<bool> ExistsAsync(int idProf, int ccPer);
    Task<Estudio> CreateAsync(Estudio estudio);
    Task<bool> UpdateAsync(Estudio estudio);
    Task<bool> DeleteAsync(int idProf, int ccPer);
}
