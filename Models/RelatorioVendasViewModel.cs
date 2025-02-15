using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FacilComercio.Models
{
    public class RelatorioVendasViewModel
    {
        public int Ven_Id { get; set; }         // ID da venda
        public DateTime Ven_Data { get; set; } // Data da venda
        public string? Cli_Nome { get; set; }   // Nome do cliente
        public string Vendedor { get; set; }   // Nome do vendedor
        public decimal Ven_Total { get; set; } // Valor total da venda
    }

}
