using Aprovacao_Pedidos.BancoDeDados;
using Aprovacao_Pedidos.Enumeraveis;
using Aprovacao_Pedidos.Excecoes;
using Aprovacao_Pedidos.Fachada;
using Aprovacao_Pedidos.Models;
using Aprovacao_Pedidos.ObjetosDeTransferencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aprovacao_Pedidos.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ContextoDeBancoDeDados _ctxBancoDeDados;
        private readonly IConfiguration _configuration;
        public static int idUsuarioConectado;

        public PedidosController(ContextoDeBancoDeDados ctxBancoDeDados, IConfiguration configuration)
        {
            _ctxBancoDeDados = ctxBancoDeDados;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                var idDoUsuarioLogado = User.Claims.FirstOrDefault(c => c.Type == "UsuarioID");

                if (User.IsInRole(TipoDePermissaoDeUsuario.Comercial.ToString()))
                {
                    var qry = from entPedidos in _ctxBancoDeDados.pedidos.AsEnumerable()
                              join entPedidosAprovados in _ctxBancoDeDados.pedidosAprovados.AsEnumerable() on entPedidos.ID equals entPedidosAprovados.AprovacaoID into entPedidosAprovados_LeftJoin
                              from entPedidosAprovados in entPedidosAprovados_LeftJoin.DefaultIfEmpty()
                              select entPedidos;

                    var pedidosOTC = new PedidosFachada(null, _ctxBancoDeDados, null).MontarOTC_Aprovacao(qry);

                    //Trazer apenas os pedidos "Pendente de Aprovação Comercial"
                    pedidosOTC = pedidosOTC.Where(entP => entP.Status == TipoDeValorAConsiderarNoStatus.PendenteDeAprovacaoComercial.GetDescription() || entP.Status == TipoDeValorAConsiderarNoStatus.PendenteDeAprovacaoDiretoria.GetDescription()).OrderByDescending(p => p.ID).ToList();

                    return View(pedidosOTC);
                }


                if (User.IsInRole(TipoDePermissaoDeUsuario.Diretoria.ToString()))
                {
                    var qry = from entPedidos in _ctxBancoDeDados.pedidos.AsEnumerable()
                              join entPedidosAprovados in _ctxBancoDeDados.pedidosAprovados.AsEnumerable() on entPedidos.ID equals entPedidosAprovados.AprovacaoID
                              where entPedidosAprovados.AprovadoComercial == true
                              select entPedidos;

                    var pedidosOTC = new PedidosFachada(null, _ctxBancoDeDados, null).MontarOTC_Aprovacao(qry);

                    //Trazer apenas os pedidos "Pendente de Aprovação Diretoria"
                    pedidosOTC = pedidosOTC.Where(entP => entP.Status == TipoDeValorAConsiderarNoStatus.PendenteDeAprovacaoDiretoria.GetDescription()).OrderByDescending(p => p.ID).ToList();
                    return View(pedidosOTC);
                }

                if (idDoUsuarioLogado != null)
                {
                    idUsuarioConectado = int.Parse(idDoUsuarioLogado.Value);

                    var qry = _ctxBancoDeDados.pedidos.Where(entP => entP.USUARIO_ID == idUsuarioConectado).AsEnumerable();

                    var pedidosOTC = new PedidosFachada(null, _ctxBancoDeDados, null).MontarOTC_Aprovacao(qry).OrderByDescending(p => p.ID);

                    return View(pedidosOTC);
                }
                else
                {
                    throw new ValorInvalidoExcecao("Usuario não possui cadastro.");
                }
            }
            catch (ValorInvalidoExcecao ex)
            {
                TempData["Erro"] = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Incluir(string numeroPedido, string localConexao)
        {
            try
            {
                if (numeroPedido == null)
                {
                    throw new ValorInvalidoExcecao("Informe o número do pedido.");
                }

                if (localConexao == null)
                {
                    throw new ValorInvalidoExcecao("Informe o Local de Conexão");
                }

                //Constrói o OTE para mandar a fachada
                var pedidoOTE = new PedidoOTE
                {
                    NumeroDoPedido = numeroPedido,
                    TipoDeConexao = localConexao == TipoDeConexaoAConsiderarDoPedido.SJC.ToString() ? TipoDeConexaoAConsiderarDoPedido.SJC : TipoDeConexaoAConsiderarDoPedido.MINAS
                };

                var pedidosOTC = new PedidosFachada(pedidoOTE, _ctxBancoDeDados, _configuration).ListaPedidoDeAprovacao();

                return PartialView("_CriarPedido", pedidosOTC);

            }
            catch (ValorInvalidoExcecao ex)
            {
                TempData["Erro"] = ex.Message;
                return View();
            }

        }

        [HttpPost]
        public IActionResult Criar(PedidoOTC pedidoOTC)
        {
            try
            {
                if (pedidoOTC == null)
                {
                    throw new ValorInvalidoExcecao("Informe os campos do pedido");
                }

                var entAprovacao = new PedidoEntidade
                {
                    CIDADE = pedidoOTC.CIDADE,
                    DATA = DateTime.Now,
                    NUM_PEDIDO = pedidoOTC.NUM_PEDIDO,
                    PDV_PARCELAS = pedidoOTC.PDV_PARCELAS,
                    QTD_GAVETA = pedidoOTC.qtd_gaveta,
                    QTD_PLASTICA = pedidoOTC.qtd_plastica,
                    QTD_TETRA = pedidoOTC.qtd_tetra,
                    QTD_YALE = pedidoOTC.qtd_yale,
                    VALOR_DO_PEDIDO = pedidoOTC.VALOR_DO_PEDIDO,
                    VALOR_GAVETA = pedidoOTC.GavetasValor,
                    VALOR_PA = pedidoOTC.valorPA,
                    VALOR_PLASTICA = pedidoOTC.PlasticaValor,
                    VALOR_TETRA = pedidoOTC.TetrasValor,
                    VALOR_TOTAL_CHAVES = pedidoOTC.VALOR_TOTAL_CHAVES,
                    VALOR_YALE = pedidoOTC.VALOR_YALE,
                    VENDEDOR = pedidoOTC.VENDEDOR,
                    USUARIO_ID = idUsuarioConectado,
                    DISTRIBUIDOR = pedidoOTC.DISTRIBUIDOR,
                    BASE_DE_DADOS = (int)pedidoOTC.TipoDeConexao
                };

                //Adiciona ao contexto
                _ctxBancoDeDados.pedidos.Add(entAprovacao);

                //Persiste o Registro
                _ctxBancoDeDados.SaveChanges();


                //Incluir na tabela Pedidos_Aprovados
                var pedidosAprovados = new PedidosAprovadosEntidade
                {
                    AprovacaoID = entAprovacao.ID,
                    AprovadoComercial = false,
                    AprovadoDiretoria = false
                };

                //Adiciona ao contexto
                _ctxBancoDeDados.pedidosAprovados.Add(pedidosAprovados);
                //Persiste o registro
                _ctxBancoDeDados.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (ValorInvalidoExcecao ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Incluir));
            }
        }

        [HttpGet]
        public IActionResult SelecionarPedido(int id)
        {
            try
            {
                var entPedido = _ctxBancoDeDados.pedidos.Where(entP => entP.ID == id);

                var pedidoOTE = new PedidoOTE
                {
                    NumeroDoPedido = entPedido.Select(p => p.NUM_PEDIDO).FirstOrDefault(),
                    TipoDeConexao = entPedido.Select(p => p.BASE_DE_DADOS).FirstOrDefault() == 7 ? TipoDeConexaoAConsiderarDoPedido.SJC : TipoDeConexaoAConsiderarDoPedido.MINAS
                };

                var pedidosOTC = new PedidosFachada(pedidoOTE, _ctxBancoDeDados, _configuration).MontarOTC(entPedido, considerarTabelaDeAprovacao: true);



                //Obter o Status do Pedido
                var statusDoPedido = (from entPedidos in entPedido
                                      join entPedidosAprovados in _ctxBancoDeDados.pedidosAprovados.AsQueryable() on entPedidos.ID equals entPedidosAprovados.AprovacaoID
                                      select new
                                      {
                                          Status = entPedidosAprovados.AprovadoDiretoria ? TipoDeValorAConsiderarNoStatus.Aprovado : entPedidosAprovados.ReprovadoDiretoria ? TipoDeValorAConsiderarNoStatus.Reprovado:
                                     entPedidosAprovados.AprovadoComercial ? TipoDeValorAConsiderarNoStatus.PendenteDeAprovacaoDiretoria: entPedidosAprovados.ReprovadoComercial ? TipoDeValorAConsiderarNoStatus.Reprovado: TipoDeValorAConsiderarNoStatus.PendenteDeAprovacaoComercial,

                                      }).FirstOrDefault();


                if(statusDoPedido != null)
                {
                    pedidosOTC.Status = statusDoPedido.Status;
                }              


                return View(pedidosOTC);
            }
            catch (ValorInvalidoExcecao ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        public IActionResult SelecionarPedido(PedidoEntidade entPedido, string aprovacao, string observacaoDeReprova)
        {
            try
            {
                var entPedidosAprovacao =
               (from entPedidos in _ctxBancoDeDados.pedidos.Where(entP => entP.ID == entPedido.ID).ToList()
                join entPedidosAprovados in _ctxBancoDeDados.pedidosAprovados.ToList() on entPedido.ID equals entPedidosAprovados.AprovacaoID
                select entPedidosAprovados).FirstOrDefault();

                var nivelDoUsuarioConectado = User.IsInRole(TipoDePermissaoDeUsuario.Comercial.ToString()) ? TipoDePermissaoDeUsuario.Comercial : TipoDePermissaoDeUsuario.Diretoria;


                //Caso a aprovação venha null
                if (aprovacao == null)
                {
                    throw new ValorInvalidoExcecao("Informe o status do pedidos");
                }


                if (aprovacao == "aprovar")
                {

                    if (nivelDoUsuarioConectado == TipoDePermissaoDeUsuario.Comercial)
                    {
                        if (entPedidosAprovacao.ReprovadoComercial == true)
                        {
                            throw new ValorInvalidoExcecao("Um pedido reprovado não pode ser mais alterado");
                        }

                        entPedidosAprovacao.AprovadoComercial = true;
                        entPedidosAprovacao.ReprovadoComercial = false;
                    }
                    else
                    {
                        if (entPedidosAprovacao.AprovadoComercial == false)
                        {
                            throw new ValorInvalidoExcecao("O pedido precisa ser aprovado pelo Comercial antes da Diretoria.");
                        }

                        if (entPedidosAprovacao.ReprovadoDiretoria == true)
                        {
                            throw new ValorInvalidoExcecao("Um pedido reprovado não pode ser mais alterado");
                        }
                        entPedidosAprovacao.AprovadoDiretoria = true;
                        entPedidosAprovacao.ReprovadoDiretoria = false;

                    }
                }
                else
                {
                    if (nivelDoUsuarioConectado == TipoDePermissaoDeUsuario.Comercial)
                    {
                        if (entPedidosAprovacao.AprovadoComercial == true)
                        {
                            throw new ValorInvalidoExcecao("Um pedido aprovado não pode ser mais alterado");
                        }
                        entPedidosAprovacao.ReprovadoComercial = true;
                        entPedidosAprovacao.AprovadoComercial = false;
                        entPedidosAprovacao.Observacao = observacaoDeReprova.ToString();
                    }
                    else
                    {
                        if (entPedidosAprovacao.AprovadoComercial == false && entPedidosAprovacao.ReprovadoComercial == false)
                        {
                            throw new ValorInvalidoExcecao("O Pedido precisa ser aprovado ou reprovado pelo Comercial antes da Diretoria");
                        }

                        if (entPedidosAprovacao.AprovadoDiretoria == true)
                        {
                            throw new ValorInvalidoExcecao("Um pedido aprovado não pode ser mais alterado");
                        }
                        entPedidosAprovacao.ReprovadoDiretoria = true;
                        entPedidosAprovacao.AprovadoDiretoria = false;
                        entPedidosAprovacao.Observacao = (string)observacaoDeReprova.ToString();
                    }
                }


                _ctxBancoDeDados.pedidosAprovados.Update(entPedidosAprovacao);
                _ctxBancoDeDados.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (ValorInvalidoExcecao ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }

        }
    }
}
