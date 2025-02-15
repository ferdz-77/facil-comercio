using System.Collections.Generic;
using FacilComercio.Models;  // Garanta que o namespace da sua classe Produto está correto

namespace FacilComercio.Models
{
    public class ListarProdutosViewModel
    {
        public List<Produto> Produtos { get; set; } // Lista de produtos
        public int TotalProdutos { get; set; } // Total de produtos, para fins de paginação
    }
}
