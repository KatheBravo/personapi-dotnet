using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Models.Interfaces;

/// <summary>Contrato DAO para operaciones de persistencia de estudios.</summary>
public interface IEstudioRepository
{
    Task<IEnumerable<Estudio>> GetAllAsync();
    Task<Estudio?> GetByIdAsync(int idProf, int ccPer);
    Task<IEnumerable<Estudio>> GetByPersonaAsync(int ccPer);
    Task<IEnumerable<Estudio>> GetByProfesionAsync(int idProf);
    Task<bool> ExistsAsync(int idProf, int ccPer);
    Task<Estudio> CreateAsync(Estudio estudio);
    Task<bool> UpdateAsync(Estudio estudio);
    Task<bool> DeleteAsync(int idProf, int ccPer);
}
