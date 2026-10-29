using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class TelefonosController : Controller
{
    private readonly ITelefonoRepository _telefonos;
    private readonly IPersonaRepository _personas;

    public TelefonosController(ITelefonoRepository telefonos, IPersonaRepository personas)
    {
        _telefonos = telefonos;
        _personas = personas;
    }

    // GET: Telefonos
    public async Task<IActionResult> Index()
    {
        return View(await _telefonos.GetAllAsync());
    }

    // GET: Telefonos/Details/3001112233
    public async Task<IActionResult> Details(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var telefono = await _telefonos.GetByIdAsync(id);
        return telefono is null ? NotFound() : View(telefono);
    }

    // GET: Telefonos/Create
    public async Task<IActionResult> Create(int? duenio)
    {
        await CargarPersonas(duenio);
        return View(new Telefono { Duenio = duenio ?? 0 });
    }

    // POST: Telefonos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Num,Oper,Duenio")] Telefono telefono)
    {
        // El dueño debe existir y el número telefónico no puede repetirse
        if (ModelState.IsValid)
        {
            if (!await _personas.ExistsAsync(telefono.Duenio))
                ModelState.AddModelError(nameof(Telefono.Duenio), "Seleccione un dueño válido.");
            if (await _telefonos.ExistsAsync(telefono.Num))
                ModelState.AddModelError(nameof(Telefono.Num), "Ese número ya está registrado.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _telefonos.CreateAsync(telefono);
                TempData["Mensaje"] = $"Teléfono {telefono.Num} creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar: " + ex.GetBaseException().Message);
            }
        }
        await CargarPersonas(telefono.Duenio);
        return View(telefono);
    }

    // GET: Telefonos/Edit/3001112233
    public async Task<IActionResult> Edit(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var telefono = await _telefonos.GetByIdAsync(id);
        if (telefono is null) return NotFound();
        await CargarPersonas(telefono.Duenio);
        return View(telefono);
    }

    // POST: Telefonos/Edit/3001112233
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Num,Oper,Duenio")] Telefono telefono)
    {
        if (id != telefono.Num) return NotFound();

        if (ModelState.IsValid && !await _personas.ExistsAsync(telefono.Duenio))
            ModelState.AddModelError(nameof(Telefono.Duenio), "Seleccione un dueño válido.");

        if (ModelState.IsValid)
        {
            try
            {
                if (!await _telefonos.UpdateAsync(telefono)) return NotFound();
                TempData["Mensaje"] = "Teléfono actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar: " + ex.GetBaseException().Message);
            }
        }
        await CargarPersonas(telefono.Duenio);
        return View(telefono);
    }

    // GET: Telefonos/Delete/3001112233
    public async Task<IActionResult> Delete(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var telefono = await _telefonos.GetByIdAsync(id);
        return telefono is null ? NotFound() : View(telefono);
    }

    // POST: Telefonos/Delete/3001112233
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _telefonos.DeleteAsync(id);
        TempData["Mensaje"] = "Teléfono eliminado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarPersonas(int? seleccionado)
    {
        var personas = (await _personas.GetAllAsync())
            .Select(p => new { p.Cc, Texto = $"{p.Cc} - {p.Nombre} {p.Apellido}" });
        ViewData["Personas"] = new SelectList(personas, "Cc", "Texto", seleccionado);
    }
}
