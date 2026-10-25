using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers;

/// <summary>Controlador MVC (vistas Razor) del CRUD de profesiones.</summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class ProfesionesController : Controller
{
    private readonly IProfesionRepository _profesiones;

    public ProfesionesController(IProfesionRepository profesiones)
    {
        _profesiones = profesiones;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _profesiones.GetAllAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var profesion = await _profesiones.GetByIdAsync(id.Value);
        return profesion is null ? NotFound() : View(profesion);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nom,Des")] Profesion profesion)
    {
        if (ModelState.IsValid && await _profesiones.ExistsAsync(profesion.Id))
            ModelState.AddModelError(nameof(Profesion.Id), "Ya existe una profesión con ese id.");

        if (ModelState.IsValid)
        {
            try
            {
                await _profesiones.CreateAsync(profesion);
                TempData["Mensaje"] = $"Profesión {profesion.Nom} creada.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar: " + ex.GetBaseException().Message);
            }
        }
        return View(profesion);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var profesion = await _profesiones.GetByIdAsync(id.Value);
        return profesion is null ? NotFound() : View(profesion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Des")] Profesion profesion)
    {
        if (id != profesion.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (!await _profesiones.UpdateAsync(profesion)) return NotFound();
                TempData["Mensaje"] = "Profesión actualizada.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar: " + ex.GetBaseException().Message);
            }
        }
        return View(profesion);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var profesion = await _profesiones.GetByIdAsync(id.Value);
        return profesion is null ? NotFound() : View(profesion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _profesiones.DeleteAsync(id);
        TempData["Mensaje"] = "Profesión eliminada.";
        return RedirectToAction(nameof(Index));
    }
}
