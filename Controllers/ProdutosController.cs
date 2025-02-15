using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using FacilComercio.Models;
using Microsoft.Data.SqlClient;
using WebApplication1.Data; // Ajuste para o namespace correto
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace WebApplication1.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
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

        [HttpGet]
        public async Task<IActionResult> BuscarProdutos(string termo)
        {
            try
            {
                // Obter a loja do usuário logado
                var lojId = HttpContext.Session.GetInt32("Loj_Id");
                if (lojId == null)
                {
                    return Json(new { success = false, message = "Loja não identificada na sessão." });
                }

                // Busca por nome ou EAN
                var produtos = await _context.Produtos
                    .Where(p => p.Loj_Id == lojId.Value &&
                                (p.Prod_Nome.Contains(termo) || p.Prod_EAN.Contains(termo)))
                    .Select(p => new
                    {
                        p.Prod_Id,
                        p.Prod_Nome,
                        p.Prod_EAN,
                        Preco = p.Prod_Preco.ToString("C"),
                        Estoque = p.Prod_Estoque
                    })
                    .Take(10) // Limita o resultado para autocomplete
                    .ToListAsync();

                return Json(new { success = true, produtos });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar produtos: {ex.Message}");
                return Json(new { success = false, message = "Erro ao buscar produtos." });
            }
        }

        // GET: Produtos/Create
        public async Task<IActionResult> Create()
        {
            var lojId = HttpContext.Session.GetInt32("Loj_Id"); // Obter o ID da loja do usuário logado
            if (lojId == null)
            {
                return RedirectToAction("Index", "Home"); // Redirecionar se o ID da loja não estiver definido
            }

            // Buscar categorias ativas da loja associada
            var lojIdParam = new SqlParameter("@Loj_Id", lojId.Value);
            var categorias = await _context.Categorias
                .FromSqlRaw("EXEC sp_GetCategoriasAtivasPorLoja @Loj_Id", lojIdParam)
                .ToListAsync();

            ViewBag.Categorias = categorias;

            return View();
        }

        // POST: Produtos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produto produto, IFormFile? UploadImage)
        {


            var lojId = HttpContext.Session.GetInt32("Loj_Id"); // Obter o ID da loja do usuário logado
            if (lojId == null)
            {
                ModelState.AddModelError("", "Erro: Loja não associada ao usuário logado.");
                return View(produto);
            }

            produto.Loj_Id = lojId.Value; // Atribuir automaticamente o Loj_Id


            if (!string.IsNullOrEmpty(produto.Prod_EAN))
            {
                var produtoDuplicadoEAN = await _context.Produtos
                    .Where(p => p.Prod_EAN == produto.Prod_EAN && p.Loj_Id == produto.Loj_Id)
                    .Select(p => new { p.Prod_Id }) // Busca apenas o campo necessário
                    .FirstOrDefaultAsync();

                if (produtoDuplicadoEAN != null)
                {
                    ModelState.AddModelError("Prod_EAN", "Já existe um produto com este código EAN.");
                }
            }

            var produtoDuplicadoNome = await _context.Produtos
                    .Where(p => p.Prod_Nome == produto.Prod_Nome && p.Loj_Id == produto.Loj_Id)
                    .Select(p => new { p.Prod_Id }) // Busca apenas o campo necessário
                    .FirstOrDefaultAsync();

            if (produtoDuplicadoNome != null)
            {
                ModelState.AddModelError("Prod_Nome", "Já existe um produto com este nome.");
            }

            if (!ModelState.IsValid)
            {
                // Recarregar categorias em caso de erro
                var lojIdParam = new SqlParameter("@Loj_Id", produto.Loj_Id);
                ViewBag.Categorias = await _context.Categorias
                    .FromSqlRaw("EXEC sp_GetCategoriasAtivasPorLoja @Loj_Id", lojIdParam)
                    .ToListAsync();

                return View(produto);
            }



            if (!ModelState.IsValid)
            {
                // Recarregar categorias para corrigir o erro de NullReferenceException
                var lojIdParam = new SqlParameter("@Loj_Id", lojId.Value);
                var categorias = await _context.Categorias
                    .FromSqlRaw("EXEC sp_GetCategoriasAtivasPorLoja @Loj_Id", lojIdParam)
                    .ToListAsync();
                ViewBag.Categorias = categorias;

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Erro no ModelState: {error.ErrorMessage}");
                }

                return View(produto);
            }

            // Se uma imagem foi escolhida pelo usuário, fazer upload
            if (UploadImage != null && UploadImage.Length > 0)
            {
                try
                {
                    // Gera o caminho para salvar a imagem no diretório wwwroot/uploads/produtos
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "produtos");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder); // Cria o diretório, se não existir
                    }

                    var fileName = Path.GetFileName(UploadImage.FileName);
                    var filePath = Path.Combine(uploadFolder, fileName);

                    // Salva a imagem no diretório
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await UploadImage.CopyToAsync(stream);
                    }

                    // Define a URL relativa da imagem no banco de dados
                    produto.Prod_ImagemUrl = $"/uploads/produtos/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao fazer upload da imagem: {ex.Message}");
                    ModelState.AddModelError("", "Erro ao salvar a imagem. Por favor, tente novamente.");
                    return View(produto);
                }
            }
            //else if (!string.IsNullOrEmpty(produto.Prod_ImagemUrl) && produto.Prod_ImagemUrl.StartsWith("http"))
            else if (!string.IsNullOrEmpty(produto.Prod_ImagemUrl) && produto.Prod_ImagemUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))

            {
                //produto.Prod_ImagemUrl = produto.Prod_ImagemUrl;
            }

            else
            {
                //ModelState.AddModelError("", "É necessário fornecer uma imagem para o produto.");
                //return View(produto);
                produto.Prod_ImagemUrl = "/uploads/produtos/default.jpg"; // Caminho para uma imagem padrão
            }

            // Ajuste na passagem de parâmetros
            if (ModelState.IsValid)
            {
                try
                {
                    // Recupera a loja ID do usuário logado
                    var lojaId = HttpContext.Session.GetInt32("Loj_Id");
                    if (!lojaId.HasValue)
                    {
                        ModelState.AddModelError("", "A loja do usuário não foi identificada.");
                        return View(produto);
                    }

                    // Configurar os parâmetros para a procedure
                    var parametros = new[]
                    {
                        new SqlParameter("@Prod_Id", produto.Prod_Id == 0 ? (object)DBNull.Value : produto.Prod_Id),
                        new SqlParameter("@Loj_Id", lojaId.Value), // A loja vem do usuário logado
                        new SqlParameter("@Prod_Nome", produto.Prod_Nome ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_Descricao", produto.Prod_Descricao ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_Preco", produto.Prod_Preco),
                        new SqlParameter("@Prod_Estoque", produto.Prod_Estoque),
                        new SqlParameter("@Prod_EstoqueMinimo", produto.Prod_EstoqueMinimo),
                        new SqlParameter("@Prod_UnidadeMedida", produto.Prod_UnidadeMedida ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_EAN", produto.Prod_EAN ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_NCM", produto.Prod_NCM ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_CFOP", produto.Prod_CFOP ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_ImagemUrl", produto.Prod_ImagemUrl ?? (object)DBNull.Value),
                        new SqlParameter("@Cat_Id", produto.Cat_Id.HasValue ? (object)produto.Cat_Id.Value : DBNull.Value),
                        new SqlParameter("@Prod_Status", produto.Prod_Status),
                        new SqlParameter("@Prod_Marca", produto.Prod_Marca ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_Tamanho", produto.Prod_Tamanho ?? (object)DBNull.Value)
                    };

                    // Executar a procedure para salvar o produto
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_ProdutoSalvar @Prod_Id, @Loj_Id, @Prod_Nome, @Prod_Descricao, @Prod_Preco, @Prod_Estoque, @Prod_EstoqueMinimo, @Prod_UnidadeMedida, @Prod_EAN, @Prod_NCM, @Prod_CFOP, @Prod_ImagemUrl, @Cat_Id, @Prod_Status, @Prod_Marca, @Prod_Tamanho",
                        parametros);

                    TempData["MensagemSucesso"] = "Produto cadastrado com sucesso!";
                    return RedirectToAction("Create");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao salvar produto: {ex.Message}");
                    ModelState.AddModelError("", "Erro ao salvar o produto. Por favor, tente novamente.");
                }
            }


            return View(produto);

        }

        // GET: Produtos/ListarProdutos
        public async Task<IActionResult> ListarProdutos(string busca, int? filtroCategoria, int pagina = 1)
        {
            const int itensPorPagina = 10; // Define o número de itens por página

            // Obter a loja do usuário logado
            var lojId = HttpContext.Session.GetInt32("Loj_Id");
            if (lojId == null)
            {
                return RedirectToAction("Index", "Home"); // Redireciona para a Home se o usuário não estiver logado
            }

            // Buscar categorias para o filtro
            var lojIdParam = new SqlParameter("@Loj_Id", lojId.Value);
            var categorias = await _context.Categorias
                .FromSqlRaw("EXEC sp_GetCategoriasAtivasPorLoja @Loj_Id", lojIdParam)
                .ToListAsync();

            ViewBag.Categorias = categorias;

            // Parâmetros para a execução da procedure
            var parametros = new[]
            {
                new SqlParameter("@Loj_Id", lojId.Value),
                new SqlParameter("@Busca", busca ?? (object)DBNull.Value),
                new SqlParameter("@FiltroCategoria", filtroCategoria ?? (object)DBNull.Value),
                new SqlParameter("@Pagina", pagina),
                new SqlParameter("@ItensPorPagina", itensPorPagina)
            };

            // Executa a consulta para buscar produtos com o total
            var produtos = await _context.Produtos
                .FromSqlRaw("EXEC sp_ListarProdutosPorLoja @Loj_Id, @Busca, @FiltroCategoria, @Pagina, @ItensPorPagina", parametros)
                .ToListAsync();

            // Calcular a paginação
            var totalProdutos = produtos.FirstOrDefault()?.TotalProdutos ?? 0;
            var totalPaginas = (int)Math.Ceiling((double)totalProdutos / itensPorPagina);
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(produtos);
        }

       [HttpPost]
        public async Task<IActionResult> ToggleStatus(int produtoId, bool status)
        {
            try
            {
                // Carregar apenas as colunas necessárias para o toggle
                var produto = await _context.Produtos
                    .Where(p => p.Prod_Id == produtoId)
                    .Select(p => new { p.Prod_Id, p.Prod_Status }) // Carregar somente as colunas essenciais
                    .FirstOrDefaultAsync();

                if (produto == null)
                {
                    return NotFound(new { success = false, message = "Produto não encontrado." });
                }

                // Atualizar o status manualmente
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Produto SET Prod_Status = @Status WHERE Prod_Id = @Id",
                    new SqlParameter("@Status", status),
                    new SqlParameter("@Id", produtoId)
                );

                return Json(new { success = true, newStatus = status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: Produtos/Editar/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Retorna 404 se o ID não for informado
            }

            // Recupera o produto pelo ID diretamente
            var produto = await _context.Produtos
                .Where(p => p.Prod_Id == id)
                .Select(p => new Produto
                {
                    Prod_Id = p.Prod_Id,
                    Loj_Id = p.Loj_Id,
                    Prod_Nome = p.Prod_Nome,
                    Prod_Descricao = p.Prod_Descricao,
                    Prod_Preco = p.Prod_Preco,
                    Prod_Estoque = p.Prod_Estoque,
                    Prod_EstoqueMinimo = p.Prod_EstoqueMinimo,
                    Prod_UnidadeMedida = p.Prod_UnidadeMedida,
                    Prod_EAN = p.Prod_EAN,
                    Prod_NCM = p.Prod_NCM,
                    Prod_CFOP = p.Prod_CFOP,
                    Prod_ImagemUrl = p.Prod_ImagemUrl,
                    Cat_Id = p.Cat_Id,
                    Prod_Status = p.Prod_Status,
                    Prod_Marca = p.Prod_Marca,
                    Prod_Tamanho = p.Prod_Tamanho
                })
                .FirstOrDefaultAsync();


            if (produto == null)
            {
                return NotFound(); // Retorna 404 se o produto não for encontrado
            }

            return View(produto); // Retorna a view com o produto para edição
        }

        // POST: Produtos/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Produto produto, IFormFile? UploadImage)
        {
            if (id != produto.Prod_Id)
            {
                return NotFound(); // Retorna 404 se o ID não coincidir
            }

            // Obter a loja do usuário logado
            var lojId = HttpContext.Session.GetInt32("Loj_Id");
            if (lojId == null)
            {
                ModelState.AddModelError("", "Erro: Loja não associada ao usuário logado.");
                return View(produto);
            }

            produto.Loj_Id = lojId.Value;

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload da imagem, se necessário
                    if (UploadImage != null && UploadImage.Length > 0)
                    {
                        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "produtos");
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder); // Cria o diretório, se necessário
                        }

                        var fileName = Path.GetFileName(UploadImage.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await UploadImage.CopyToAsync(stream);
                        }

                        produto.Prod_ImagemUrl = $"/uploads/produtos/{fileName}";
                    }

                    // Executar procedure para atualizar o produto
                    var parametros = new[]
                    {
                        new SqlParameter("@Prod_Id", produto.Prod_Id),
                        new SqlParameter("@Loj_Id", produto.Loj_Id),
                        new SqlParameter("@Prod_Nome", produto.Prod_Nome ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_Descricao", produto.Prod_Descricao ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_Preco", produto.Prod_Preco),
                        new SqlParameter("@Prod_Estoque", produto.Prod_Estoque),
                        new SqlParameter("@Prod_EstoqueMinimo", produto.Prod_EstoqueMinimo),
                        new SqlParameter("@Prod_UnidadeMedida", produto.Prod_UnidadeMedida ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_EAN", produto.Prod_EAN ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_NCM", produto.Prod_NCM ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_CFOP", produto.Prod_CFOP ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_ImagemUrl", produto.Prod_ImagemUrl ?? (object)DBNull.Value),
                        new SqlParameter("@Cat_Id", produto.Cat_Id.HasValue ? (object)produto.Cat_Id.Value : DBNull.Value),
                        new SqlParameter("@Prod_Status", produto.Prod_Status),
                        new SqlParameter("@Prod_Marca", produto.Prod_Marca ?? (object)DBNull.Value),
                        new SqlParameter("@Prod_Tamanho", produto.Prod_Tamanho ?? (object)DBNull.Value)
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_ProdutoSalvar @Prod_Id, @Loj_Id, @Prod_Nome, @Prod_Descricao, @Prod_Preco, @Prod_Estoque, @Prod_EstoqueMinimo, @Prod_UnidadeMedida, @Prod_EAN, @Prod_NCM, @Prod_CFOP, @Prod_ImagemUrl, @Cat_Id, @Prod_Status, @Prod_Marca, @Prod_Tamanho",
                        parametros);

                    TempData["MensagemSucesso"] = "Produto atualizado com sucesso!";
                    // return RedirectToAction("ListarProdutos"); // Retorna à listagem
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao atualizar produto: {ex.Message}");
                    ModelState.AddModelError("", "Erro ao atualizar o produto. Por favor, tente novamente.");
                }
            }

            // Recarregar categorias para o dropdown, caso ocorra erro
            var lojIdParam = new SqlParameter("@Loj_Id", lojId.Value);
            var categorias = await _context.Categorias
                .FromSqlRaw("EXEC sp_GetCategoriasAtivasPorLoja @Loj_Id", lojIdParam)
                .ToListAsync();
            ViewBag.Categorias = categorias;

            return View(produto);
        }

        // GET: Produtos/CadastrarLote
        [HttpGet]
        public IActionResult CadastrarLote(int? produtoId)
        {
            if (produtoId.HasValue)
            {
                // Buscar informações do produto existente, se necessário
                var produto = _context.Produtos.FirstOrDefault(p => p.Prod_Id == produtoId.Value);

                if (produto == null)
                {
                    return NotFound("Produto não encontrado.");
                }

                // Passar dados do produto para a View
                return View(new LoteViewModel
                {
                    ProdutoId = produto.Prod_Id,
                    NomeProduto = produto.Prod_Nome
                });
            }

            // Se não for vinculado a um produto existente, abrir o formulário vazio
            return View(new LoteViewModel());
        }

        [HttpPost]
        public IActionResult SalvarLote(LoteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine($"Erro na chave {modelStateKey}: {error.ErrorMessage}");
                    }
                }

                return View("CadastrarLote", model);
            }

            try
            {
                // Chamada da stored procedure
                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_SalvarLote @ProdutoId, @NumeroLote, @Quantidade, @Validade, @DataCadastro",
                    new SqlParameter("@ProdutoId", model.ProdutoId),
                    new SqlParameter("@NumeroLote", model.NumeroLote),
                    new SqlParameter("@Quantidade", model.Quantidade),
                    new SqlParameter("@Validade", model.Validade ?? (object)DBNull.Value),
                    new SqlParameter("@DataCadastro", DateTime.Now)
                );

                TempData["MensagemSucesso"] = "Lote cadastrado e estoque atualizado com sucesso!";
                return View("CadastrarLote", new LoteViewModel());

            }
            catch (Exception ex)
            {
                // Logar o erro e retornar a view com mensagem de erro
                TempData["MensagemErro"] = $"Erro ao salvar lote: {ex.Message}";
                return View("CadastrarLote", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> LotesProximosValidade(int diasAntes = 60)

        {
            try
            {
                // Parâmetro da Procedure
                var parametro = new SqlParameter("@DiasAntes", diasAntes);

                // Execução da Procedure e retorno em uma lista
                var lotes = await _context.LotesProximosValidadeViewModel
                    .FromSqlRaw("EXEC sp_LotesProximosValidade @DiasAntes", parametro)
                    .ToListAsync();

                // Convertendo Validade de string para DateTime
                foreach (var lote in lotes)
                {
                    if (DateTime.TryParse(lote.Validade, out DateTime dataValidade))
                    {
                        lote.Validade = dataValidade.ToString("dd-MM-yyyy");
                    }
                }

                // Retornar para a View
                return View("LotesProximosValidade", lotes);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao buscar lotes próximos à validade: {ex.Message}";
                return RedirectToAction("Index", "Produtos");
            }
        }

        [HttpPost]
        public IActionResult ExportarRelatorioLotes()
        {
            // Buscar os dados
            var parametro = new SqlParameter("@DiasAntes", 30);
            var lotes = _context.LotesProximosValidadeViewModel
                .FromSqlRaw("EXEC sp_LotesProximosValidade @DiasAntes", parametro)
                .ToList();

            // Convertendo Validade de string para DateTime
            foreach (var lote in lotes)
            {
                if (DateTime.TryParse(lote.Validade, out DateTime dataValidade))
                {
                    lote.Validade = dataValidade.ToString("dd/MM/yyyy");
                }
            }

            // Configurar PDF
            MemoryStream memoryStream = new MemoryStream();
            Document document = new Document();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            // Adicionar título
            document.Add(new Paragraph("Relatório de Lotes Próximos à Validade"));
            document.Add(new Paragraph(" "));

            // Adicionar tabela
            PdfPTable table = new PdfPTable(4);
            table.AddCell("Produto");
            table.AddCell("Lote");
            table.AddCell("Quantidade");
            table.AddCell("Validade");
           

            foreach (var lote in lotes)
            {
                table.AddCell(lote.ProdutoNome);
                table.AddCell(lote.NumeroLote);
                table.AddCell(lote.Quantidade.ToString());
                table.AddCell(lote.Validade);
            }

            document.Add(table);
            document.Close();

            // Retornar arquivo PDF
            return File(memoryStream.ToArray(), "application/pdf", "RelatorioLotes.pdf");
        }

        public async Task<IActionResult> MovimentacoesEstoque(string dataInicio, string dataFim, string tipoMovimentacao)
        {
            // Converte as datas recebidas
            DateTime? dataInicioParsed = string.IsNullOrEmpty(dataInicio) ? (DateTime?)null : DateTime.Parse(dataInicio);
            DateTime? dataFimParsed = string.IsNullOrEmpty(dataFim) ? (DateTime?)null : DateTime.Parse(dataFim);

            // Define os parâmetros
            var parametros = new[]
            {
                new SqlParameter("@DataInicio", dataInicioParsed ?? (object)DBNull.Value),
                new SqlParameter("@DataFim", dataFimParsed ?? (object)DBNull.Value),
                new SqlParameter("@TipoMovimentacao", tipoMovimentacao ?? (object)DBNull.Value)
            };

            // Executa a procedure
            var movimentacoes = await _context.Set<MovimentacaoEstoqueViewModel>()
                .FromSqlRaw("EXEC sp_RelatorioMovimentacoesEstoque @DataInicio, @DataFim, @TipoMovimentacao", parametros)
                .ToListAsync();

            return View(movimentacoes);
        }


        [HttpGet]
        public async Task<IActionResult> ExportarRelatorioMovimentacaoEstoque(string dataInicio, string dataFim, string tipoMovimentacao)
        {
            // Converter os parâmetros
            DateTime? dataInicioParsed = string.IsNullOrEmpty(dataInicio) ? (DateTime?)null : DateTime.Parse(dataInicio);
            DateTime? dataFimParsed = string.IsNullOrEmpty(dataFim) ? (DateTime?)null : DateTime.Parse(dataFim);

            // Definir parâmetros para a procedure
            var parametros = new[]
            {
                new SqlParameter("@DataInicio", dataInicioParsed ?? (object)DBNull.Value),
                new SqlParameter("@DataFim", dataFimParsed ?? (object)DBNull.Value),
                new SqlParameter("@TipoMovimentacao", tipoMovimentacao ?? (object)DBNull.Value)
            };

            // Obter os dados
            var movimentacoes = await _context.Set<MovimentacaoEstoqueViewModel>()
                .FromSqlRaw("EXEC sp_RelatorioMovimentacoesEstoque @DataInicio, @DataFim, @TipoMovimentacao", parametros)
                .ToListAsync();

            // Configurar o PDF
            Document document = new Document();
            MemoryStream stream = new MemoryStream();
            PdfWriter.GetInstance(document, stream);
            document.Open();

            // Título
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            document.Add(new Paragraph("Relatório de Movimentações de Estoque", titleFont));
            document.Add(new Paragraph($"Data Início: {dataInicioParsed:dd/MM/yyyy} | Data Fim: {dataFimParsed:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph($"Tipo de Movimentação: {tipoMovimentacao ?? "Todos"}", normalFont));
            document.Add(new Paragraph(" ")); // Espaço

            // Criar a tabela
            PdfPTable table = new PdfPTable(5); // Número de colunas
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 2, 3, 2, 1 }); // Largura relativa das colunas

            // Cabeçalhos
            table.AddCell(new PdfPCell(new Phrase("ID", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Data", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Produto", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Tipo", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Quantidade", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });

            // Dados
            foreach (var mov in movimentacoes)
            {
                table.AddCell(new PdfPCell(new Phrase(mov.Mov_Id.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(mov.Mov_DataMov.ToString("dd/MM/yyyy"), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(mov.Prod_Nome, normalFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                table.AddCell(new PdfPCell(new Phrase(mov.Mov_TipoMov, normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(mov.Mov_Quantidade.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            // Adicionar tabela ao documento
            document.Add(table);
            document.Close();

            // Retornar o PDF
            return File(stream.ToArray(), "application/pdf", "Relatorio_Movimentacoes_Estoque.pdf");
        }

        public async Task<IActionResult> ProdutosProximosValidade(int diasParaValidade = 30, string categoria = null)
        {
            var parametros = new[]
            {
                new SqlParameter("@DiasParaValidade", diasParaValidade),
                new SqlParameter("@Categoria", categoria ?? (object)DBNull.Value)
            };

            var produtos = await _context.Set<ProdutosProximosValidadeViewModel>()
                .FromSqlRaw("EXEC sp_ProdutosProximosValidade @DiasParaValidade, @Categoria", parametros)
                .ToListAsync();

            return View(produtos);
        }

        public async Task<IActionResult> ExportarRelatorioProdutosProximoVencimento(int diasParaValidade = 30, string categoria = null)
        {
            var parametros = new[]
            {
            new SqlParameter("@DiasParaValidade", diasParaValidade),
            new SqlParameter("@Categoria", string.IsNullOrEmpty(categoria) ? (object)DBNull.Value : categoria)
        };

            var produtos = await _context.Set<ProdutosProximosValidadeViewModel>()
                .FromSqlRaw("EXEC sp_ProdutosProximosValidade @DiasParaValidade, @Categoria", parametros)
                .ToListAsync();

            if (!produtos.Any())
            {
                TempData["Mensagem"] = "Nenhum produto encontrado para os critérios selecionados.";
                return RedirectToAction("ProdutosProximosValidade");
            }

            // Configurar a geração de PDF
            using var pdf = new MemoryStream();
            var doc = new Document();
            PdfWriter.GetInstance(doc, pdf);
            doc.Open();

            doc.Add(new Paragraph("Relatório de Produtos Próximos à Validade"));
            doc.Add(new Paragraph($"Filtros: Dias para Validade - {diasParaValidade}, Categoria - {categoria ?? "Todos"}"));
            doc.Add(new Paragraph(" ")); // Espaço em branco

            var table = new PdfPTable(5)
            {
                WidthPercentage = 100
            };

            table.AddCell("ID");
            table.AddCell("Nome");
            table.AddCell("Categoria");
            table.AddCell("Data de Validade");
            table.AddCell("Quantidade");

            foreach (var produto in produtos)
            {
                table.AddCell(produto.Prod_Id.ToString());
                table.AddCell(produto.Prod_Nome);
                table.AddCell(produto.Categoria);
                table.AddCell(produto.Lote_DataValidade.ToString("dd/MM/yyyy"));
                table.AddCell(produto.Lote_Quantidade.ToString());
            }

            doc.Add(table);
            doc.Close();

            return File(pdf.ToArray(), "application/pdf", "Relatorio_Produtos_Proximos_Validade.pdf");
        }



        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.Prod_Id == id);
        }
    }
}