using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers;

/// <summary>Controlador MVC (vistas Razor) del CRUD de teléfonos.</summary>
// IgnoreApi evita que Swagger muestre estas acciones: Swagger es solo para la API REST,
// y estas acciones devuelven páginas HTML, no JSON.
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

    // Mismo esquema que PersonasController. El id de las rutas aquí es el número (texto).

    // GET: Telefonos
    public async Task<IActionResult> Index()
    {
        return View(await _telefonos.GetAllAsync());
    }

    public async Task<IActionResult> Details(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var telefono = await _telefonos.GetByIdAsync(id);
        return telefono is null ? NotFound() : View(telefono);
    }

    // GET: Telefonos/Create?duenio=...  (el dueño llega preseleccionado desde el detalle de la persona)
    public async Task<IActionResult> Create(int? duenio)
    {
        await CargarPersonas(duenio);
        return View(new Telefono { Duenio = duenio ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Num,Oper,Duenio")] Telefono telefono)
    {
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
                TempData["Mensaje"] = $"Teléfono {telefono.Num} creado.";
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

    public async Task<IActionResult> Edit(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var telefono = await _telefonos.GetByIdAsync(id);
        if (telefono is null) return NotFound();
        await CargarPersonas(telefono.Duenio);
        return View(telefono);
    }

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
                TempData["Mensaje"] = "Teléfono actualizado.";
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

    public async Task<IActionResult> Delete(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var telefono = await _telefonos.GetByIdAsync(id);
        return telefono is null ? NotFound() : View(telefono);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _telefonos.DeleteAsync(id);
        TempData["Mensaje"] = "Teléfono eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // Lista desplegable de personas para elegir al dueño del teléfono.
    private async Task CargarPersonas(int? seleccionado)
    {
        var personas = (await _personas.GetAllAsync())
            .Select(p => new { p.Cc, Texto = $"{p.Cc} - {p.Nombre} {p.Apellido}" });
        ViewData["Personas"] = new SelectList(personas, "Cc", "Texto", seleccionado);
    }
}
