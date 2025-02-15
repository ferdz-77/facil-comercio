using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FacilComercio.Models
{
    public class LoteViewModel
    {
        public int? ProdutoId { get; set; }
        public string? NomeProduto { get; set; } // Apenas para exibição
        public string NumeroLote { get; set; }
        public int Quantidade { get; set; }
        public DateTime? Validade { get; set; }
    }
}
