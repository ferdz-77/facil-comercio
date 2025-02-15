namespace FacilComercio.Models
{
    public class UsuarioLoja
    {
        public int Usu_Id { get; set; } // Mapeia Usu_Id
        public int Loj_Id { get; set; } // Mapeia Loj_Id
        public string Permissao { get; set; } // Mapeia Permissao

        // Relações
        public Usuario Usuario { get; set; }
        public Loja Loja { get; set; }
    }
}
