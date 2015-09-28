using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSO
{
    public class Particula
    {
        public Particula(int indice, double[] posicao, double[] velocidade)
        {
            Indice = indice;
            PosicaoAtual = posicao;
            MelhorPosicao = posicao.Clone() as double[];
            Velocidade = velocidade;
        }

        public int Indice { get; private set; }
        public double[] Velocidade { get; private set; }
        public double[] PosicaoAtual { get; set; }
        public double[] MelhorPosicao { get; private set; }
        public RCConfiguration Config { get; set; }
        public RCEvaluator Eval { get; set; }
        public RCEvaluator BestEval { get; set; }

        public double Fitness
        {
            get;
            set;
        }

        public double MelhorFitness
        {
            get;
            set;
        }

        public void AtualizarMelhorPosicaoParticula()
        {
            AtualizarMelhorPosicaoParticula(false);
        }

        public void AtualizarMelhorPosicaoParticula(bool copia)
        {      
            if ((MelhorFitness >= Fitness && Fitness >=0) || copia)
            {
                MelhorFitness = Fitness;
                MelhorPosicao = PosicaoAtual.Clone() as double[];
                BestEval = Eval;
            }
        }

        public double[] GetSubListValuesFromIndex(int count)
        {
            return PosicaoAtual.ToList().GetRange(0, count).ToArray();
        }

        public int GetFlagCountFromSubListValues(int index)
        {
            return GetSubListValuesFromIndex(index).Count(v => v == 1);
        }

        public int GetHiddenNodes(int index, int count)
        {
            return PosicaoAtual.ToList().GetRange(index, count).Count(v => v == 1);
        }
    }
}
