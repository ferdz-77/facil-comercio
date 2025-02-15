using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FacilComercio.Models;
using Microsoft.Extensions.Logging;
using WebApplication1.Data; // Ajuste para o namespace correto
using WebApplication1.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using System.Data;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
//using FacilComercio.Models.FacilComercio.Models;


namespace WebApplication1.Controllers
{
    public class VendasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IVendaService _vendaService; // O serviço que você está usando para manipular vendas
        private readonly ILogger<VendasController> _logger; // O logger para registrar logs

        // GET: Vendas/PDV
        public IActionResult PDV()
        {
            return View();
        }

        public VendasController(AppDbContext context)
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


        private static CarrinhoVenda Carrinho = new CarrinhoVenda(); // Simulação do carrinho em memória

        [HttpPost]
        public async Task<IActionResult> AdicionarAoCarrinho(int produtoId, int quantidade)
        {
            var produto = await _context.Produtos
                .AsNoTracking()
                .Where(p => p.Prod_Id == produtoId)
                .Select(p => new Produto
                {
                    Prod_Id = p.Prod_Id,
                    Prod_Nome = p.Prod_Nome,
                    Prod_Preco = p.Prod_Preco
                })
                .FirstOrDefaultAsync();

            if (produto == null || quantidade <= 0)
            {
                return BadRequest("Produto inválido ou quantidade incorreta.");
            }

            var itemExistente = Carrinho.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (itemExistente != null)
            {
                itemExistente.Quantidade += quantidade; // Atualiza a quantidade se o item já existe
            }
            else
            {
                Carrinho.Itens.Add(new VendaItem
                {
                    ProdutoId = produto.Prod_Id,
                    NomeProduto = produto.Prod_Nome,
                    PrecoUnitario = produto.Prod_Preco,
                    Quantidade = quantidade
                });
            }

            return Json(new { sucesso = true, carrinho = Carrinho });
        }

        [HttpGet]
        public IActionResult ObterCarrinho()
        {
            return Json(Carrinho); // Retorna o carrinho atual
        }

        [HttpPost]
        public IActionResult RemoverProdutoCarrinho([FromBody] RemoverProdutoRequest request)
        {
            try
            {
                // Verificar se o produtoId foi passado corretamente
                var produtoId = request?.ProdutoId;
                if (produtoId == null)
                {
                    return BadRequest("Produto ID não informado.");
                }

                // Lógica para remover o produto do carrinho
                var item = Carrinho.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
                if (item != null)
                {
                    Carrinho.Itens.Remove(item);  // Remover o item
                    return Json(new { sucesso = true, carrinho = Carrinho });
                }
                else
                {
                    return Json(new { sucesso = false, mensagem = "Produto não encontrado no carrinho." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao remover o produto: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AlterarQuantidade([FromBody] AlterarQuantidadeRequest request)
        {
            try
            {
                var item = Carrinho.Itens.FirstOrDefault(i => i.ProdutoId == request.ProdutoId);
                if (item == null)
                {
                    return Json(new { sucesso = false, mensagem = "Produto não encontrado no carrinho." });
                }

                item.Quantidade = request.Quantidade;
              //  item.SubTotal = item.PrecoUnitario * item.Quantidade;

                return Json(new { sucesso = true, carrinho = Carrinho });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao alterar a quantidade: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AtualizarCarrinho([FromBody] CarrinhoVenda carrinho)
        {
            if (carrinho == null || !carrinho.Itens.Any())
            {
                return BadRequest(new { sucesso = false, mensagem = "Carrinho inválido ou vazio." });
            }

            try
            {
                // Atualiza o carrinho armazenado no backend
                Carrinho.Itens = carrinho.Itens;
                Carrinho.AplicarDesconto(carrinho.DescontoTotal);

                Console.WriteLine($"Desconto Total: {carrinho.DescontoTotal}");

                if (carrinho.ClienteId.HasValue)
                {
                    Carrinho.ClienteId = carrinho.ClienteId;
                }

                return Json(new { sucesso = true, mensagem = "Carrinho atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Erro ao atualizar o carrinho: " + ex.Message });
            }
        }


        [HttpGet]
        public IActionResult ObterCarrinhoAtualizado()
        {
            // Retorna o carrinho atualizado para o cliente diretamente
            return Ok(new { sucesso = true, carrinho = Carrinho });
           //return Json(new { sucesso = true, carrinho = Carrinho });

        }
        // Classe para receber a requisição
        public class AlterarQuantidadeRequest
        {
            public int ProdutoId { get; set; }
            public int Quantidade { get; set; }
        }

        [HttpPost]
        public IActionResult SalvarDesconto([FromBody] DescontoDto desconto)
        {
            // Verificar se os dados do desconto são válidos
            if (desconto == null || desconto.ProdutoId <= 0 || desconto.Desconto < 0)
            {
                return BadRequest("Dados inválidos para o desconto.");
            }

            // Encontrar o item no carrinho
            var item = Carrinho.Itens.FirstOrDefault(i => i.ProdutoId == desconto.ProdutoId);
            if (item == null)
            {
                return NotFound("Produto não encontrado no carrinho.");
            }

            // Atualiza o desconto
            item.Desconto = desconto.Desconto;

            return Json(new { sucesso = true, carrinho = Carrinho });
        }

        // POST: AplicarDescontoTotal
        [HttpPost]
        public IActionResult AplicarDescontoTotal([FromBody] DescontoRequest request)
        {
            try
            {
                // Obtém o carrinho atualizado (chama o método ObterCarrinhoAtualizado)
                var carrinhoResult = ObterCarrinhoAtualizado() as OkObjectResult;

                if (carrinhoResult == null || carrinhoResult.Value == null)
                {
                    return BadRequest("Carrinho não encontrado.");
                }

                var carrinho = ((dynamic)carrinhoResult.Value).carrinho;

                // Verificar o tipo de desconto
                decimal valorDescontoTotal = request.isPercentual
                    ? (carrinho.Total * request.desconto) / 100
                    : request.desconto;


                // Aplicar o desconto no carrinho
                carrinho.AplicarDesconto(valorDescontoTotal);

                // Retornar o carrinho atualizado com sucesso
                return Ok(new { sucesso = true, carrinho = carrinho });
            }
            catch (Exception ex)
            {
                // Caso ocorra erro, retornar status de erro
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AssociarClienteAoCarrinho([FromBody] ClienteRequest request)
        {
            try
            {
                if(request.ClienteId == 0)
                {
                    Carrinho.ClienteId = null;
                    Carrinho.ClienteNome = null;

                    return Json(new { sucesso = true, carrinho = Carrinho });
                }

                // Valida se o cliente existe
                var cliente = _context.Clientes.FirstOrDefault(c => c.Cli_Id == request.ClienteId);
                if (cliente == null)
                {
                    return BadRequest("Cliente não encontrado.");
                }

                // Atualiza o cliente no carrinho
                Carrinho.ClienteId = cliente.Cli_Id;
                Carrinho.ClienteNome = cliente.Cli_Nome;

                return Json(new { sucesso = true, carrinho = Carrinho });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao associar cliente: " + ex.Message });
            }
        }

        private int ObterIdMeioPagamento(string meioPagamento)
        {
            // Substitua isso por uma consulta real ao banco de dados, se necessário
            var meiosDePagamento = new Dictionary<string, int>
            {
                { "PIX", 1 },
                { "Débito", 2 },
                { "Crédito", 3 },
                { "Dinheiro", 4 }
            };

            return meiosDePagamento.ContainsKey(meioPagamento) ? meiosDePagamento[meioPagamento] : 0;
        }


        [HttpPost]  
        public IActionResult FinalizarVenda([FromBody] CarrinhoVenda carrinho)
        {
            if (carrinho == null || !carrinho.Itens.Any())
            {
                return Json(new { sucesso = false, mensagem = "Carrinho vazio. Adicione produtos para continuar." });
            }

            try
            {
                // Recuperar Loj_Id da ViewBag
                var lojId = HttpContext.Session.GetInt32("Loj_Id");

                if (lojId == null)
                {
                    return Json(new { sucesso = false, mensagem = "Loj_Id não encontrado." });
                }

                // Recuperar idUsuario da ViewBag
                var idUsuario = HttpContext.Session.GetInt32("idUsuario");

                if (idUsuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "idUsuario não encontrado." });
                }

                // Preparar os parâmetros para a stored procedure
                var itensVendaTable = new DataTable();
                itensVendaTable.Columns.Add("ProdutoId", typeof(int));
                itensVendaTable.Columns.Add("Quantidade", typeof(int));
                itensVendaTable.Columns.Add("PrecoUnitario", typeof(decimal));

                foreach (var item in carrinho.Itens)
                {
                    itensVendaTable.Rows.Add(item.ProdutoId, item.Quantidade, item.PrecoUnitario);
                }


                // Preparar os pagamentos
                var pagamentosTable = new DataTable();
                pagamentosTable.Columns.Add("mpa_id", typeof(int));
                pagamentosTable.Columns.Add("pve_valor", typeof(decimal));
                pagamentosTable.Columns.Add("pve_dataPagamento", typeof(DateTime));

                foreach (var pagamento in carrinho.Pagamentos)
                {
                    // Converter o meio de pagamento para o ID correspondente
                    int meioPagamentoId = ObterIdMeioPagamento(pagamento.MeioPagamento); // Função para mapear os meios de pagamento
                    pagamentosTable.Rows.Add(meioPagamentoId, pagamento.Valor, DateTime.Now);
                }

                var clienteIdParam = new SqlParameter("@Cli_Id", carrinho.ClienteId ?? (object)DBNull.Value);
                var totalParam = new SqlParameter("@Ven_Total", carrinho.Total);
                var dataParam = new SqlParameter("@Data", DateTime.Now);
                var lojIdParam = new SqlParameter("@Loj_Id", lojId.Value);
                var UsuIdParam = new SqlParameter("@Usu_Id", idUsuario.Value);
                var descontoTotalParam = new SqlParameter("@Desconto", carrinho.DescontoTotal);
                
                var itensVendaParam = new SqlParameter("@ItensVenda", SqlDbType.Structured)
                 {
                   TypeName = "TVP_ItemVenda", // O nome do tipo de parâmetro de tabela
                   Value = itensVendaTable
                };

                // NOVIDADE

                var pagamentosParam = new SqlParameter("@PagamentosVenda", SqlDbType.Structured)
                {
                    TypeName = "TVP_PagamentoVenda", // O nome do tipo de parâmetro de tabela para os pagamentos
                    Value = pagamentosTable
                };

                // FIM DA NOVIDADE


                // Parâmetro de saída para capturar o ID da venda
                var vendaIdParam = new SqlParameter("@Ven_Id", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                // Chamar a stored procedure
                // _context.Database.ExecuteSqlRaw("EXEC sp_CriarVenda @ClienteId, @Total, @Data, @ItensVenda", clienteIdParam, totalParam, dataParam, itensVendaParam);
                // Chamar a stored procedure
                _context.Database.ExecuteSqlRaw("EXEC sp_CriarVenda @Cli_Id, @Ven_Total, @Data, @Loj_Id, @Usu_Id, @Desconto, @ItensVenda,  @PagamentosVenda, @Ven_Id OUTPUT",
                    clienteIdParam, totalParam, dataParam, lojIdParam, UsuIdParam, descontoTotalParam, itensVendaParam, pagamentosParam, vendaIdParam);

                // Capturar o ID da venda gerado
                int vendaId = (int)vendaIdParam.Value;

                // Consultar os detalhes da venda (caso precise da data ou outras informações)
                var venda = _context.Vendas
                    .Where(v => v.Ven_Id == vendaId)
                    .Select(v => new { v.Ven_Id, v.Ven_Data, v.Total })
                    .FirstOrDefault();

               

                // DADOS PARA O CUMPOM

                // Consultar o nome e o endereço da loja no banco
                var loja = _context.Lojas
                    .Where(l => l.Loj_Id == lojId)
                    .Select(l => new { l.Loj_Nome, l.Loj_Endereco })
                    .FirstOrDefault();

                if (loja == null)
                {
                    throw new Exception("Loja não encontrada no banco de dados.");
                }

                var itensVenda = _context.ItemVenda
                .Where(iv => iv.VendaId == venda.Ven_Id)
                .Select(iv => new
                {
                    ProdutoNome = iv.Produto.Prod_Nome,  // Nome do Produto
                    Quantidade = iv.Quantidade,     // Quantidade
                    PrecoUnitario = iv.PrecoUnitario, // Preço Unitário
                    SubTotal = iv.Quantidade * iv.PrecoUnitario // Subtotal
                }).ToList();

                var meiosPagamento = new Dictionary<int, string>
                {
                    { 1, "PIX" },
                    { 2, "CREDITO" },
                    { 3, "DEBITO" },
                    { 4, "DEINHEIRO" }
                };

                var pagamentos = _context.PagamentosVenda
                    .Where(pv => pv.Ven_Id == venda.Ven_Id) // Filtrar pela venda
                    .GroupBy(pv => pv.Mpa_Id)               // Agrupar pelo ID do meio de pagamento
                    .Select(g => new
                    {
                        MeioPagamento = meiosPagamento[g.Key], // Mapeia o ID para o nome
                        Valor = g.Sum(pv => pv.Pve_Valor)      // Soma dos valores
                    })
                    .ToDictionary(p => p.MeioPagamento, p => p.Valor);




                string nomeLoja = loja.Loj_Nome;
                string enderecoLoja = loja.Loj_Endereco;
                string numeroPedido = $"NRO: {venda.Ven_Id:D8} - {venda.Ven_Data:dd/MM/yyyy HH:mm}";

                string nomeOperador = ViewBag.nomeUsuario;

                List<string> itens = new List<string>();

                int contador = 1;
                foreach (var item in itensVenda)
                {
                    itens.Add($"{contador:D3} - {item.ProdutoNome}");
                    itens.Add($"{item.Quantidade} unid x R$ {item.PrecoUnitario:F2} = R$ {item.SubTotal:F2}");
                    contador++;
                }

                decimal totalGeral = venda.Total;

                //Dictionary<string, decimal> pagamentos = new Dictionary<string, decimal>
                //{
                //    { "PIX", 18.00m },
                //    { "CREDITO", 10.00m }
                //};

                DateTime dataHoraImpressao = DateTime.Now;

                string cupom = GerarCupom(nomeLoja, enderecoLoja, numeroPedido, nomeOperador, itens, totalGeral, pagamentos, dataHoraImpressao);
                Console.WriteLine(cupom);

                ImprimirCupom(cupom);


                // Limpar o carrinho
                Carrinho.LimparCarrinho();

                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        public string GerarCupom(string nomeLoja, string enderecoLoja, string numeroPedido, string nomeOperador, List<string> itens, decimal totalGeral, Dictionary<string, decimal> pagamentos, DateTime dataHoraImpressao)
        {
            int larguraCupom = 40; // Largura máxima do cupom em caracteres

            // Centralizar títulos
            string CentralizarTexto(string texto)
            {
                int espacos = (larguraCupom - texto.Length) / 2;
                return new string(' ', espacos) + texto;
            }

            string linhaTitulo = CentralizarTexto("VENDA");
            string linhaOperacao = CentralizarTexto("== TOTALIZACAO DA OPERACAO ==");
            string linhaModalidades = CentralizarTexto("== MODALIDADES PAGAMENTO ==");

            // Tracejados
            string tracejadoDuplo = new string('=', larguraCupom);
            string tracejadoSimples = new string('-', larguraCupom);

            // Construir o cupom
            StringBuilder cupom = new StringBuilder();
            cupom.AppendLine(tracejadoDuplo);

            cupom.AppendLine(nomeLoja);
            cupom.AppendLine(enderecoLoja);

            cupom.AppendLine(tracejadoDuplo);

            cupom.AppendLine(linhaTitulo);

            cupom.AppendLine(tracejadoDuplo);

            cupom.AppendLine(numeroPedido);

            cupom.AppendLine(tracejadoDuplo);

            cupom.AppendLine("Operador: " + nomeOperador);

            cupom.AppendLine(tracejadoDuplo);

            cupom.AppendLine("Itens");
            
            foreach (var item in itens)
            {
                cupom.AppendLine(item);
            }

            cupom.AppendLine(linhaOperacao);

            cupom.AppendLine($"Total Geral ..........: R$ {totalGeral:F2}");
            int totalItens = itens.Count;
            //cupom.AppendLine($"Itens: {totalItens} | Volumes: {totalItens}"); // Ajuste conforme a lógica do seu sistema

            cupom.AppendLine(tracejadoSimples);

            cupom.AppendLine(linhaModalidades);
            foreach (var pagamento in pagamentos)
            {
                cupom.AppendLine($"{pagamento.Key.ToUpper()} ..........: R$ {pagamento.Value:F2}");
            }

            cupom.AppendLine(tracejadoSimples);

            cupom.AppendLine($"TOTAL ..........: R$ {totalGeral:F2}");

            cupom.AppendLine(tracejadoSimples);

            cupom.AppendLine("Obrigado pela compra!");
            cupom.AppendLine($"Doc. Impresso em {dataHoraImpressao:dd/MM/yyyy} às {dataHoraImpressao:HH:mm:ss}");

            cupom.AppendLine(tracejadoDuplo);

            return cupom.ToString();
        }



        // Classe para receber a requisição
        public class ClienteRequest
        {
            public int ClienteId { get; set; }
        }

        [HttpGet("Venda/GetProdutos/{vendaId}")]
        public async Task<IActionResult> GetProdutos(int vendaId)
        {
            try
            {
                var produtos = await _context.Produtos
                    .FromSqlRaw("EXEC GetProdutosPorVenda @Ven_Id = {0}", vendaId)
                    .ToListAsync();

                if (produtos == null || produtos.Count == 0)
                {
                    return NotFound("Nenhum produto encontrado para a venda especificada.");
                }

                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno no servidor: " + ex.Message);
            }
        }

        [HttpPost]
        [Route("Venda/DevolverProdutos/{id}")]
        public async Task<IActionResult> DevolverProdutos(int id, [FromBody] List<int> produtos)
        {
            if (produtos == null || produtos.Count == 0)
            {
                return BadRequest("Nenhum produto selecionado para devolução.");
            }

            // Chamar a stored procedure para devolver os produtos
            foreach (var prodId in produtos)
            {
                var parametros = new[]
                {
                    new SqlParameter("@Ven_Id", SqlDbType.Int) { Value = id },
                    new SqlParameter("@Prod_Id", SqlDbType.Int) { Value = prodId }
                };

                await _context.Database.ExecuteSqlRawAsync("EXEC sp_DevolverProdutos @Ven_Id, @Prod_Id", parametros);
            }

            // Após devolver, podemos buscar os produtos atualizados
            var parametrosBusca = new[]
            {
                new SqlParameter("@Ven_Id", SqlDbType.Int) { Value = id }
            };

            var produtosDevolvidos = await _context.Produtos
                .FromSqlRaw("EXEC GetProdutosPorVenda @Ven_Id", parametrosBusca)
                .ToListAsync();

            return RedirectToAction("Index", "Devolucoes");

            //return View(produtosDevolvidos);  // Ou qualquer outra forma de retornar a visualização com os produtos
        }

        [HttpGet]
        public async Task<IActionResult> VendasPorPeriodo(DateTime? dataInicio, DateTime? dataFim)
        {
            if (!dataInicio.HasValue || !dataFim.HasValue)
            {
                TempData["MensagemErro"] = "Por favor, selecione um período válido.";
                return View(new List<RelatorioVendasViewModel>());
            }

            var parametroInicio = new SqlParameter("@DataInicio", dataInicio.Value);
            var parametroFim = new SqlParameter("@DataFim", dataFim.Value);

            try
            {
                var vendas = await _context.Set<RelatorioVendasViewModel>()
                    .FromSqlRaw("EXEC sp_RelatorioVendasPorPeriodo @DataInicio, @DataFim", parametroInicio, parametroFim)
                    .ToListAsync();

                return View(vendas);
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao gerar relatório: {ex.Message}";
                return View(new List<RelatorioVendasViewModel>());
            }
        }

        public async Task<IActionResult> ExportarRelatorioVendasPeriodo(DateTime dataInicio, DateTime dataFim)
        {
            // Recuperar os dados filtrados com base na procedure
            var parametroInicio = new SqlParameter("@DataInicio", dataInicio);
            var parametroFim = new SqlParameter("@DataFim", dataFim);

            var vendas = await _context.RelatorioVendasViewModel
                .FromSqlRaw("EXEC sp_RelatorioVendasPorPeriodo @DataInicio, @DataFim", parametroInicio, parametroFim)
                .ToListAsync();

            // Criar o documento PDF
            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 10, 10, 10, 10);
            var writer = PdfWriter.GetInstance(document, stream);

            document.Open();

            // Adicionar título e cabeçalho
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            document.Add(new Paragraph("Relatório de Vendas por Período", titleFont));
            document.Add(new Paragraph($"Período: {dataInicio:dd/MM/yyyy} - {dataFim:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph("\n"));

            // Adicionar a tabela
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 10, 20, 30, 20, 20 });

            table.AddCell(new PdfPCell(new Phrase("ID Venda", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Data", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Cliente", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Vendedor", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Total", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

            foreach (var venda in vendas)
            {
                table.AddCell(new PdfPCell(new Phrase(venda.Ven_Id.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(venda.Ven_Data.ToString("dd/MM/yyyy"), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(venda.Cli_Nome, normalFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                table.AddCell(new PdfPCell(new Phrase(venda.Vendedor, normalFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                table.AddCell(new PdfPCell(new Phrase(venda.Ven_Total.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            document.Add(table);
            document.Close();

            // Retornar o arquivo PDF
            return File(stream.ToArray(), "application/pdf", "Relatorio_Vendas_Periodo.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> VendasPorVendedor(DateTime? dataInicio, DateTime? dataFim, int? usuId)
        {
            // Parâmetros para a procedure
            var parametros = new[]
            {
                new SqlParameter("@DataInicio", dataInicio ?? (object)DBNull.Value),
                new SqlParameter("@DataFim", dataFim ?? (object)DBNull.Value),
                new SqlParameter("@Usu_Id", usuId ?? (object)DBNull.Value)
            };

            // Execução da procedure
            var vendas = await _context.Set<RelatorioVendasVendedorViewModel>()
                .FromSqlRaw("EXEC sp_RelatorioVendasPorVendedor @DataInicio, @DataFim, @Usu_Id", parametros)
                .ToListAsync();

            // Carregar lista de vendedores para o filtro
            var vendedores = await _context.Usuarios.Select(u => new { u.Usu_Id, u.Usu_Nome }).ToListAsync();
            ViewBag.Vendedores = vendedores;

            return View(vendas);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarRelatorioVendasVendedor(DateTime? dataInicio, DateTime? dataFim, int? usuId)
        {
            // Parâmetros para a procedure
            var parametros = new[]
            {
                new SqlParameter("@DataInicio", (object)dataInicio ?? DBNull.Value),
                new SqlParameter("@DataFim", (object)dataFim ?? DBNull.Value),
                new SqlParameter("@Usu_Id", (object)usuId ?? DBNull.Value)
            };

            // Chamada da procedure
            var relatorio = await _context.RelatorioVendasVendedorViewModel
                .FromSqlRaw("EXEC sp_RelatorioVendasPorVendedor @DataInicio, @DataFim, @Usu_Id", parametros)
                .ToListAsync();

            if (!relatorio.Any())
            {
                TempData["Mensagem"] = "Nenhum dado encontrado para os filtros selecionados.";
                return RedirectToAction("VendasPorVendedor");
            }

            // Gerar PDF (mesma lógica anterior)
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 10, 10, 20, 20);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Adicionar título e informações
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var title = new Paragraph($"Relatório de Vendas por Vendedor\n\n", fontTitle);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            var fontSubTitle = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            var periodInfo = new Paragraph($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}\n", fontSubTitle);
            if (usuId.HasValue)
            {
                var vendedor = await _context.Usuarios.FindAsync(usuId);
                periodInfo.Add($"Vendedor: {vendedor?.Usu_Nome ?? "Todos"}\n\n");
            }
            periodInfo.Alignment = Element.ALIGN_CENTER;
            document.Add(periodInfo);

            // Adicionar tabela
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 10, 20, 30, 20, 20 });

            // Cabeçalho da tabela
            table.AddCell(new PdfPCell(new Phrase("ID Venda", fontTitle)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Data", fontTitle)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Cliente", fontTitle)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Vendedor", fontTitle)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Total", fontTitle)) { HorizontalAlignment = Element.ALIGN_CENTER });

            // Dados da tabela
            var fontData = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            foreach (var item in relatorio)
            {
                table.AddCell(new PdfPCell(new Phrase(item.Ven_Id.ToString(), fontData)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.Ven_Data.ToString("dd/MM/yyyy"), fontData)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.Cli_Nome, fontData)) { HorizontalAlignment = Element.ALIGN_LEFT });
                table.AddCell(new PdfPCell(new Phrase(item.Vendedor, fontData)) { HorizontalAlignment = Element.ALIGN_LEFT });
                table.AddCell(new PdfPCell(new Phrase(item.Ven_Total.ToString("C"), fontData)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Relatorio_Vendas_Vendedor_{dataInicio:yyyyMMdd}_{dataFim:yyyyMMdd}.pdf");
        }

        private void ImprimirCupom(string cupomTexto)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = "IMPRESSORA-MULTLOJA";

            printDoc.PrintPage += (sender, e) =>
            {
                // Definir fonte e dimensões
                System.Drawing.Font font = new System.Drawing.Font("Consolas", 9);
                float larguraMaxima = e.Graphics.DpiX * 2.7f; // Convertendo 80mm para pixels com base na densidade da impressora (2.7 polegadas)
                float alturaLinha = font.GetHeight(e.Graphics);
                float x = 5; // Margem esquerda
                float y = 10; // Margem superior

                // Dividir o texto em linhas por quebra natural (\n)
                string[] linhas = cupomTexto.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string linha in linhas)
                {
                    string textoAtual = linha;
                    while (textoAtual.Length > 0)
                    {
                        // Ajustar o texto para caber na largura máxima
                        //int caracteresQueCabem = MedirTextoQueCabe(textoAtual, font, larguraMaxima, e.Graphics);
                        int caracteresQueCabem = MedirTextoQueCabe(textoAtual, font, larguraMaxima, e.Graphics); // Reduzir 7 caracteres

                        if (caracteresQueCabem <= 0)
                            break; // Evita loop infinito se o cálculo falhar

                        string linhaImpressa = textoAtual.Substring(0, caracteresQueCabem);
                        e.Graphics.DrawString(linhaImpressa, font, Brushes.Black, new PointF(x, y));
                        y += alturaLinha; // Avançar para a próxima linha
                        textoAtual = textoAtual.Substring(caracteresQueCabem); // Texto restante
                    }
                }
            };

            try
            {
                printDoc.Print();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao imprimir: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcula quantos caracteres cabem dentro da largura especificada.
        /// </summary>
        private int MedirTextoQueCabe(string texto, System.Drawing.Font font, float larguraMaxima, Graphics graphics)
        {
            for (int i = 1; i <= texto.Length; i++)
            {
                string substring = texto.Substring(0, i);
                float larguraTexto = graphics.MeasureString(substring, font).Width;
                if (larguraTexto > larguraMaxima)
                {
                    return i - 1; // Retorna o máximo de caracteres que cabem
                }
            }
            return texto.Length;
        }



        public class RemoverProdutoRequest
        {
            public int ProdutoId { get; set; }
        }

        public class DescontoDto
        {
            public int ProdutoId { get; set; }
            public decimal Desconto { get; set; }
        }


    }
}