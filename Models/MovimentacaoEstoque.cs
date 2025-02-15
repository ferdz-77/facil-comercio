namespace FacilComercio.Models
{
    public class MovimentacaoEstoque
    {
        public int Id { get; set; } // Mapeia Mov_Id
        public int ProdutoId { get; set; } // Mapeia Prod_Id
        public int Loj_Id { get; set; } // Mapeia Loj_Id
        public int Quantidade { get; set; } // Mapeia Mov_Quantidade
        public string TipoMov { get; set; } // Mapeia Mov_TipoMov ("Entrada" ou "Saída")
        public DateTime DataMov { get; set; } = DateTime.Now; // Mapeia Mov_DataMov

        // Relações
        public Produto Produto { get; set; } // Produto associado à movimentação
        public Loja Loja { get; set; } // Loja associada à movimentação
    }
}
