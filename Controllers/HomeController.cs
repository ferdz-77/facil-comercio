using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FacilComercio.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using FacilComercio.Models.DTOs;
using WebApplication1.Data; // Ajuste para o namespace correto



namespace FacilComercio.Controllers
{
    [Authorize] // Garante que apenas usuários autenticados acessem
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Verifica se a sessão contém o idUsuario
            if (HttpContext.Session.GetInt32("idUsuario") == null)
            {
                // Recarrega as informações do usuário com base na autenticação
                var email = User.Identity.Name; // Assume que o email foi usado como identidade
                if (email != null)
                {
                    var usuario = _context.Usuarios.FirstOrDefault(u => u.Usu_Email == email);

                    if (usuario != null)
                    {
                        // Armazena as informações do usuário na sessão
                        HttpContext.Session.SetInt32("idUsuario", usuario.Usu_Id);
                        HttpContext.Session.SetString("Nome", usuario.Usu_Nome);

                        // Carrega as permissões do usuário
                        var usuarioLoja = _context.UsuarioLojas.FirstOrDefault(ul => ul.Usu_Id == usuario.Usu_Id);
                        if (usuarioLoja != null)
                        {
                            HttpContext.Session.SetInt32("Loj_Id", usuarioLoja.Loj_Id);
                            HttpContext.Session.SetString("Permissao", usuarioLoja.Permissao);
                        }
                    }
                }
            }

            // Atribui o idUsuario e outros dados da sessão ao ViewBag
            ViewBag.idUsuario = HttpContext.Session.GetInt32("idUsuario");
            ViewBag.nomeUsuario = HttpContext.Session.GetString("Nome");
            ViewBag.Permissao = HttpContext.Session.GetString("Permissao");

            base.OnActionExecuting(context);
        }

        // Ação para a página inicial (após o login)
        public async Task<IActionResult> Index()
        {

            // Recuperar Loj_Id do usuário logado da sessão
            var lojaId = HttpContext.Session.GetInt32("Loj_Id");

            if (lojaId == null)
            {
                TempData["MensagemErro"] = "Erro: Loja não encontrada na sessão.";
                return RedirectToAction("Login", "Account");
            }

            // Chamar a procedure para buscar os usuários da loja
            var lojIdParam = new SqlParameter("@Loj_Id", lojaId);
            var usuarios = await _context.UsuarioListagemDtos
                .FromSqlRaw("EXEC sp_ListarUsuariosPorLoja @Loj_Id", lojIdParam)
                .AsNoTracking()
                .ToListAsync();

            // Passar os dados para a View
            ViewBag.Usuarios = usuarios;

            try
            {
                // Chama a procedure para buscar produtos com estoque baixo
                var produtosEstoqueBaixo = await _context.Set<EstoqueBaixoViewModel>()
                    .FromSqlRaw("EXEC sp_ProdutosEstoqueBaixo @Loj_Id", lojIdParam)
                    .ToListAsync();

                // Contar o total de produtos
                var totalProdutos = await _context.Produtos
                    .Where(p => p.Loj_Id == lojaId.Value)
                    .CountAsync();

                // Passar informações para a View
                ViewBag.TotalProdutos = totalProdutos;
                ViewBag.TotalEstoqueBaixo = produtosEstoqueBaixo.Count;

                // Passar os produtos para a View
                return View(produtosEstoqueBaixo);

            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao carregar produtos: {ex.Message}";
                return View(new List<Produto>()); // Retorna uma lista vazia em caso de erro
            }

            //return View();
        }

    }
}
