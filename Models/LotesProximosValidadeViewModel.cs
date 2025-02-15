using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FacilComercio.Models
{
    public class LotesProximosValidadeViewModel
    {
        public int Lote_Id { get; set; }
        public string NumeroLote { get; set; }
        public string ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public string Validade { get; set; }
        // public int? Prod_Id { get; set; }
       // public string? Categoria { get; set; }
    }
}