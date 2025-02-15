using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Executa a próxima etapa do pipeline
            await _next(context);

            // Log de sucesso
            await RegistrarLog(context, "SUCCESS", "INFO", "Requisição bem-sucedida.");
        }
        catch (Exception ex)
        {
            // Log de erro
            await RegistrarLog(context, "ERROR", "ERROR", ex.Message);
            throw; // Propaga o erro após o log
        }
    }

    private async Task RegistrarLog(HttpContext context, string resultado, string nivel, string detalhes)
    {

        // Ignorar requisições desnecessárias
        if (context.Request.Method == "GET" &&
            (context.Request.Path.ToString().Equals("/") || context.Request.Path.ToString().Contains("/Account/Login")))
        {
            return;
        }

        try
        {
            // Resolve o AppDbContext dentro do escopo da requisição
            using var scope = context.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var usuarioId = context.Session.GetInt32("idUsuario") ?? 0;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";
            var path = context.Request.Path.ToString(); // Converte PathString para string

            var parameters = new[]
            {
                new SqlParameter("@DataHora", DateTime.Now),
                new SqlParameter("@Usu_Id", usuarioId == 0 ? (object)DBNull.Value : usuarioId),
                new SqlParameter("@Acao", $"{context.Request.Method} {path}"),
                new SqlParameter("@Entidade", nivel),
                new SqlParameter("@EntidadeId", DBNull.Value),
                new SqlParameter("@Detalhes", detalhes ?? (object)DBNull.Value),
                new SqlParameter("@IP", ip),
                new SqlParameter("@Resultado", resultado)
            };

            await using var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "EXEC sp_LogSistemaRegistrar @Usu_Id, @Acao, @Entidade, @EntidadeId, @Detalhes, @IP, @Resultado";
            command.Parameters.AddRange(parameters);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar log: {ex.Message}");
        }
    }
}