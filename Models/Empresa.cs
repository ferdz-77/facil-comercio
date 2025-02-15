using System;

namespace FacilComercio.Models
{
    public class Empresa
    {
        public int Id { get; set; } // Mapeia Emp_Id
        public string CNPJ { get; set; } // Mapeia Emp_CNPJ
        public string Nome { get; set; } // Mapeia Emp_Nome
        public string Endereco { get; set; } // Mapeia Emp_Endereco
        public string Contato { get; set; } // Mapeia Emp_Contato
        public DateTime CriadoEm { get; set; } = DateTime.Now; // Mapeia Emp_CriadoEm, com valor padrão
        public DateTime? AtualizadoEm { get; set; } // Mapeia Emp_AtualizadoEm
        public int? CriadoPor { get; set; } // Mapeia Emp_CriadoPor

        // Relação com a entidade Usuario, caso precise obter detalhes do usuário que criou
        public Usuario UsuarioCriador { get; set; } // Navegação opcional para o usuário
    }
}
