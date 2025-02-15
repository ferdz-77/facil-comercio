using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using FacilComercio.Models;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

// Configura��o de cache e sess�o
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de sess�o
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configura��o de autentica��o por cookies com nome espec�fico para o sistema
builder.Services.AddAuthentication("FacilComercioAuthCookie")
    .AddCookie("FacilComercioAuthCookie", options =>
    {
        options.LoginPath = "/Account/Login";            // Caminho para a p�gina de login
        options.AccessDeniedPath = "/Account/AccessDenied"; // Caminho para acesso negado
    });


// **Registro do IHttpContextAccessor**
builder.Services.AddHttpContextAccessor();

// Registro do Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // Define o nome do cabe�alho CSRF
});

// Adiciona os servi�os necess�rios para MVC
builder.Services.AddControllersWithViews();

// Configura��o do DbContext com SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configura sess�o antes de autentica��o e autoriza��o
app.UseSession();

// Adiciona o middleware customizado para gerenciar sess�es persistentes
app.UseMiddleware<SessionMiddleware>();

// Adiciona o middleware para registrar logs
app.UseMiddleware<LoggingMiddleware>();

// Configura��es do pipeline de requisi��o HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de autentica��o e autoriza��o
app.UseAuthentication();
app.UseAuthorization();

// Configura��o de roteamento padr�o
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
