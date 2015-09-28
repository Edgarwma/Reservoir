using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PSO
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

            Util.InicializaRandom(100); //MUDAR PARA 100
            int numParticulas = 10;
            int dimensoes = 100 ;
            int numeroMaximoAvaliacoesFuncao = 100;
            int numIteracoesPorConfig = 1;
            double[][] resultados = new double[numIteracoesPorConfig][];
            
            string dataSetPath = @"C:\Users\Edgar Almeida\Desktop\Dados\caxias.txt";


            for (int l = 0; l < numIteracoesPorConfig; l++)
            {
                resultados[l] = new double[4];// fitness, EPMA, inputN, hiddenNodes
            }

            using (TextWriter twResult = new StreamWriter(@"C:\Users\Edgar Almeida\Desktop\Dados\PSOWilcox.txt"))
            {
                 // Clerc
                Configuracao config2 = new Configuracao(numeroMaximoAvaliacoesFuncao, 0.9, 2.05, 2.05, ETipoTopologia.Local, false, false, ETipoAtualizacaoVelocidade.Clerc);

                #region Config2
                Console.WriteLine("Config2");

                for (int i = 0; i < numIteracoesPorConfig; i++)
                {
                    using (TextWriter twConfig = new StreamWriter(@"C:\Users\Edgar Almeida\Desktop\Dados\PSOConfig2_" + i + ".txt"))
                    {
                        List<Particula> particulas = CriarParticulasAleatorias(dimensoes, numParticulas);

                        PSO teste = new PSO(particulas, config2, dataSetPath);

                        Particula melhorParticula = teste.Executar(twConfig);


                        resultados[i][0]= melhorParticula.MelhorFitness;
                        resultados[i][1] = melhorParticula.BestEval.ValidationEPMA;
                        resultados[i][2] = melhorParticula.Config.Prov.InputsN;
                        resultados[i][3] = melhorParticula.Config.HidenNodesNumber;

                        twResult.Write(resultados[i][0].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                        Console.Write(resultados[i][0].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                        twResult.Write(" " + resultados[i][1].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                        Console.Write(" " + resultados[i][1].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                        twResult.Write(" " + resultados[i][2].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                        Console.Write(" " + resultados[i][2].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                        twResult.Write(" " + resultados[i][3].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                        Console.Write(" " + resultados[i][3].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));

                        twResult.WriteLine();
                        Console.WriteLine();

                        /////////////////// Salvando predição ///////////////////////////////
                    
                        double real = 0;
                        double prediction = 0;
                        double Error = 0;

                        StreamWriter File = new System.IO.StreamWriter(@"C:\Users\Edgar Almeida\Desktop\Dados\" + "predicao"+ i +".csv", true);

                        for (int m = 0; m < melhorParticula.Config.Prov.ValidationSetLines; m++)
                        {
                            for (int j = 0; j < melhorParticula.Config.Prov.OutputsN; j++)
                            {
                                real = melhorParticula.Config.Prov.DeNormalizeData(melhorParticula.Config.Prov.ValidationSet[m].Output[j], PSO.outputA, PSO.outputB, j + (int)melhorParticula.Config.Prov.InputsN);
                                prediction = melhorParticula.Config.Prov.DeNormalizeData(melhorParticula.Eval.ELM.GetValidationT[m][j], PSO.outputA, PSO.outputB, j + (int)melhorParticula.Config.Prov.InputsN);

                                Error = real - prediction;
                                File.WriteLine(real + ";" + prediction + ";" + Error);
                            }
                        }
                        File.Close();
                    }
                }
                #endregion

                for (int l = 0; l < resultados.Length; l++)
                {                    
                    twResult.Write(resultados[l][0].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                    Console.Write(resultados[l][0].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                    twResult.Write(" " +resultados[l][1].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                    Console.Write(" " + resultados[l][1].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                    twResult.Write(" " + resultados[l][2].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                    Console.Write(" " + resultados[l][2].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                    twResult.Write(" " + resultados[l][3].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
                    Console.Write(" " + resultados[l][3].ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));

                    twResult.WriteLine();
                    Console.WriteLine();
                }
            }

            Console.WriteLine("FIM");
            Console.ReadLine();
        }

        private static List<Particula> CriarParticulasAleatorias(int dimensoes, int numParticulas)
        {
            List<Particula> particulas = new List<Particula>(numParticulas);

            for (int j = 0; j < numParticulas; j++)
            {               
                double[] posicao = new double[dimensoes];
                double[] velocidade = new double[dimensoes];

                for (int i = 0; i < dimensoes; i++)
                    velocidade[i] = Math.Round(Util.Random.NextDouble(), 0);

                particulas.Add(new Particula(j, posicao, velocidade));
            }

            int increment = PSO.MaxHiddenNodes / numParticulas;

            int minvalue = 0;
            int maxValue = increment;

            foreach (Particula particle in particulas)
            {
                while (particle.PosicaoAtual.Count(p => p.Equals(1)) <= (30 * 2 / 3))
                {
                    particle.PosicaoAtual = new double[dimensoes];
                    for (int i = 0; i < 30; i++)
                    {
                        particle.PosicaoAtual[i] =((double)Math.Round(Util.Random.NextDouble()));
                    }
                }

                double[] a = Util.CreateList(PSO.MaxHiddenNodes, minvalue, maxValue);

                int c = a.Length - 1;
                for (int i = particle.PosicaoAtual.Length - 1; c > -1; i--)
                {
                    particle.PosicaoAtual[i] = a[c];
                    c--;
                }

                minvalue += increment;
                maxValue += increment;
            }

            return particulas;
        }
    }
}
