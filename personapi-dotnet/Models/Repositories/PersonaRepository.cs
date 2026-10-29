using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Models.Repositories;

/// <summary>
/// Implementación real del DAO de personas. Es la única clase que sabe que los datos
/// están en SQL Server y que se llega a ellos con Entity Framework Core.
/// </summary>
public class PersonaRepository : IPersonaRepository
{
    private readonly PersonaDbContext _context;

    // No creamos el contexto con "new": nos lo entrega el contenedor de dependencias
    // (registrado en Program.cs), ya configurado con la cadena de conexión.
    public PersonaRepository(PersonaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Persona>> GetAllAsync()
    {
        // AsNoTracking: solo vamos a mostrar los datos, no a modificarlos,
        // así que le pedimos a EF que no los guarde en su memoria de cambios (es más rápido).
        return await _context.Personas
            .AsNoTracking()
            .OrderBy(p => p.Apellido).ThenBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Persona?> GetByIdAsync(int cc)
    {
        // Include trae también los datos relacionados en la misma consulta (un JOIN),
        // para que la vista de detalle pueda mostrar teléfonos y estudios con el nombre de la profesión.
        return await _context.Personas
            .AsNoTracking()
            .Include(p => p.Telefonos)
            .Include(p => p.Estudios).ThenInclude(e => e.IdProfNavigation)
            .FirstOrDefaultAsync(p => p.Cc == cc);
    }

    public Task<bool> ExistsAsync(int cc)
    {
        // AnyAsync genera un SELECT muy liviano que solo responde sí o no.
        return _context.Personas.AnyAsync(p => p.Cc == cc);
    }

    public async Task<Persona> CreateAsync(Persona persona)
    {
        _context.Personas.Add(persona);     // la marca como "nueva"
        await _context.SaveChangesAsync();  // aquí se ejecuta el INSERT
        return persona;
    }

    public async Task<bool> UpdateAsync(Persona persona)
    {
        // Primero buscamos el registro tal como está guardado.
        var existente = await _context.Personas.FindAsync(persona.Cc);
        if (existente is null) return false;

        // Copiamos encima solo los valores simples (nombre, apellido, género, edad).
        // La cédula es la llave y no cambia. EF detecta qué columnas cambiaron
        // y arma un UPDATE solo con esas.
        _context.Entry(existente).CurrentValues.SetValues(persona);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int cc)
    {
        var existente = await _context.Personas.FindAsync(cc);
        if (existente is null) return false;

        // No hace falta borrar a mano sus teléfonos y estudios:
        // la base de datos lo hace sola gracias a ON DELETE CASCADE (ver db/DDL.sql).
        _context.Personas.Remove(existente);
        await _context.SaveChangesAsync();
        return true;
    }
}
