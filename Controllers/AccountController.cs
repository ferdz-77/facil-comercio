using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FacilComercio.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System;
using WebApplication1.Data; // Ajuste para o namespace correto


namespace FacilComercio.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AccountController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        // Exibe a View de Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {


            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Usu_Email == email);
            
            string logResultado = "Falha";

            if (usuario != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.Usu_SenhaHash, password);

                if (result == PasswordVerificationResult.Success)
                {
                    // Carregar informações adicionais do usuário usando uma procedure
                    var usuarioIdParam = new SqlParameter("@UsuarioId", usuario.Usu_Id);

                    try
                    {
                        var usuarioLoja = _context.UsuarioLojas
                            .FromSqlRaw("EXEC sp_GetUsuarioLoja @UsuarioId", usuarioIdParam)
                            .AsEnumerable()
                            .FirstOrDefault();

                        if (usuarioLoja != null)
                        {
                            // Armazena as informações do usuário na sessão
                            HttpContext.Session.SetString("Nome", usuario.Usu_Nome);
                            HttpContext.Session.SetInt32("Loj_Id", usuarioLoja.Loj_Id);
                            HttpContext.Session.SetString("Permissao", usuarioLoja.Permissao);
                            HttpContext.Session.SetInt32("idUsuario", usuario.Usu_Id);


                            // Obtém o Emp_Id associado à loja ou ao usuário (exemplo)
                            var empId = _context.Lojas
                                .Where(l => l.Loj_Id == usuarioLoja.Loj_Id)
                                .Select(l => l.Loj_EmpresaId)
                                .FirstOrDefault();

                            if (empId > 0) // Verifica se empId é maior que zero (válido)
                            {
                                HttpContext.Session.SetInt32("Emp_Id", empId); // Armazena o Emp_Id na sessão
                            }

                            // Define o ViewBag para a permissão 
                            ViewBag.Permissao = usuarioLoja.Permissao;
                            ViewBag.nomeUsuario = usuario.Usu_Nome;
                            ViewBag.Loj_Id = usuarioLoja.Loj_Id;
                            ViewBag.Emp_Id = empId;

                            // Cria as declarações para o usuário autenticado
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, usuario.Usu_Nome),
                                new Claim(ClaimTypes.Email, usuario.Usu_Email),
                                new Claim("Loj_Id", usuarioLoja.Loj_Id.ToString()),
                                new Claim("Permissao", usuarioLoja.Permissao)
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            // Cria o cookie de autenticação
                            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                            // Cria o cookie de autenticação
                            await HttpContext.SignInAsync("FacilComercioAuthCookie", new ClaimsPrincipal(claimsIdentity));


                            logResultado = "Sucesso";
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ViewData["MensagemErro"] = $"Nenhum registro encontrado em UsuarioLoja para Usuário ID: {usuario.Usu_Id}";
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["MensagemErro"] = $"Erro ao carregar UsuarioLoja: {ex.Message}";
                    }
                }
                else
                {
                    logResultado = "Senha incorreta";
                    ViewData["MensagemErro"] = "Usuário e/ou senha inválidos";
                }
            }
            else
            {
                logResultado = "Usuário não encontrado";
                ViewData["MensagemErro"] = "Usuário e/ou senha inválidos";
            }

            // Exibe a mensagem de erro caso o login falhe
            ModelState.AddModelError("", "Email ou senha inválidos.");
            return View();
        }

        // Método para logout
        public async Task<IActionResult> Logout()
        {
            // Limpa as informações da sessão
            HttpContext.Session.Remove("Nome");
            HttpContext.Session.Remove("Loj_Id");
            HttpContext.Session.Remove("Permissao");

            // Remove o cookie de autenticação com o esquema correto
            await HttpContext.SignOutAsync("FacilComercioAuthCookie");

            // Redireciona para a página de login
            return RedirectToAction("Login", "Account");
        }


        // Ação para exibir o formulário de registro
        public IActionResult Register()
        {
            return View();
        }

        // Ação para processar o registro
        [HttpPost]
        public async Task<IActionResult> Register(string nome, string email, string password)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Usuario
                {
                    Usu_Nome = nome,
                    Usu_Email = email,
                    Usu_SenhaHash = _passwordHasher.HashPassword(null, password)
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View();
        }
    }
}
