using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSO
{
    public class Configuracao
    {
        public Configuracao(int numMaxAval, double w, double c1, double c2, ETipoTopologia tipoTop, bool variarC1C2, bool variarW, ETipoAtualizacaoVelocidade tipoVel)
        {
            NumeroMaximoAvaliacoesFuncao = numMaxAval;
            W = w;
            C1 = c1;
            C2 = c2;
            TipoTopologia = tipoTop;
            VariarC1C2 = variarC1C2;
            VariarW = variarW;
            TipoAtualizacaoVelocidade = tipoVel;
        }

        public Configuracao(int numMaxAval, double w, double c1, double c2, ETipoTopologia tipoTop, bool variarC1C2, double taxaVariacaoC1C2, int limiteVariacaoC1C2, bool variarW, double taxaVariacaoW, int limiteVariacaoW, ETipoAtualizacaoVelocidade tipoVel)
        {
            NumeroMaximoAvaliacoesFuncao = numMaxAval;
            W = w;
            C1 = c1;
            C2 = c2;
            TipoTopologia = tipoTop;
            VariarC1C2 = variarC1C2;
            TaxaVariacaoC1C2 = taxaVariacaoC1C2;
            LimiteVariacaoC1C2 = limiteVariacaoC1C2;
            VariarW = variarW;
            TaxaVariacaW = taxaVariacaoW;
            LimiteVariacaoW = limiteVariacaoW;
            TipoAtualizacaoVelocidade = tipoVel;
        }

        public int NumeroMaximoAvaliacoesFuncao { get; private set; }
        public double W { get; private set; }
        public double C1 { get; private set; }
        public double C2 { get; private set; }
        public ETipoTopologia TipoTopologia { get; private set; }
        public ETipoAtualizacaoVelocidade TipoAtualizacaoVelocidade { get; private set; }
        public bool VariarC1C2 { get; private set; }
        public double TaxaVariacaoC1C2 { get; private set; }
        public int LimiteVariacaoC1C2 { get; private set; }
        public bool VariarW { get; private set; }
        public double TaxaVariacaW { get; private set; }
        public int LimiteVariacaoW { get; private set; }


    }
}
