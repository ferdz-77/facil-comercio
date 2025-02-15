namespace FacilComercio.Models
{
    public class PagamentosVenda
    {
        public int Id { get; set; }          // Mapeia Pve_Id
        public int Ven_Id { get; set; }      // Mapeia Ven_Id
        public int Mpa_Id { get; set; }      // Mapeia mpa_id
        public decimal Pve_Valor { get; set; }  // Mapeia pve_valor
        public DateTime Pve_DataPagamento { get; set; } // Mapeia pve_dataPagamento

        // Relações
        public Venda Venda { get; set; }     // Venda associada
                                             // Relação
        public MeioPagamento MeioPagamento { get; set; }
    }

    public class MeioPagamento
    {
        public int Id { get; set; }
        public string Nome { get; set; } // Nome do meio de pagamento, ex: "PIX", "CREDITO"
    }

}
