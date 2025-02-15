using WebApplication1.Models;

namespace FacilComercio.Models
{
    public class DescontoRequest
    {
        public int venId { get; set; }
        public decimal desconto { get; set; }
        public bool isPercentual { get; set; }
    }

}
