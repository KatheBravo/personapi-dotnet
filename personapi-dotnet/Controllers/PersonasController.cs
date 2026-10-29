using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class PersonasController : Controller
{
    private readonly IPersonaRepository _personas;

    public PersonasController(IPersonaRepository personas)
    {
        _personas = personas;
    }

    // GET: Personas
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Cc,Nombre,Apellido,Genero,Edad")] Persona persona)
    {
        // Validación de cédula única
        if (ModelState.IsValid && await _personas.ExistsAsync(persona.Cc))
            ModelState.AddModelError(nameof(Persona.Cc), "Ya existe una persona con esa cédula.");

        if (ModelState.IsValid)
        {
            try
            {
                await _personas.CreateAsync(persona);
                TempData["Mensaje"] = $"Persona {persona.Nombre} {persona.Apellido} creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar: " + ex.GetBaseException().Message);
            }
        }
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
                TempData["Mensaje"] = "Persona actualizada exitosamente.";
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
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _personas.DeleteAsync(id);
        TempData["Mensaje"] = "Persona eliminada exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    private void CargarGeneros(string? seleccionado = null)
    {
        ViewData["Generos"] = new SelectList(new[]
        {
            new { Valor = "M", Texto = "Masculino (M)" },
            new { Valor = "F", Texto = "Femenino (F)" }
        }, "Valor", "Texto", seleccionado);
    }
}
