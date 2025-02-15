using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FacilComercio.Models
{
    namespace FacilComercio.Models
    {
        public class RelatorioVendasPorVendedorViewModel
        {
            public int Ven_Id { get; set; }
            public DateTime Ven_Data { get; set; }
            public string Cli_Nome { get; set; }
            public decimal Ven_Total { get; set; }
            public string Vendedor { get; set; }
        }
    }
}