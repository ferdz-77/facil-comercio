using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FacilComercio.Models;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Data;

public class SessionMiddleware
{
    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {

        // Verifica se o usuário está autenticado e a sessão está vazia
        if (context.User.Identity.IsAuthenticated && context.Session.GetInt32("idUsuario") == null)
        {
            var email = context.User.Identity.Name;
            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

            var usuario = await dbContext.Usuarios
                .Include(u => u.UsuarioLojas)
                .FirstOrDefaultAsync(u => u.Usu_Email == email);

            if (usuario != null)
            {
                // Armazena os dados do usuário na sessão
                context.Session.SetInt32("idUsuario", usuario.Usu_Id);
                context.Session.SetString("Nome", usuario.Usu_Nome);

                var usuarioLoja = usuario.UsuarioLojas.FirstOrDefault();
                if (usuarioLoja != null)
                {
                    context.Session.SetInt32("Loj_Id", usuarioLoja.Loj_Id);
                    context.Session.SetString("Permissao", usuarioLoja.Permissao);
                }
            }
        }
       

        await _next(context);
    }
}
