using Aprovacao_Pedidos.BancoDeDados;
using Aprovacao_Pedidos.Enumeraveis;
using Aprovacao_Pedidos.Excecoes;
using Aprovacao_Pedidos.Models;
using Aprovacao_Pedidos.ObjetosDeTransferencia;
using Dapper;
using FirebirdSql.Data.FirebirdClient;

namespace Aprovacao_Pedidos.Fachada
{
    public class PedidosFachada
    {
        #region Membros
        private readonly ContextoDeBancoDeDados _ctxBancoDeDados;
        private readonly IConfiguration _configuration;
        private PedidoOTE _pedidoOTE;
        #endregion

        #region Construtores
        public PedidosFachada(PedidoOTE pedidoOTE, ContextoDeBancoDeDados ctxBancoDeDados, IConfiguration configuration)
        {
            _ctxBancoDeDados = ctxBancoDeDados;
            _pedidoOTE = pedidoOTE;
            _configuration = configuration;
        }
        #endregion

        #region Métodos

        public PedidoOTC MontarOTC(IEnumerable<PedidoEntidade> qryPedidosEntidade, bool considerarTabelaDeAprovacao = false)
        {

            // Materializar a lista de pedidos aprovados
            var pedidosAprovadosLista = _ctxBancoDeDados.pedidosAprovados.ToList();

            var qry = (from entPedidosEntidade in qryPedidosEntidade
                       join entPedidosAprovadosEntidade in pedidosAprovadosLista on entPedidosEntidade.ID equals entPedidosAprovadosEntidade.AprovacaoID into entPedidosAprovados_LeftJoin
                       from entPedidosAprovadosEntidade in entPedidosAprovados_LeftJoin.DefaultIfEmpty()
                       let existeMaisDeUmPedido = qryPedidosEntidade.Count() > 0
                       let valorDoPedido = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.VALOR_DO_PEDIDO) : entPedidosEntidade.VALOR_DO_PEDIDO
                       let valorTotalChaves = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.VALOR_TOTAL_CHAVES) : entPedidosEntidade.VALOR_TOTAL_CHAVES
                       let valorPA = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.VALOR_PA) : entPedidosEntidade.VALOR_PA
                       let valorPR = valorDoPedido - (valorTotalChaves + valorPA)
                       select new PedidoOTC
                       {
                           CIDADE = entPedidosEntidade.CIDADE,
                           NUM_PEDIDO = existeMaisDeUmPedido ? string.Join(",", qryPedidosEntidade.Select(p => p.NUM_PEDIDO)) : entPedidosEntidade.NUM_PEDIDO,
                           VENDEDOR = entPedidosEntidade.VENDEDOR,
                           VALOR_DO_PEDIDO = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.VALOR_DO_PEDIDO) : entPedidosEntidade.VALOR_DO_PEDIDO,
                           GavetasValor = existeMaisDeUmPedido ? qryPedidosEntidade.Average(p => p.VALOR_GAVETA) : entPedidosEntidade.VALOR_GAVETA,
                           qtd_gaveta = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.QTD_GAVETA) : entPedidosEntidade.QTD_GAVETA,
                           qtd_tetra = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.QTD_TETRA) : entPedidosEntidade.QTD_TETRA,
                           qtd_plastica = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.QTD_PLASTICA) : entPedidosEntidade.QTD_PLASTICA,
                           qtd_yale = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.QTD_YALE) : entPedidosEntidade.QTD_YALE,
                           VALOR_YALE = existeMaisDeUmPedido ? qryPedidosEntidade.Average(p => p.VALOR_YALE) : entPedidosEntidade.VALOR_YALE,
                           PlasticaValor = existeMaisDeUmPedido ? qryPedidosEntidade.Average(p => p.VALOR_PLASTICA) : entPedidosEntidade.VALOR_PLASTICA,
                           TetrasValor = existeMaisDeUmPedido ? qryPedidosEntidade.Average(p => p.VALOR_TETRA) : entPedidosEntidade.VALOR_TETRA,
                           valorPA = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.VALOR_PA) : entPedidosEntidade.VALOR_PA,
                           VALOR_TOTAL_CHAVES = existeMaisDeUmPedido ? qryPedidosEntidade.Sum(p => p.VALOR_TOTAL_CHAVES) : entPedidosEntidade.VALOR_TOTAL_CHAVES,
                           PDV_PARCELAS = entPedidosEntidade.PDV_PARCELAS,
                           DISTRIBUIDOR = entPedidosEntidade.DISTRIBUIDOR,
                           TipoDeConexao = _pedidoOTE.TipoDeConexao,
                           Observacao = considerarTabelaDeAprovacao ? entPedidosAprovadosEntidade.Observacao : "",
                           resto = valorPR
                       
                       }).FirstOrDefault();

            return qry;
        }

        public List<AprovacaoOTC> MontarOTC_Aprovacao(IEnumerable<PedidoEntidade> qryPedidosEntidade)
        {
            // Materializar a lista de pedidos se não for nula
            var pedidosLista = qryPedidosEntidade?.ToList() ?? _ctxBancoDeDados.pedidos.ToList();

            // Materializar a lista de pedidos aprovados
            var pedidosAprovadosLista = _ctxBancoDeDados.pedidosAprovados.ToList();

            var qry = (from entPedidosEntidade in pedidosLista
                       join entPedidosAprovadosEntidade in pedidosAprovadosLista on entPedidosEntidade.ID equals entPedidosAprovadosEntidade.AprovacaoID
                       select new AprovacaoOTC
                       {
                           ID = entPedidosEntidade.ID,
                           Data = entPedidosEntidade.DATA.Value,
                           Status = entPedidosAprovadosEntidade.AprovadoDiretoria ? TipoDeValorAConsiderarNoStatus.Aprovado.GetDescription() : entPedidosAprovadosEntidade.ReprovadoDiretoria ? TipoDeValorAConsiderarNoStatus.Reprovado.GetDescription() :
                                    entPedidosAprovadosEntidade.AprovadoComercial ? TipoDeValorAConsiderarNoStatus.PendenteDeAprovacaoDiretoria.GetDescription() : entPedidosAprovadosEntidade.ReprovadoComercial ? TipoDeValorAConsiderarNoStatus.Reprovado.GetDescription() : TipoDeValorAConsiderarNoStatus.PendenteDeAprovacaoComercial.GetDescription(),
                           Vendedor = entPedidosEntidade.VENDEDOR
                       }).ToList();
            return qry;
        }

        public PedidoOTC ListaPedidoDeAprovacao()
        {
            //Montar Query 
            var qryAConsiderar = $"select ped.pdv_numero as num_pedido,r.rep_nome as vendedor,(m.mun_nome || ',' || m.mun_uf) as cidade, ped.pdv_valorliquido as valor_do_pedido,\r\n" +
                $"(select round(avg(i.pvi_unitario),2)\r\nfrom pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\n" +
                $"and p.pro_nivel3 = '1'\r\nand p.pro_nivel4 = '1') as valor_yale, \r\n(select sum(i.pvi_quantidade)\r\nfrom pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\n" +
                $"where i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel3 = '1'\r\n) as qtd_yale,\r\n(select round(avg(i.pvi_unitario),2)\r\nfrom pedidos_vendas_itens i\r\n" +
                $"inner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel3 = '29') as valor_gaveta,\r\n(select sum(i.pvi_quantidade)\r\n" +
                $"from pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel3 = '29') as qtd_gaveta,\r\n" +
                $"(select round(avg(i.pvi_unitario),2)\r\nfrom pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel3 = '4') as valor_plastica, \r\n" +
                $"(select sum(i.pvi_quantidade)\r\nfrom pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel3 = '4') as qtd_plastica,\r\n" +
                $"(select round(avg(i.pvi_unitario),2)\r\nfrom pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel3 = '2') as valor_tetra, \r\n" +
                $"(select sum(i.pvi_quantidade)\r\nfrom pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel3 = '2') as qtd_tetra,\r\n" +
                $"(select round(sum(i.pvi_totalitem),2)\r\nfrom pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel1 = '1' and p.pro_nivel2 not in (1) ) as valor_pa,\r\n" +
                $"(select round(sum(i.pvi_totalitem),2)\r\nfrom pedidos_vendas_itens i\r\ninner join produtos p on p.pro_codigo = i.pvi_pro_codigo\r\nwhere i.pvi_numero = ped.pdv_numero\r\nand p.pro_nivel2 = '1') as valor_total_chaves, ped.pdv_parcelas\r\n" +
                $"from pedidos_vendas ped\r\ninner join clientes c on c.cli_codigo = ped.pdv_cli_codigo\r\ninner join municipios m on m.mun_codigo = c.cli_mun_codigo\r\ninner join representantes r on r.rep_codigo = ped.pdv_rep_codigo\r\n" +
                $"where ped.pdv_numero in ({_pedidoOTE.NumeroDoPedido})";

            //Usar a conexão do firebird para executar o código
            using (var conexao = new FbConnection(_configuration.GetConnectionString(_pedidoOTE.TipoDeConexao.ToString())))
            {
                //Abre a conexão
                conexao.Open();

                //Realiza a consulta no banco
                var entPedido = conexao.Query<PedidoEntidade>(qryAConsiderar);

                //Verifica se existe mais de 1 pedido
                if (entPedido.Count() >= 1)
                {
                    //Verifica se existe mais de 1 vendedor no pedido
                    var existeMaisDeUmVendedor = entPedido.Select(p => p.VENDEDOR).Distinct().Count();

                    //caso tenha
                    if (existeMaisDeUmVendedor > 1)
                    {
                        throw new ValorInvalidoExcecao("Não é possível fazer a listagem com 2 ou mais vendedores diferentes.");
                    }

                    //Monta o OTC para o retorno
                    return MontarOTC(entPedido);
                }
                //Caso não tenha nenhum registro
                else
                {
                    throw new ValorInvalidoExcecao($"Não existe dados disponível para o pedido {_pedidoOTE.NumeroDoPedido}");
                }
            }
        }

        #endregion
    }
}
