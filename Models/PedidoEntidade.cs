using System.ComponentModel.DataAnnotations.Schema;
using Aprovacao_Pedidos.Enumeraveis;

namespace Aprovacao_Pedidos.Models
{
    [Table("APROVACAO_TELEVENDAS")]
    public class PedidoEntidade
    {
        public int ID { get; set; }
        public string VENDEDOR { get; set; }
        public string CIDADE { get; set; }
        public decimal? VALOR_DO_PEDIDO { get; set; }
        public decimal? VALOR_GAVETA { get; set; }
        [Column("NUM_PEDIDOS")]
        public string NUM_PEDIDO { get; set; }
        public decimal? VALOR_PLASTICA { get; set; }
        public decimal? VALOR_TETRA { get; set; }
        public decimal? VALOR_PA { get; set; }
        public decimal VALOR_TOTAL_CHAVES{ get; set; }
        public int? PDV_PARCELAS { get; set; }
        public DateTime? DATA { get; set; }
        public int? QTD_GAVETA { get; set; }
        public int? QTD_PLASTICA { get; set; }
        public int? QTD_TETRA { get; set; }
        public decimal? VALOR_YALE { get; set; }
        public int? QTD_YALE { get; set; }
        public int USUARIO_ID { get; set; }
        public int BASE_DE_DADOS { get; set; }
        public bool DISTRIBUIDOR { get; set; }

    }
}
