using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ProfesionesController : Controller
{
    private readonly IProfesionRepository _profesiones;

    public ProfesionesController(IProfesionRepository profesiones)
    {
        _profesiones = profesiones;
    }

    // GET: Profesiones
    public async Task<IActionResult> Index()
    {
        return View(await _profesiones.GetAllAsync());
    }

    // GET: Profesiones/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var profesion = await _profesiones.GetByIdAsync(id.Value);
        return profesion is null ? NotFound() : View(profesion);
    }

    // GET: Profesiones/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Profesiones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nom,Des")] Profesion profesion)
    {
        // Validación de identificador único
        if (ModelState.IsValid && await _profesiones.ExistsAsync(profesion.Id))
            ModelState.AddModelError(nameof(Profesion.Id), "Ya existe una profesión con ese id.");

        if (ModelState.IsValid)
        {
            try
            {
                await _profesiones.CreateAsync(profesion);
                TempData["Mensaje"] = $"Profesión {profesion.Nom} creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar: " + ex.GetBaseException().Message);
            }
        }
        return View(profesion);
    }

    // GET: Profesiones/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var profesion = await _profesiones.GetByIdAsync(id.Value);
        return profesion is null ? NotFound() : View(profesion);
    }

    // POST: Profesiones/Edit/5
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
                TempData["Mensaje"] = "Profesión actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar: " + ex.GetBaseException().Message);
            }
        }
        return View(profesion);
    }

    // GET: Profesiones/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var profesion = await _profesiones.GetByIdAsync(id.Value);
        return profesion is null ? NotFound() : View(profesion);
    }

    // POST: Profesiones/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _profesiones.DeleteAsync(id);
        TempData["Mensaje"] = "Profesión eliminada exitosamente.";
        return RedirectToAction(nameof(Index));
    }
}
