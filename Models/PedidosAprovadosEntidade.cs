using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aprovacao_Pedidos.Models
{
    [Table("PEDIDOS_APROVADOS")]
    public class PedidosAprovadosEntidade
    {
        [Key]
        public int ID { get; set; }
        public int AprovacaoID { get; set; }
        public bool AprovadoComercial { get; set; }
        public bool AprovadoDiretoria { get; set ; }
        public bool ReprovadoComercial { get; set; }
        public bool ReprovadoDiretoria { get; set ; }
        public string? Observacao { get; set; }
    }
}
