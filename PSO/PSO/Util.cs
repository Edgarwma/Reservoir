using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSO
{
    public class Par
    {
        public Par(int prim, int seg)
        {
            Primeiro = prim;
            Segundo = seg;
        }

        public int Primeiro { get; private set; }
        public int Segundo { get; private set; }
    }

    public class CauchyRandom
    {
        private Random _Random;

        public CauchyRandom(Random rand)
        {
            _Random = rand;
        }

        public double NextCauchy(double a = 0, double b = 1)
        {
            double x = Util.Random.NextDouble();

            double p = b / (Math.PI * (Math.Pow(b, 2) + Math.Pow((x - a), 2)));

            return p;
        }
    }

    public class GaussianRandom
    {
        private bool _temDesvio;
        private double _desvio;
        private readonly Random _random;

        public GaussianRandom(Random random)
        {
            _random = random;
        }

        public double NextGaussian(double mu = 0, double sigma = 1)
        {
            if (_temDesvio)
            {
                _temDesvio = false;
                return _desvio * sigma + mu;
            }

            double v1, v2, raiz;
            do
            {
                v1 = 2 * _random.NextDouble() - 1;
                v2 = 2 * _random.NextDouble() - 1;
                raiz = v1 * v1 + v2 * v2;
            } while (raiz >= 1 || raiz == 0);

            var polar = Math.Sqrt(-2 * Math.Log(raiz) / raiz);
            _desvio = v2 * polar;
            _temDesvio = true;
            return v1 * polar * sigma + mu;
        }
    }

    public static class Util
    {
        private static GaussianRandom _GaussianRandom;
        private static CauchyRandom _CauchyRandom;
        private static Random _Random;
              
        public static void InicializaRandom(int semente)
        {
            _Random = new Random(semente);
            _GaussianRandom = new GaussianRandom(_Random);
            _CauchyRandom = new CauchyRandom(_Random);
        }

        public static GaussianRandom GaussianRandom
        {
            get
            {
                return _GaussianRandom;
            }

        }

        public static CauchyRandom CauchyRandom
        {
            get
            {
                return _CauchyRandom;
            }
        }

        public static Random Random
        {
            get
            {
                if (_Random == null)
                    throw new InvalidOperationException("O gerador de números aleatórios não foi inicializado");

                return _Random;
            }
        }

        public static double[] CreateList(int listSize, int minValue, int maxValue)
        {
            double[] list = new double[listSize];

            int value = Util.Random.Next(minValue, maxValue);

            for (int i = 0; i < value; i++)
                list[i] = 1;

            return list;
        }
    }

    public enum ETipoTopologia
    {
        Global = 0,
        Local = 1,
        Focal = 2,
    }

    public enum ETipoAtualizacaoVelocidade
    {
        Classica = 0,
        Clerc = 1
    }
}
