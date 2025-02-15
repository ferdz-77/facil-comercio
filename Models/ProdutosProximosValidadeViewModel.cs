using System.Collections.Generic;
using FacilComercio.Models;  // Garanta que o namespace da sua classe Produto está correto

namespace FacilComercio.Models
{
    public class ProdutosProximosValidadeViewModel
    {
        public int Prod_Id { get; set; }
        public string Prod_Nome { get; set; }
        public string Categoria { get; set; }
        public string Lote_Numero { get; set; }
        public DateTime Lote_DataValidade { get; set; }
        public int Lote_Quantidade { get; set; }
    }

}
