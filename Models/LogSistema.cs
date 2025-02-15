namespace FacilComercio.Models
{
    public class LogSistema
    {
        public int Id { get; set; } // Mapeia Log_Id
        public DateTime DataHora { get; set; } = DateTime.Now; // Mapeia DataHora
        public int? Usu_Id { get; set; } // Mapeia UsuarioId
        public string Acao { get; set; } // Mapeia Acao
        public string Entidade { get; set; } // Mapeia Entidade
        public int? EntidadeId { get; set; } // Mapeia EntidadeId
        public string Detalhes { get; set; } // Mapeia Detalhes
        public string IP { get; set; } // Mapeia IP
        public string Resultado { get; set; } // Mapeia Resultado

        // Relação
        public Usuario Usuario { get; set; } // Usuário associado
    }
}
