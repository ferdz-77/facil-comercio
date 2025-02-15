namespace FacilComercio.Models.DTOs
{
    public class UsuarioListagemDto
    {
        public int Usu_Id { get; set; }
        public string Usu_Nome { get; set; }
        public string Usu_Email { get; set; }
        public string? Usu_Telefone { get; set; }
        public string? Usu_CPF { get; set; }
        public bool Usu_Status { get; set; } // Indica se o vendedor está ativo ou inativo
    }
}
