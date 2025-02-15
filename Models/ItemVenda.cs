using System.ComponentModel.DataAnnotations.Schema;

namespace FacilComercio.Models
{
    public class ItemVenda
    {
        public int Id { get; set; } // Mapeia Ite_Id
        [Column("Ven_Id")]
        public int VendaId { get; set; } // Mapeia Ven_Id
        [Column("Prod_Id")]
        public int ProdutoId { get; set; } // Mapeia Prod_Id
        [Column("Ite_Quantidade")]
        public int Quantidade { get; set; } // Mapeia Ite_Quantidade
        [Column("Ite_PrecoUnitario")]
        public decimal PrecoUnitario { get; set; } // Mapeia Ite_PrecoUnitario

        // Relações
        public Venda Venda { get; set; } // Venda associada
        public Produto Produto { get; set; } // Produto associado ao item
    }
}
