using Microsoft.AspNetCore.Mvc;

namespace JLM.Controllers;

public class ErrorController : Controller
{
    [HttpGet("/erro/{statusCode:int?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index(int? statusCode = 500)
    {
        var code = statusCode ?? 500;
        Response.StatusCode = code;
        ViewBag.StatusCode = code;
        ViewBag.Message = code switch
        {
            400 => "A requisição não pôde ser processada.",
            401 => "Faça login para continuar.",
            403 => "Você não tem permissão para acessar esta página.",
            404 => "A página que você procura não existe ou foi movida.",
            _ => "Algo saiu do eixo. Estamos ajustando os detalhes."
        };
        return View();
    }
}
