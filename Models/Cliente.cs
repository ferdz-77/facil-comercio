namespace FacilComercio.Models
{
    public class Cliente
    {
        public int Cli_Id { get; set; } // PK Cliente

        public int? Emp_Id { get; set; } // FK opcional para Empresa

        public string Cli_Nome { get; set; }

        public string Cli_Email { get; set; }

        public string Cli_Telefone { get; set; }

        public string Cli_Endereco { get; set; }

        public DateTime Cli_CriadoEm { get; set; } = DateTime.Now;

        public DateTime? Cli_AtualizadoEm { get; set; }
        public string Cli_CPF { get; set; }   // CPF do cliente
        public bool Cli_Status { get; set; }  // Status (Ativo/Inativo)

        // Propriedade de navegação
        public Empresa Empresa { get; set; }
    }
}
