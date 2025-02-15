using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FacilComercio.Models
{
    public class MovimentacaoEstoqueViewModel
    {
        public int Mov_Id { get; set; }
        public DateTime Mov_DataMov { get; set; }
        public string Prod_Nome { get; set; }
        public string Mov_TipoMov { get; set; }
        public int Mov_Quantidade { get; set; }
    }
}
