namespace FacilComercio.Models
{
    public class Pagamento
    {
        public string MeioPagamento { get; set; } = string.Empty; // PIX, Débito, Crédito, Dinheiro
        public decimal Valor { get; set; } = 0; // Valor pago com esse meio
    }
}
