using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLP
{
    // Este enum existe para sabermos qual tipo de erro utilizaremos.

    // Para um trabalho de previsão, uma base de dados onde existem valores zero ou
    // muito próximos a zero, nos impede de utilizar o EPMA, pois teríamos casos com 
    // o denominador da fórmula do EPMA (Valor observado) zero, o que resultaria em
    // valores incoerentes.

    // Para estes casos com bases de valores zerados, o recomendado é abolir o EPMA
    // e utilizar o EMA (Erro médio absoluto) e o EMAN (Erro médio absoluto normalizado),
    // seguem fórmulas:

    // EMA = [Somatório dos valores absolutos das substrações (Valor observado - Valor calculado)] / N
    // onde N é a quantidade de valores no conjunto de verificação.

    // EMAN = EMA / Potência instalada da usina
    // também pode se utilizar a média entre as potências medidas, cada um tem seus pontos negativos
    // e positivos, argumentar de acordo com o que você desejará utilizar.

    public enum EnumTipoValoresPrevisao
    {
        ExistemValoresZeroOuProxNaBase, ValoresAcimaDeZeroNaBase
    }

}
