using WebApplication1.Models;

namespace FacilComercio.Models
{
    public class Loja
    {
        public int Loj_Id { get; set; } // Mapeia Loj_Id
        public int Loj_EmpresaId { get; set; } // Mapeia Loj_EmpresaId
        public string Loj_Nome { get; set; } // Mapeia Loj_Nome
        public string Loj_Endereco { get; set; } // Mapeia Loj_Endereco
        public string Loj_ContatoEm { get; set; } // Mapeia Loj_ContatoEm
        public DateTime Loj_Criado { get; set; } = DateTime.Now; // Mapeia Loj_Criado
        public DateTime? Loj_AtualizadoEm { get; set; } // Mapeia Loj_AtualizadoEm
        public int? Loj_CriadoPor { get; set; } // Mapeia Loj_CriadoPor

        // Relações
        public Empresa Empresa { get; set; } // Empresa associada
        public Usuario UsuarioCriador { get; set; } // Usuário que criou a loja
        public ICollection<UsuarioLoja> UsuarioLojas { get; set; } // Usuários associados à loja
        public ICollection<Produto> Produtos { get; set; } // Produtos da loja

        public ICollection<Venda> Vendas { get; set; } // Vendas realizadas na loja
    }
}
