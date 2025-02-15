using System;
using System.Collections.Generic;
using System.Linq;
using FacilComercio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data; // Ajuste o namespace para o seu projeto

namespace WebApplication1.Controllers
{
    public class DevolucoesController : Controller
    {

        private readonly AppDbContext _context;

        public DevolucoesController(AppDbContext context)
        {
            _context = context;
        }

        // Método executado antes de cada ação
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Recupera os dados da sessão e preenche o ViewBag
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            if (idUsuario.HasValue)
            {
                ViewBag.idUsuario = idUsuario.Value;
                ViewBag.nomeUsuario = HttpContext.Session.GetString("Nome");
                ViewBag.Permissao = HttpContext.Session.GetString("Permissao");
                ViewBag.Loj_Id = HttpContext.Session.GetInt32("Loj_Id");
            }
        }

        public async Task<IActionResult> Index(string busca, string cpf, int? vendaId, DateTime? dataInicio, DateTime? dataFim, int pagina = 1)
        {
            const int itensPorPagina = 10; // Define o número de itens por página

            // Obter a loja do usuário logado
            var lojId = HttpContext.Session.GetInt32("Loj_Id");
            if (lojId == null)
            {
                return RedirectToAction("Index", "Home"); // Redireciona para a Home se o usuário não estiver logado
            }

            // Parâmetros para a execução da procedure
            var parametros = new[]
            {
                new SqlParameter("@Loj_Id", lojId.Value),
                new SqlParameter("@Busca", busca ?? (object)DBNull.Value),
                 new SqlParameter("@Cpf", cpf ?? (object)DBNull.Value),
                new SqlParameter("@VendaId", vendaId ?? (object)DBNull.Value),
                new SqlParameter("@DataInicio", dataInicio ?? (object)DBNull.Value),
                new SqlParameter("@DataFim", dataFim ?? (object)DBNull.Value),
                new SqlParameter("@Pagina", pagina),
                new SqlParameter("@ItensPorPagina", itensPorPagina)
            };

            // Executa a consulta para buscar as vendas
            var vendas = await _context.Vendas
                .FromSqlRaw("EXEC sp_ListarVendas @Loj_Id, @Busca, @Cpf, @VendaId, @DataInicio, @DataFim, @Pagina, @ItensPorPagina", parametros)
                .ToListAsync();

            // Calcular a paginação
            var totalVendas = vendas.FirstOrDefault()?.Total ?? 0; // Certifique-se de que "Total" é retornado pela procedure
            var totalPaginas = (int)Math.Ceiling((double)totalVendas / itensPorPagina);
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(vendas);
        }

    }
}
