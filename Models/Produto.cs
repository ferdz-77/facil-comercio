using System.ComponentModel.DataAnnotations.Schema;

namespace FacilComercio.Models
{
    public class Produto
    {
        public int Prod_Id { get; set; } // Mapeia Prod_Id
        public int Loj_Id { get; set; } // Mapeia Loj_Id
        public int? Cat_Id { get; set; }
        public string Prod_Nome { get; set; } // Mapeia Prod_Nome
        public string? Prod_Descricao { get; set; } // Permitir nulo
        public decimal Prod_Preco { get; set; } // Mapeia Prod_Preco
        public int Prod_Estoque { get; set; } // Mapeia Prod_QuantidadeEstoque
        public string? Prod_ImagemUrl { get; set; } // Permitir nulo
        public DateTime Prod_CriadoEm { get; set; } = DateTime.Now; // Mapeia Prod_CriadoEm
        public int Prod_EstoqueMinimo { get; set; } // Mapeia Prod_EstoqueMinimo
        public string Prod_UnidadeMedida { get; set; } // 
        public string Prod_EAN { get; set; } // 
        public string? Prod_NCM { get; set; } // Permite valores nulos
        public string? Prod_CFOP { get; set; } // Permite valores nulos
        public bool Prod_Status { get; set; }
        public string? Prod_Marca { get; set; } // Permite valores nulos 
        public string? Prod_Tamanho { get; set; }

        // Colunas adicionais retornadas pela procedure
        [NotMapped]
        public string? CategoriaNome { get; set; } // Nome da categoria
        [NotMapped]
        public int TotalProdutos { get; set; }    // Total de produtos

        // Relação
        public Loja? Loja { get; set; }

    }
}
