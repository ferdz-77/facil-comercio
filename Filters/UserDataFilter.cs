using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Filters // Altere o namespace para corresponder ao do seu projeto
{
    public class UserDataFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                var httpContext = context.HttpContext;

                // Popula as ViewBag com os dados da sessão
                controller.ViewBag.idUsuario = httpContext.Session.GetInt32("Usuario_Id");
                controller.ViewBag.nomeUsuario = httpContext.Session.GetString("Usuario_Nome");
                controller.ViewBag.Permissao = httpContext.Session.GetString("Permissao");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Não faz nada após a execução da ação
        }
    }
}
