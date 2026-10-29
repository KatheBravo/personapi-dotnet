using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class EstudiosController : Controller
{
    private readonly IEstudioRepository _estudios;
    private readonly IPersonaRepository _personas;
    private readonly IProfesionRepository _profesiones;

    public EstudiosController(IEstudioRepository estudios, IPersonaRepository personas, IProfesionRepository profesiones)
    {
        _estudios = estudios;
        _personas = personas;
        _profesiones = profesiones;
    }

    // GET: Estudios
    public async Task<IActionResult> Index()
    {
        return View(await _estudios.GetAllAsync());
    }

    // GET: Estudios/Details?idProf=1&ccPer=1010101010
    public async Task<IActionResult> Details(int? idProf, int? ccPer)
    {
        if (idProf is null || ccPer is null) return NotFound();
        var estudio = await _estudios.GetByIdAsync(idProf.Value, ccPer.Value);
        return estudio is null ? NotFound() : View(estudio);
    }

    // GET: Estudios/Create
    public async Task<IActionResult> Create(int? ccPer)
    {
        await CargarListas(null, ccPer);
        return View(new Estudio { CcPer = ccPer ?? 0 });
    }

    // POST: Estudios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdProf,CcPer,Fecha,Univer")] Estudio estudio)
    {
        // Validación de relaciones foráneas y clave compuesta única
        if (ModelState.IsValid)
        {
            if (!await _personas.ExistsAsync(estudio.CcPer))
                ModelState.AddModelError(nameof(Estudio.CcPer), "Seleccione una persona válida.");
            if (!await _profesiones.ExistsAsync(estudio.IdProf))
                ModelState.AddModelError(nameof(Estudio.IdProf), "Seleccione una profesión válida.");
            if (await _estudios.ExistsAsync(estudio.IdProf, estudio.CcPer))
                ModelState.AddModelError(string.Empty, "Esa persona ya tiene registrada esa profesión.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _estudios.CreateAsync(estudio);
                TempData["Mensaje"] = "Estudio registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar: " + ex.GetBaseException().Message);
            }
        }
        await CargarListas(estudio.IdProf, estudio.CcPer);
        return View(estudio);
    }

    // GET: Estudios/Edit?idProf=1&ccPer=1010101010
    public async Task<IActionResult> Edit(int? idProf, int? ccPer)
    {
        if (idProf is null || ccPer is null) return NotFound();
        var estudio = await _estudios.GetByIdAsync(idProf.Value, ccPer.Value);
        return estudio is null ? NotFound() : View(estudio);
    }

    // POST: Estudios/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("IdProf,CcPer,Fecha,Univer")] Estudio estudio)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if (!await _estudios.UpdateAsync(estudio)) return NotFound();
                TempData["Mensaje"] = "Estudio actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar: " + ex.GetBaseException().Message);
            }
        }
        var original = await _estudios.GetByIdAsync(estudio.IdProf, estudio.CcPer);
        if (original is not null)
        {
            estudio.CcPerNavigation = original.CcPerNavigation;
            estudio.IdProfNavigation = original.IdProfNavigation;
        }
        return View(estudio);
    }

    // GET: Estudios/Delete?idProf=1&ccPer=1010101010
    public async Task<IActionResult> Delete(int? idProf, int? ccPer)
    {
        if (idProf is null || ccPer is null) return NotFound();
        var estudio = await _estudios.GetByIdAsync(idProf.Value, ccPer.Value);
        return estudio is null ? NotFound() : View(estudio);
    }

    // POST: Estudios/Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int idProf, int ccPer)
    {
        await _estudios.DeleteAsync(idProf, ccPer);
        TempData["Mensaje"] = "Estudio eliminado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListas(int? idProf, int? ccPer)
    {
        var personas = (await _personas.GetAllAsync())
            .Select(p => new { p.Cc, Texto = $"{p.Cc} - {p.Nombre} {p.Apellido}" });
        var profesiones = (await _profesiones.GetAllAsync())
            .Select(p => new { p.Id, Texto = $"{p.Id} - {p.Nom}" });

        ViewData["Personas"] = new SelectList(personas, "Cc", "Texto", ccPer);
        ViewData["Profesiones"] = new SelectList(profesiones, "Id", "Texto", idProf);
    }
}
