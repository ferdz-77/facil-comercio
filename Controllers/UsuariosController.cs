using FacilComercio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data; // Ajuste para o namespace correto


public class UsuariosController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Usuario> _passwordHasher;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<Usuario>();
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
            // Recupera o nome do usuário da sessão
            var nomeUsuario = HttpContext.Session.GetString("Nome");
            ViewBag.nomeUsuario = nomeUsuario;

            var permissao = HttpContext.Session.GetString("Permissao");
            ViewBag.Permissao = permissao;
        }
    }

    // GET: Usuarios/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Usuarios/Create - Processa a criação do novo usuário e o vincula à loja
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string Usu_Nome, string Usu_Email, string Usu_SenhaHash, string Usu_Telefone, string Usu_CPF)
    {

        if (ModelState.IsValid)
        {
            var hashedPassword = _passwordHasher.HashPassword(null, Usu_SenhaHash);

            try
            {
               
                // Executa a procedure para salvar o usuário e retorna o Usu_Id gerado
                var idParam = new SqlParameter("@Usu_Id", DBNull.Value); // Supondo que a procedure insere um novo ID
                var nomeParam = new SqlParameter("@Usu_Nome", Usu_Nome);
                var emailParam = new SqlParameter("@Usu_Email", Usu_Email);
                var senhaHashParam = new SqlParameter("@Usu_SenhaHash", hashedPassword);
                var telefoneParam = new SqlParameter("@Usu_Telefone", Usu_Telefone);
                var cpfParam = new SqlParameter("@Usu_CPF", Usu_CPF);

                // Executa a procedure de inserção
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_UsuarioSalvar  @Usu_Id, @Usu_Nome, @Usu_Email, @Usu_SenhaHash, @Usu_Telefone, @Usu_CPF",
                    idParam, nomeParam, emailParam, senhaHashParam, telefoneParam, cpfParam);

                // Após salvar o usuário, obtém o Usu_Id para criar o vínculo em UsuarioLoja
                var novoUsuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Usu_Email == Usu_Email);

                if (novoUsuario == null)
                {
                    ModelState.AddModelError("", "Erro ao obter o ID do novo usuário.");
                    return View();
                }

                var lojaId = HttpContext.Session.GetInt32("Loj_Id");
                if (lojaId == null)
                {
                    ModelState.AddModelError("", "Erro: Loja não encontrada na sessão.");
                    return View();
                }

                
                // Salva o vínculo com a loja
                var usuIdParam = new SqlParameter("@Usu_Id", novoUsuario.Usu_Id);
                var lojIdParam = new SqlParameter("@Loj_Id", lojaId.Value);
                var permissaoParam = new SqlParameter("@Permissao", "Vendedor");

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_UsuarioLojaSalvar @Usu_Id, @Loj_Id, @Permissao",
                    usuIdParam, lojIdParam, permissaoParam);

                ViewData["MensagemSucesso"] = "Sucesso: Usuário criado e vinculado à loja!";
                //return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Erro ao criar vínculo na tabela UsuarioLoja: {dbEx.InnerException?.Message}");
                ModelState.AddModelError("", "Erro ao criar vínculo com a loja. Verifique os dados e tente novamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar usuário: {ex.Message}");
                ModelState.AddModelError("", "Erro ao salvar o usuário.");
            }
        }

        return View();
    }

    // GET: Usuarios/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    // POST: Usuarios/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Usuario usuario)
    {
        if (id != usuario.Usu_Id)
        {
            return NotFound();
        }

        ModelState.Remove("Vendas");
        ModelState.Remove("LogSistema");
        ModelState.Remove("LojasCriadas");
        ModelState.Remove("UsuarioLojas");
        ModelState.Remove("Usu_SenhaHash");
       
        if (ModelState.IsValid)
        {
            try
            {
                var usuarioExistente = await _context.Usuarios.FindAsync(id);
                usuarioExistente.Usu_Nome = usuario.Usu_Nome;
                usuarioExistente.Usu_Email = usuario.Usu_Email;
                usuarioExistente.Usu_Telefone = usuario.Usu_Telefone;
                usuarioExistente.Usu_CPF = usuario.Usu_CPF;

                if (!string.IsNullOrEmpty(usuario.Usu_SenhaHash))
                {
                    usuarioExistente.Usu_SenhaHash = _passwordHasher.HashPassword(usuario, usuario.Usu_SenhaHash);
                }

                var idParam = new SqlParameter("@Usu_Id", usuarioExistente.Usu_Id);
                var nomeParam = new SqlParameter("@Usu_Nome", usuarioExistente.Usu_Nome);
                var emailParam = new SqlParameter("@Usu_Email", usuarioExistente.Usu_Email);
                var senhaHashParam = new SqlParameter("@Usu_SenhaHash", usuarioExistente.Usu_SenhaHash ?? (object)DBNull.Value);
                var telefonelParam = new SqlParameter("@Usu_Telefone", usuarioExistente.Usu_Telefone);
                var cpfParam = new SqlParameter("@Usu_CPF", usuarioExistente.Usu_CPF);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_UsuarioSalvar @Usu_Id, @Usu_Nome, @Usu_Email, @Usu_SenhaHash, @Usu_Telefone, @Usu_CPF",
                    idParam, nomeParam, emailParam, senhaHashParam, telefonelParam, cpfParam);

                HttpContext.Session.SetString("Nome", usuarioExistente.Usu_Nome);

                ViewData["MensagemSucesso"] = "Dados atualizados com sucesso!";

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar usuário: {ex.Message}");
                ModelState.AddModelError("", "Ocorreu um erro ao salvar as alterações.");
                return View(usuario);
            }
        }
        else
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }
        return View(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int usuarioId, bool status)
    {
        try
        {
            Console.WriteLine($"ID recebido: {usuarioId}");
            Console.WriteLine($"Novo status recebido: {status}");

            // Verifica se o ID do usuário existe
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Usu_Id == usuarioId);
            if (usuario == null)
            {
                Console.WriteLine("Usuário não encontrado.");
                return NotFound("Usuário não encontrado");
            }

            // Atualiza o status do usuário
            usuario.Usu_Status = status;
            await _context.SaveChangesAsync();

            Console.WriteLine("Status atualizado com sucesso.");
            return Ok(); // Retorna status 200 se tudo estiver certo
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no ToggleStatus: {ex.Message}");
            return StatusCode(500, "Erro interno no servidor"); // Retorna um erro genérico
        }
    }

    // Ação que lista os Usuários
    public async Task<IActionResult> ListarUsuarios()
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

        // Usando a procedure e o DTO para mapear os resultados
        var usuarios = await _context.UsuarioListagemDtos
            .FromSqlRaw("EXEC sp_ListarUsuariosPorLoja @Loj_Id", lojIdParam)
            .AsNoTracking()
            .ToListAsync();

        // Passar os dados para a View
        if (usuarios == null || !usuarios.Any())
        {
            TempData["MensagemErro"] = "Nenhum usuário encontrado para essa loja.";
            return RedirectToAction("Index", "Home");  // Redirecionar para uma página inicial ou qualquer outra
        }

        ViewBag.Usuarios = usuarios;  // Passando os usuários para a view

        return View(usuarios);  // Retorna a view com os dados dos usuários
    }

}
