using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacilComercio.Models;
using Microsoft.Data.SqlClient;
using WebApplication1.Data; // Ajuste para o namespace correto



public class CategoriasController : Controller
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
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
        }
    }

    // GET: Categorias/ListarCategorias
    public async Task<IActionResult> ListarCategorias(string busca, int? filtroCategoria, int pagina = 1)
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
                new SqlParameter("@Pagina", pagina),
                new SqlParameter("@ItensPorPagina", itensPorPagina)
            };

        // Executa a consulta para buscar produtos com o total
        var categorias = await _context.Categorias
            .FromSqlRaw("EXEC sp_ListarCategoriasPorLoja @Loj_Id, @Busca, @Pagina, @ItensPorPagina", parametros)
            .ToListAsync();

        // Calcular a paginação
        var totalCategorias = categorias.FirstOrDefault()?.TotalCategorias ?? 0;
        var totalPaginas = (int)Math.Ceiling((double)totalCategorias / itensPorPagina);
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = totalPaginas;

        return View(categorias);
    }

    // GET: Categorias/Create
    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        // Obter o ID da loja do usuário logado
        var lojId = HttpContext.Session.GetInt32("Loj_Id");
        if (lojId == null)
        {
            ModelState.AddModelError("", "Erro: Loja não associada ao usuário logado.");
            return View(categoria);
        }

        // Passando o Loj_Id para a View
        ViewBag.Loj_Id = lojId;

        categoria.Loj_Id = lojId.Value; // Atribuir automaticamente o Loj_Id

        if (categoria.Loj_Id == 0) // Verifica se o valor está sendo enviado
        {
            ModelState.AddModelError("", "Erro: Loj_Id não foi recebido.");
            return View(categoria); // Retorna a view com o erro
        }

        // Verificar duplicidade de título
        var categoriaDuplicada = await _context.Categorias
            .AsNoTracking() // Garante que nenhuma propriedade extra seja carregada
            .Where(c => c.Cat_Titulo == categoria.Cat_Titulo && c.Loj_Id == categoria.Loj_Id)
            .Select(c => c.Cat_Id) // Evita carregar colunas desnecessárias
            .FirstOrDefaultAsync();

        if (categoriaDuplicada != 0)
        {
            ModelState.AddModelError("Cat_Titulo", "Já existe uma categoria com este título.");
            return View(categoria);
        }

        ModelState.Remove("Loja"); // Remove a validação para a propriedade de navegação

        if (ModelState.IsValid)
        {
            try
            {
                var parametros = new[]
                {
                        new SqlParameter("@Cat_Id", categoria.Cat_Id == 0 ? (object)DBNull.Value : categoria.Cat_Id),
                        new SqlParameter("@Loj_Id", categoria.Loj_Id),
                        new SqlParameter("@Cat_Titulo", categoria.Cat_Titulo),
                        new SqlParameter("@Cat_Descricao", categoria.Cat_Descricao ?? (object)DBNull.Value),
                    };

                // Chamar a procedure para salvar
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_CategoriaSalvar @Cat_Id, @Loj_Id, @Cat_Titulo, @Cat_Descricao",
                    parametros
                );

                TempData["MensagemSucesso"] = "Categoria cadastrada com sucesso!";
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar categoria: {ex.Message}");
                ModelState.AddModelError("", "Erro ao salvar a categoria. Por favor, tente novamente.");
            }
        }
        else
        {
            // Logar os erros do ModelState no console para depuração
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Erro no ModelState: {error.ErrorMessage}");
            }

            // Recarregar quaisquer dados necessários para a View, se aplicável
            var lojIdParam = new SqlParameter("@Loj_Id", categoria.Loj_Id);
            var categorias = await _context.Categorias
                .FromSqlRaw("EXEC sp_ListarCategoriasPorLoja @Loj_Id", lojIdParam)
                .ToListAsync();
            ViewBag.Categorias = categorias;

            // Retornar a View para o usuário corrigir os erros
            return View(categoria);
        }


        return View(categoria);
    }

    // GET: Categorias/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Recupera o produto pelo ID diretamente
        var categoria = await _context.Categorias
            .Where(c => c.Cat_Id == id)
            .Select(c => new Categoria
            {
                Cat_Id = c.Cat_Id,
                Cat_Descricao = c.Cat_Descricao,
                Cat_CriadoEm = c.Cat_CriadoEm,
                Cat_Titulo = c.Cat_Titulo,
                Cat_Status = c.Cat_Status,
                TotalCategorias = 0
            })
            .FirstOrDefaultAsync();


        if (categoria == null)
        {
            return NotFound();
        }
        // Passar a categoria para a View
        return View(categoria);
    }

    // POST: Categorias/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria categoria)
    {
        if (id != categoria.Cat_Id)
        {
            return NotFound();
        }

        // Obter o Loj_Id do usuário logado
        var lojId = HttpContext.Session.GetInt32("Loj_Id");
        if (lojId == null)
        {
            ModelState.AddModelError("", "Erro: Loja não associada ao usuário logado.");
            return View(categoria);
        }

        categoria.Loj_Id = lojId.Value; // Garantir que o Loj_Id esteja correto

        // Verificar duplicidade de título
        var categoriaDuplicada = await _context.Categorias
            .AsNoTracking() // Garante que nenhuma propriedade extra seja carregada
            .Where(c => c.Cat_Titulo == categoria.Cat_Titulo && c.Loj_Id == categoria.Loj_Id)
            .Select(c => c.Cat_Id) // Evita carregar colunas desnecessárias
            .FirstOrDefaultAsync();

        if (categoriaDuplicada != 0)
        {
            ModelState.AddModelError("Cat_Titulo", "Já existe uma categoria com este título.");
            return View(categoria);
        }

        ModelState.Remove("Loja"); // Remover validação para a propriedade de navegação

        if (ModelState.IsValid)
        {
            try
            {
                // Configurar os parâmetros para a procedure
                var parametros = new[]
                {
                        new SqlParameter("@Cat_Id", categoria.Cat_Id),
                        new SqlParameter("@Loj_Id", categoria.Loj_Id),
                        new SqlParameter("@Cat_Titulo", categoria.Cat_Titulo),
                        new SqlParameter("@Cat_Descricao", categoria.Cat_Descricao ?? (object)DBNull.Value)
                    };

                // Chamar a procedure para atualizar a categoria
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_CategoriaSalvar @Cat_Id, @Loj_Id, @Cat_Titulo, @Cat_Descricao",
                    parametros
                );

                TempData["MensagemSucesso"] = "Categoria atualizada com sucesso!";
                return RedirectToAction("Edit", new { id = categoria.Cat_Id }); // Redireciona para a mesma página
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar categoria: {ex.Message}");
                ModelState.AddModelError("", "Erro ao atualizar a categoria. Por favor, tente novamente.");
            }
        }

        return View(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int categoriaId, bool status)
    {
        try
        {
            // Carregar apenas as colunas necessárias para o toggle
            var categoria = await _context.Categorias
                .Where(c => c.Cat_Id == categoriaId)
                .Select(c => new { c.Cat_Id, c.Cat_Status }) // Carregar somente as colunas essenciais
                .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound(new { success = false, message = "Categoria não encontrada." });
            }

            // Atualizar o status manualmente
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Categoria SET Cat_Status = @Status WHERE Cat_Id = @Id",
                new SqlParameter("@Status", status),
                new SqlParameter("@Id", categoriaId)
            );

            return Json(new { success = true, newStatus = status });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    private bool CategoriaExists(int id)
    {
        return _context.Categorias.Any(e => e.Cat_Id == id);
    }
}
