using Aprovacao_Pedidos.Enumeraveis;

namespace Aprovacao_Pedidos.ObjetosDeTransferencia
{
    public class PedidoOTC
    {
        public int ID { get; set; }
        public string VENDEDOR { get; set; }
        public string CIDADE { get; set; }
        public decimal? VALOR_DO_PEDIDO { get; set; }
        public decimal? VALOR_YALE { get; set; }
        public string NUM_PEDIDO { get; set; }
        public decimal VALOR_TOTAL_CHAVES { get; set; }
        public int? PDV_PARCELAS { get; set; }
        public bool APROVA_PARCELA { get; set; }
        public TipoDeConexaoAConsiderarDoPedido TipoDeConexao { get; set; }
        public bool DISTRIBUIDOR { get; set; }
        public string? Observacao { get; set; }
        public TipoDeValorAConsiderarNoStatus? Status { get; set; }


        public decimal? GavetasValor { get; set; }
        public decimal? PlasticaValor { get; set; }
        public decimal? TetrasValor { get; set; }
        public decimal? valorPA { get; set; }
        public int? qtd_yale { get; set; }
        public int? qtd_gaveta { get; set; }
        public int? qtd_plastica { get; set; }
        public int? qtd_tetra { get; set; }
        public decimal? resto { get; set; }

        public string ValorYaleDetalhado
        {
            get
            {
                var valorYale = VALOR_YALE.HasValue ? Math.Round(VALOR_YALE.Value, 2) : 0;
                var valorGaveta = GavetasValor.HasValue ? Math.Round(GavetasValor.Value, 2) : 0;
                var valorPlastica = PlasticaValor.HasValue ? Math.Round(PlasticaValor.Value, 2) : 0;
                var valorTetra = TetrasValor.HasValue ? Math.Round(TetrasValor.Value, 2) : 0;
                var valorPA_ = valorPA.HasValue ? Math.Round(valorPA.Value, 2) : 0;
                var valorPR = resto.HasValue ? Math.Round(resto.Value, 2) : 0;


                return $"{qtd_yale} YALE COMUM À R$ {valorYale}/ " +
                    $"{qtd_gaveta} GAVETAS À R$ {valorGaveta} / " +
                    $"{qtd_plastica} PLÁSTICAS À R$ {valorPlastica} / " +
                    $"{qtd_tetra} TETRAS À R$ {valorTetra} / " +
                    $"R$ {valorPA_} em PA /" +
                    $" R$ {valorPR} em PR";
            }
        }
    }
}
