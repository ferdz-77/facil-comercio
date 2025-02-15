using FacilComercio.Models;
using System.Threading.Tasks;
using WebApplication1.Models;  // Para o modelo Venda

namespace WebApplication1.Services
{
    public interface IVendaService
    {
        Task<Venda> ObterVendaPorId(int venId);
        Task AtualizarVenda(Venda venda);
        Task AdicionarVenda(Venda venda);  // Caso necessário
    }
}
