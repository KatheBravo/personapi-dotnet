using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers;

/// <summary>Controlador MVC (vistas Razor) del CRUD de personas.</summary>
// IgnoreApi evita que Swagger muestre estas acciones: Swagger es solo para la API REST,
// y estas acciones devuelven páginas HTML, no JSON.
[ApiExplorerSettings(IgnoreApi = true)]
public class PersonasController : Controller
{
    private readonly IPersonaRepository _personas;

    public PersonasController(IPersonaRepository personas)
    {
        _personas = personas;
    }

    // Patrón de todas las acciones MVC:
    //   GET  -> muestra una página (lista, detalle o formulario vacío/lleno).
    //   POST -> recibe el formulario, valida, guarda con el repositorio y redirige.
    // Después de guardar se redirige (patrón Post/Redirect/Get) para que si el usuario
    // recarga la página el formulario no se envíe dos veces.

    // GET: Personas  -> lista de todas las personas
    public async Task<IActionResult> Index()
    {
        return View(await _personas.GetAllAsync());
    }

    // GET: Personas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var persona = await _personas.GetByIdAsync(id.Value);
        return persona is null ? NotFound() : View(persona);
    }

    // GET: Personas/Create
    public IActionResult Create()
    {
        CargarGeneros();
        return View();
    }

    // POST: Personas/Create
    // [ValidateAntiForgeryToken] exige el token oculto que pone el formulario: así nadie
    // puede enviar el formulario desde otra página (protección contra CSRF).
    // [Bind] limita qué campos se aceptan del formulario, para que no "cuelen" otros.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Cc,Nombre,Apellido,Genero,Edad")] Persona persona)
    {
        // ModelState.IsValid es true si se cumplieron todas las reglas de EntityMetadata.cs.
        // Además revisamos a mano una regla de negocio: la cédula no puede repetirse.
        if (ModelState.IsValid && await _personas.ExistsAsync(persona.Cc))
            ModelState.AddModelError(nameof(Persona.Cc), "Ya existe una persona con esa cédula.");

        if (ModelState.IsValid)
        {
            try
            {
                await _personas.CreateAsync(persona);
                // TempData sobrevive a una redirección: el layout lo muestra como aviso verde.
                TempData["Mensaje"] = $"Persona {persona.Nombre} {persona.Apellido} creada.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar: " + ex.GetBaseException().Message);
            }
        }
        // Si algo falló volvemos a mostrar el formulario con lo que el usuario escribió
        // y los mensajes de error junto a cada campo.
        CargarGeneros(persona.Genero);
        return View(persona);
    }

    // GET: Personas/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var persona = await _personas.GetByIdAsync(id.Value);
        if (persona is null) return NotFound();
        CargarGeneros(persona.Genero);
        return View(persona);
    }

    // POST: Personas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Cc,Nombre,Apellido,Genero,Edad")] Persona persona)
    {
        if (id != persona.Cc) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (!await _personas.UpdateAsync(persona)) return NotFound();
                TempData["Mensaje"] = "Persona actualizada.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar: " + ex.GetBaseException().Message);
            }
        }
        CargarGeneros(persona.Genero);
        return View(persona);
    }

    // GET: Personas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var persona = await _personas.GetByIdAsync(id.Value);
        return persona is null ? NotFound() : View(persona);
    }

    // POST: Personas/Delete/5
    // Se llama DeleteConfirmed porque ya existe un Delete(int?) para el GET con la misma firma;
    // ActionName("Delete") hace que la URL siga siendo /Personas/Delete/5.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _personas.DeleteAsync(id);
        TempData["Mensaje"] = "Persona eliminada.";
        return RedirectToAction(nameof(Index));
    }

    // Llena la lista desplegable de género (M/F) que usa el formulario.
    private void CargarGeneros(string? seleccionado = null)
    {
        ViewData["Generos"] = new SelectList(new[]
        {
            new { Valor = "M", Texto = "Masculino (M)" },
            new { Valor = "F", Texto = "Femenino (F)" }
        }, "Valor", "Texto", seleccionado);
    }
}
