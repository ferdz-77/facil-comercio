using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FacilComercio.Models
{
    public class Venda
    {
        public int Ven_Id { get; set; } // Mapeia Ven_Id
        public int Loj_Id { get; set; } // Mapeia Loj_Id
        public int? UsuarioId { get; set; } // Mapeia Usu_Id
        public DateTime Data { get; set; } = DateTime.Now; // Mapeia Ven_Data
        public DateTime? Ven_Data { get; set; }
        [Column("Ven_Total")]
        public decimal Total { get; set; } // Mapeia Ven_Total
        public decimal Desconto { get; set; } // Mapeia Ven_Desconto

        // Propriedade de navegação para produtos
        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();

        // Chave estrangeira para Cliente
        public int? CliId { get; set; }

        // Cliente relacionado (apenas usado em carregamento explícito)
        [JsonIgnore]
        public Cliente Cliente { get; set; }

        // Nome do cliente, retornado diretamente pela procedure
        public string Cli_Nome { get; set; }

        // Relações
        [JsonIgnore]
        public Loja Loja { get; set; } // Loja onde a venda foi realizada

        [JsonIgnore]
        public Usuario Usuario { get; set; } // Usuário que realizou a venda

        // Propriedade de navegação
        public List<ItemVenda> ItensVenda { get; set; } = new List<ItemVenda>();
    }
}
