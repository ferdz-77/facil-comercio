using FacilComercio.Models;
using System.Threading.Tasks;
using WebApplication1.Data;  // Namespace para o AppDbContext
using WebApplication1.Models;  // Namespace para o modelo Venda (ajuste conforme necessário)
using WebApplication1.Services;  // Namespace para a interface IVendaService

namespace WebApplication1.Services
{
    public class VendaService : IVendaService
    {
        private readonly AppDbContext _context;  // Contexto do banco de dados

        // Construtor com injeção de dependência
        public VendaService(AppDbContext context)
        {
            _context = context;
        }

        // Implementação do método para obter uma venda por ID
        public async Task<Venda> ObterVendaPorId(int venId)
        {
            return await _context.Vendas.FindAsync(venId);  // Pesquisa no banco de dados pela ID da venda
        }

        // Implementação do método para atualizar uma venda
        public async Task AtualizarVenda(Venda venda)
        {
            _context.Update(venda);  // Marca a venda para atualização
            await _context.SaveChangesAsync();  // Salva as alterações no banco de dados
        }

        // Exemplo de método adicional para adicionar uma venda
        public async Task AdicionarVenda(Venda venda)
        {
            _context.Vendas.Add(venda);  // Adiciona uma nova venda ao banco de dados
            await _context.SaveChangesAsync();  // Salva as alterações no banco
        }

        // Outros métodos relacionados à lógica de vendas, se necessário
    }
}
