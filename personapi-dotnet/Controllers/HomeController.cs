using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Models;
using personapi_dotnet.Models.Interfaces;

namespace personapi_dotnet.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    private readonly IPersonaRepository _personas;
    private readonly IProfesionRepository _profesiones;
    private readonly IEstudioRepository _estudios;
    private readonly ITelefonoRepository _telefonos;

    public HomeController(
        IPersonaRepository personas,
        IProfesionRepository profesiones,
        IEstudioRepository estudios,
        ITelefonoRepository telefonos)
    {
        _personas = personas;
        _profesiones = profesiones;
        _estudios = estudios;
        _telefonos = telefonos;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel();
        try
        {
            model.Personas = (await _personas.GetAllAsync()).Count();
            model.Profesiones = (await _profesiones.GetAllAsync()).Count();
            model.Estudios = (await _estudios.GetAllAsync()).Count();
            model.Telefonos = (await _telefonos.GetAllAsync()).Count();
            model.BaseDeDatosDisponible = true;
        }
        catch (Exception ex)
        {
            model.BaseDeDatosDisponible = false;
            model.MensajeError = ex.GetBaseException().Message;
        }
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
