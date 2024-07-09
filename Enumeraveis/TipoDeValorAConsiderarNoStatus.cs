using System.ComponentModel;

namespace Aprovacao_Pedidos.Enumeraveis
{
    public enum TipoDeValorAConsiderarNoStatus
    {
        [Description("Pendente de Aprovação Comercial")]
        PendenteDeAprovacaoComercial = 0,
        [Description("Pendente de Aprovação Diretoria")]
        PendenteDeAprovacaoDiretoria = 1,
        [Description("Aprovado")]
        Aprovado = 2,
        [Description("Reprovado")]
        Reprovado = 3,

    }
}
