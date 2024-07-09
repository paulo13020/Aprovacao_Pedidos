using Aprovacao_Pedidos.Enumeraveis;

namespace Aprovacao_Pedidos.ObjetosDeTransferencia
{
    public class PedidoOTE
    {
        public string NumeroDoPedido { get; set; }
        public TipoDeConexaoAConsiderarDoPedido TipoDeConexao { get; set; }
    }
}
