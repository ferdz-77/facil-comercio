namespace FacilComercio.Models
{
    public class Usuario
    {
        public int Usu_Id { get; set; } // Mapeia Usu_Id
        public string Usu_Nome { get; set; } // Mapeia Usu_Nome
        public string Usu_Email { get; set; } // Mapeia Usu_Email
        public string Usu_SenhaHash { get; set; } // Mapeia Usu_SenhaHash
        public string Usu_Telefone { get; set; } // Mapeia Usu_SenhaHash
        public string Usu_CPF { get; set; } // Mapeia Usu_SenhaHash
        public bool Usu_Status { get; set; }
        //public string Usu_Foto { get; set; } // Mapeia Usu_SenhaHash
        // Relações
        public ICollection<Loja> LojasCriadas { get; set; } // Lojas criadas pelo usuário
        public ICollection<UsuarioLoja> UsuarioLojas { get; set; } // Relacionamento com lojas que o usuário possui
        public ICollection<LogSistema> LogSistema { get; set; } // Logs associados ao usuário
        public ICollection<Venda> Vendas { get; set; } // Vendas realizadas pelo usuário
    }
}
