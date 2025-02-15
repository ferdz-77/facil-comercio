namespace FacilComercio.Models
{
    public class VendaItem
    {
        public int Id { get; set; } // Chave primária
        public int VendaId { get; set; } // ID da venda (chave estrangeira)
        public int ProdutoId { get; set; } // ID do produto
        public string NomeProduto { get; set; } // Nome do produto
        public decimal PrecoUnitario { get; set; } // Preço unitário
        public int Quantidade { get; set; } // Quantidade adicionada
        public decimal Desconto { get; set; } // Para armazenar o desconto
        public decimal SubTotal => (PrecoUnitario * Quantidade) - Desconto; // Calculado dinamicamente

    }
}
