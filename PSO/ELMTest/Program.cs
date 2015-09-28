using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ReservoirComputing;
using DataManagement;
using ReservoirComputing.Configuration;
using ReservoirComputing.Evaluation;

namespace ELMTest
{
    class Program
    {
        public static double inputA = -0.85;
        public static double inputB = 0.85;
        public static double outputA = -0.85;
        public static double outputB = 0.85;
        public static double trainSize = 0.50;
        public static double validationSize = 0.25;
        public static double testSize = 0.25;
 
        static void Main(string[] args)
        {
            try
            {
                // TestLags();
                DateTime StartTime, EndTime;
                List<object[]> bestRCs = new List<object[]>();
                int MaxRCMemory = 10;
                int seed = 1;
                int MaxHiddenNodesNumber = 150;
                int MaxWarmUpCicles = 100;
                double MaxInterConnectivity = 1;
                double SpectralRadious = 0.9;

                string dataSetPath = @"C:\Users\Edgar\Desktop\Dados\PLD\lag.csv";

                for (int h = 10; h < MaxHiddenNodesNumber && bestRCs.Count < 1; h += 5)
                {
                    for (double i = 0.0; i < MaxInterConnectivity && bestRCs.Count < 1; i += 0.02)
                    {
                        for (int w = 1; w < MaxWarmUpCicles && bestRCs.Count < 1; w += 5)
                        {
                            StartTime = DateTime.Now;
                            DataProvider dp = new DataProvider(dataSetPath, EExecutionType.Predction, seed, trainSize, validationSize, testSize);
                            //dp.ApplyLogToData();
                            dp.NormalizeData(inputA, inputB, outputA, outputB);
                            dp.ShuffleDataSet();
                            dp.SplitData();

                            RC rc = new RC(dp.TrainSet, dp.ValidationSet, dp.TestSet, new RCConfiguration(dp, seed, h, i, w, SpectralRadious, ERCActivationFunctionType.HyperbolicTangent));

                            rc.Run();
                            RCEvaluator eval = new RCEvaluator(rc, dp, EEvaluationInfo.EMQ | EEvaluationInfo.DEV | EEvaluationInfo.EPMA | EEvaluationInfo.RMSE);

                            eval.Evaluate();

                            RCEvaluator eval2 = new RCEvaluator(rc, dp, EEvaluationInfo.EMQ | EEvaluationInfo.DEV | EEvaluationInfo.EPMA | EEvaluationInfo.RMSE, true);

                            eval2.Evaluate();

                            EndTime = DateTime.Now;

                            Console.WriteLine("Neurônios camada escondida: " + h);
                            Console.WriteLine("Interconectividade (%): " + i.ToString("0.##"));
                            Console.WriteLine("Ciclos aquecimento: " + w);
                            Console.WriteLine("EMQ(7): " + eval.TestEMQ[6].ToString("0.##"));
                            Console.WriteLine("RMSE(7): " + eval.TestRMSE[6].ToString("0.##"));
                            Console.WriteLine("EPMA(7): " + eval.TestEPMA[6].ToString("0.##"));
                            Console.WriteLine("Tempo: " + EndTime.Subtract(StartTime).ToReadableString());
                            Console.WriteLine("#################################");

                            object[] b = new object[4];
                            b[0] = eval.TestEPMA[0];
                            b[1] = dp;
                            b[2] = rc;
                            b[3] = eval;

                            bestRCs.Add(b);

                            bestRCs.Sort(delegate(object[] p1, object[] p2)
                                    {
                                        return ((double)p1[0]).CompareTo(((double)p2[0]));
                                    });

                            if (bestRCs.Count > MaxRCMemory)
                                bestRCs.RemoveAt(MaxRCMemory);
                        }
                    }
                }

                Console.WriteLine("Salvando Predição da melhor configuração");
                #region Salvando Predição
                double prediction = 0;
                double Error = 0;

                DataProvider dataProv = bestRCs[0][1] as DataProvider;
                RC elm = bestRCs[0][2] as RC;
                RCEvaluator evaluator = bestRCs[0][3] as RCEvaluator;

                StreamWriter File = new System.IO.StreamWriter(@"C:\Users\Edgar\Desktop\Dados\Furnas\Predicao\predicao_" +
                    elm.HiddenNodesNumber + "_" + elm.InterConnectivity + "_" + elm.WarmUpCicles + "_" + evaluator.TestEPMA[6] + ".csv", false);

                for (int m = 0; m < dataProv.TestSetlines; m++)
                {
                    string temp = string.Empty;
                    for (int j = 0; j < dataProv.OutputsN; j++)
                    {
                        prediction = dataProv.DeNormalizeOutputData(elm.TestT[m][j], j);

                        Error = dataProv.TestSet[m].RealOutput[j] - prediction;
                        temp = string.Concat(temp + dataProv.TestSet[m].RealOutput[j] + ";" + prediction + ";" + Error + ";");
                    }
                    File.WriteLine(temp);
                }
                File.Close();
                #endregion

                Console.WriteLine("FIM");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }


        static void TestLags()
        {

            DataBuilder db = new DataBuilder(@"C:\Users\Edgar\Desktop\Dados\PLD\pesado_SE_serie.csv");
            db.LagData(1, 1);

            Data[] set = db.BuildDataSet();
            db.SaveToFile(@"C:\Users\Edgar\Desktop\Dados\PLD\lag.csv");
        }
    }
}
