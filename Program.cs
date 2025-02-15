using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using FacilComercio.Models;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração de cache e sessão
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuração de autenticação por cookies com nome específico para o sistema
builder.Services.AddAuthentication("FacilComercioAuthCookie")
    .AddCookie("FacilComercioAuthCookie", options =>
    {
        options.LoginPath = "/Account/Login";            // Caminho para a página de login
        options.AccessDeniedPath = "/Account/AccessDenied"; // Caminho para acesso negado
    });


// **Registro do IHttpContextAccessor**
builder.Services.AddHttpContextAccessor();

// Registro do Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // Define o nome do cabeçalho CSRF
});

// Adiciona os serviços necessários para MVC
builder.Services.AddControllersWithViews();

// Configuração do DbContext com SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configura sessão antes de autenticação e autorização
app.UseSession();

// Adiciona o middleware customizado para gerenciar sessões persistentes
app.UseMiddleware<SessionMiddleware>();

// Adiciona o middleware para registrar logs
app.UseMiddleware<LoggingMiddleware>();

// Configurações do pipeline de requisição HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Configuração de roteamento padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
