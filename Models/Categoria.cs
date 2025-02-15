using System.ComponentModel.DataAnnotations.Schema;

namespace FacilComercio.Models
{
    public class Categoria
    {
        public int Cat_Id { get; set; }
        public int? Loj_Id { get; set; }
        public string Cat_Titulo { get; set; }
        public string Cat_Descricao { get; set; }
        public DateTime Cat_CriadoEm { get; set; }
        public bool Cat_Status { get; set; }
        public int? TotalCategorias { get; set; }  // Adicione a propriedade para o total

        // Relações
        [NotMapped]
        public Loja Loja { get; set; } // Definição do relacionamento
    }

}
