using FacilComercio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data; // Ajuste para o namespace correto

public class ClientesController : Controller
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
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

    // GET: Clientes/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Clientes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string Cli_Nome, string Cli_Email, string Cli_Telefone, string Cli_Endereco, string Cli_CPF)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Obtendo o Emp_Id da sessão do usuário logado
                int? empId = HttpContext.Session.GetInt32("Emp_Id");
                if (!empId.HasValue)
                {
                    // Caso o Emp_Id não esteja disponível na sessão
                    ModelState.AddModelError("", "Erro: Empresa não identificada. Por favor, faça login novamente.");
                    return View();
                }

                // Executa a procedure para salvar o cliente
                var idParam = new SqlParameter("@Cli_Id", DBNull.Value); // Supondo que a procedure insere um novo ID
                var idEmpresaParam = new SqlParameter("@Emp_Id", 1);
                var nomeParam = new SqlParameter("@Cli_Nome", Cli_Nome);
                var emailParam = new SqlParameter("@Cli_Email", Cli_Email);
                var telefoneParam = new SqlParameter("@Cli_Telefone", Cli_Telefone);
                var endrecoParam = new SqlParameter("@Cli_Endereco", Cli_Endereco);
                var cpfParam = new SqlParameter("@Cli_CPF", Cli_CPF);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_ClienteSalvar @Cli_Id, @Emp_Id, @Cli_Nome, @Cli_Email, @Cli_Telefone, @Cli_Endereco, @Cli_CPF",
                    idParam, idEmpresaParam, nomeParam, emailParam, telefoneParam, endrecoParam, cpfParam);

                ViewData["MensagemSucesso"] = "Cliente criado com sucesso!";
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Erro ao salvar cliente: {dbEx.InnerException?.Message}");
                ModelState.AddModelError("", "Erro ao salvar o cliente. Verifique os dados e tente novamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                ModelState.AddModelError("", "Erro interno ao salvar o cliente.");
            }
        }

        return View();
    }

    // GET: Clientes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }
        return View(cliente);
    }

    // POST: Clientes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Cli_Id)
        {
            return NotFound();
        }

        // Exibe erros no console para debug
        foreach (var modelState in ModelState.Values)
        {
            foreach (var error in modelState.Errors)
            {
                Console.WriteLine($"Erro no ModelState: {error.ErrorMessage}");
            }
        }

        ModelState.Remove("Empresa");

        if (ModelState.IsValid)
        {
            try
            {
                var clienteExistente = await _context.Clientes.FindAsync(id);
                clienteExistente.Cli_Nome = cliente.Cli_Nome;
                clienteExistente.Cli_Email = cliente.Cli_Email;
                clienteExistente.Cli_Telefone = cliente.Cli_Telefone;
                clienteExistente.Cli_CPF = cliente.Cli_CPF;

                var idParam = new SqlParameter("@Cli_Id", clienteExistente.Cli_Id);
                var empresaParam = new SqlParameter("@Emp_Id", DBNull.Value); // Supondo que a procedure insere um novo ID
                var nomeParam = new SqlParameter("@Cli_Nome", clienteExistente.Cli_Nome);
                var emailParam = new SqlParameter("@Cli_Email", clienteExistente.Cli_Email);
                var telefoneParam = new SqlParameter("@Cli_Telefone", clienteExistente.Cli_Telefone);
                var enderecoParam = new SqlParameter("@Cli_Endereco", clienteExistente.Cli_Endereco);
                var cpfParam = new SqlParameter("@Cli_CPF", clienteExistente.Cli_CPF);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_ClienteSalvar @Cli_Id, @Emp_Id, @Cli_Nome, @Cli_Email, @Cli_Telefone, @Cli_Endereco, @Cli_CPF",
                    idParam, empresaParam, nomeParam, emailParam, telefoneParam, enderecoParam, cpfParam);

                ViewData["MensagemSucesso"] = "Cliente atualizado com sucesso!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar cliente: {ex.Message}");
                ModelState.AddModelError("", "Erro ao salvar as alterações do cliente.");
            }
        }

        return View(cliente);
    }

    [HttpGet]
    public IActionResult ListarClientes(string busca, string filtroStatus, DateTime? dataInicio, DateTime? dataFim, int pagina = 1, int tamanhoPagina = 10)
    {
        // Filtro básico
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrEmpty(busca))
        {
            query = query.Where(c => c.Cli_Nome.Contains(busca) ||
                                     c.Cli_CPF.Contains(busca) ||
                                     c.Cli_Email.Contains(busca));
        }

        if (!string.IsNullOrEmpty(filtroStatus))
        {
            bool ativo = filtroStatus == "Ativo";
            query = query.Where(c => c.Cli_Status == ativo);
        }

        if (dataInicio.HasValue && dataFim.HasValue)
        {
            query = query.Where(c => c.Cli_CriadoEm >= dataInicio && c.Cli_CriadoEm <= dataFim);
        }

        // Paginação
        var totalClientes = query.Count();
        var clientes = query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalClientes / tamanhoPagina);

        return View(clientes);
    }

    [HttpGet]
    public async Task<IActionResult> BuscarClientePorCpf(string cpf)
    {
        if (string.IsNullOrEmpty(cpf))
        {
            return BadRequest(new { sucesso = false, mensagem = "CPF não informado." });
        }

        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Cli_CPF == cpf);

        if (cliente == null)
        {
            return Json(new { sucesso = false, mensagem = "Cliente não encontrado." });
        }

        return Json(new { sucesso = true, cliente = new { cliente.Cli_Id, cliente.Cli_Nome } });
    }


    // GET: Clientes/Inativar/5
    public async Task<IActionResult> Inativar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Cli_Id == id);
        if (cliente == null)
        {
            return NotFound();
        }

        return View(cliente); // Exibe uma página de confirmação (opcional)
    }

    // POST: Clientes/Inativar/5
    [HttpPost, ActionName("Inativar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InativarConfirmado(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            cliente.Cli_Status = false; // Marca como inativo
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Cliente inativado com sucesso!";
        }
        else
        {
            TempData["MensagemErro"] = "Erro ao inativar cliente. Cliente não encontrado.";
        }

        return RedirectToAction(nameof(ListarClientes)); // Ajuste para sua rota de listagem
    }

    [HttpPost]
    public IActionResult ToggleStatus(int clienteId, bool status)
    {
        try
        {
            // Atualiza o status no banco de dados (usando EF ou uma proc)
            var cliente = _context.Clientes.Find(clienteId);
            if (cliente == null)
            {
                return NotFound(new { success = false, message = "Cliente não encontrado." });
            }

            cliente.Cli_Status = status;
            _context.SaveChanges();

            return Json(new { success = true, newStatus = status });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }


}
