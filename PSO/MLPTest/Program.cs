using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLP;
using DataManagement;
using MLP.Evaluation;

namespace MLPTest
{
    public class Program
    {
        static double inputA = 0.10;
        static double inputB = 0.90;
        static double outputA = 0.10;
        static double outputB = 0.90;
        static double trainSize = 0.50;
        static double validationSize = 0.25;
        static double testSize = 0.25;
        static int seed = 1;

        [STAThread]
        static void Main(string[] args)
        {
            string path = @"C:\Users\Edgar\Desktop\Dados\Itaipu\itaipu_serie.csv";
                        
            double alfa = 0.80;
            double beta = 0.20;

            int maxCicles = 5;
            int hiddenNeurons = 75;

            int inputLag = 48;
            int outputLag = 12;

            Console.WriteLine("Carregando dados da planilha...");

            DataBuilder db = new DataBuilder(@path);
            db.LagData(inputLag, outputLag);

            Data[] set = db.BuildDataSet();

            DataProvider prov = new DataProvider(set, EExecutionType.Predction, 1, trainSize, validationSize, testSize);

            Console.WriteLine("Aplicando tranformação logarítmica...");
            prov.ApplyLogToData();

            Console.WriteLine("Normalizando dados...");
            prov.NormalizeData(inputA, inputB, outputA, outputB);
            
            Console.WriteLine("Embaralhando dados...");
            prov.ShuffleDataSet();
            
            Console.WriteLine("Dividindo conjuntos de dados...");
            prov.SplitData();

            double[][] entradasTreinamento = null;
            double[][] saidasTreinamento = null;
            double[][] entradasValidacao = null;
            double[][] saidasValidacao = null;
            double[][] entradasTeste = null;
            double[][] saidasTeste = null;

            entradasTreinamento = new double[prov.TrainSetLines][];
            saidasTreinamento = new double[prov.TrainSetLines][];
            for (int i = 0; i < prov.TrainSetLines; i++)
            {
                entradasTreinamento[i] = new double[prov.TrainSet[i].Input.Length];
                saidasTreinamento[i] = new double[prov.TrainSet[i].Output.Length];

                for (int j = 0; j < prov.TrainSet[i].Input.Length; j++)
                    entradasTreinamento[i][j] = prov.TrainSet[i].Input[j];

                for (int j = 0; j < prov.TrainSet[i].Output.Length; j++)
                    saidasTreinamento[i][j] = prov.TrainSet[i].Output[j];
            }

            entradasValidacao = new double[prov.ValidationSetLines][];
            saidasValidacao = new double[prov.ValidationSetLines][];
            for (int i = 0; i < prov.ValidationSetLines; i++)
            {
                entradasValidacao[i] = new double[prov.ValidationSet[i].Input.Length];
                saidasValidacao[i] = new double[prov.ValidationSet[i].Output.Length];

                for (int j = 0; j < prov.ValidationSet[i].Input.Length; j++)
                    entradasValidacao[i][j] = prov.ValidationSet[i].Input[j];

                for (int j = 0; j < prov.ValidationSet[i].Output.Length; j++)
                    saidasValidacao[i][j] = prov.ValidationSet[i].Output[j];
            }

            entradasTeste = new double[prov.TestSetlines][];
            saidasTeste = new double[prov.TestSetlines][];
            for (int i = 0; i < prov.TestSetlines; i++)
            {
                entradasTeste[i] = new double[prov.TestSet[i].Input.Length];
                saidasTeste[i] = new double[prov.TestSet[i].Output.Length];

                for (int j = 0; j < prov.TestSet[i].Input.Length; j++)
                    entradasTeste[i][j] = prov.TestSet[i].Input[j];

                for (int j = 0; j < prov.TestSet[i].Output.Length; j++)
                    saidasTeste[i][j] = prov.TestSet[i].Output[j];
            }

            Console.WriteLine("Treinando Rede neural...");
            RedeMLP mlp;
            mlp = new RedeMLP(prov,
                entradasTreinamento,//array de entradas de treinamento
                saidasTreinamento,//array de saidas de treinamento
                entradasValidacao,//array de entradas de validacao
                saidasValidacao,//array de saidas de validacao
                alfa,//valor de alpha
                beta,//valor de beta
                maxCicles,//maximo de ciclos
                hiddenNeurons,//quatidade de neuronios escondida
                EnumTipoExecucao.Previsao,//booleano para definir se é previsao(true) ou classificacao(false)
                Algoritmos.BACKPROPAGATION,//constante q define o algoritmo a ser utilizado
                seed);

            Console.WriteLine("Testando Rede neural...");
            mlp.testar(entradasTeste, saidasTeste);

            EMLPEvaluationInfo PerformanceInfo = EMLPEvaluationInfo.EPMA | EMLPEvaluationInfo.EMQ | EMLPEvaluationInfo.DEV | EMLPEvaluationInfo.EvaluateLikeONS;
            MLPEvaluator eval = new MLPEvaluator(mlp, prov, PerformanceInfo);
            eval.Evaluate();
               

            Console.ReadLine();
    //        double[][] entradasTreinamento = null;
    //        double[][] saidasTreinamento = null;
    //        double[][] entradasValidacao = null;
    //        double[][] saidasValidacao = null;
    //        double[][] entradasTeste = null;
    //        double[][] saidasTeste = null;

    //         DataProvider prov = new DataProvider(@"C:\Users\Edgar\Desktop\Dados\Itaipu\lag.csv", EExecutionType.Predction, 1, 0.5, 0.25, 0.25);
    //            prov.ApplyLogToData();
    //            prov.NormalizeData(inputA, inputB, outputA, outputB);

    //            prov.ShuffleDataSet();
    //            prov.SplitData();

    //        entradasTreinamento = new double[prov.TrainSetLines][];
    //        saidasTreinamento = new double[prov.TrainSetLines][];
    //        for (int i = 0; i < prov.TrainSetLines; i++)
    //        {
    //            entradasTreinamento[i] = new double[prov.TrainSet[i].Input.Length];
    //            saidasTreinamento[i] = new double[prov.TrainSet[i].Output.Length];

    //            for (int j = 0; j < prov.TrainSet[i].Input.Length; j++)
    //                entradasTreinamento[i][j] = prov.TrainSet[i].Input[j];

    //            for (int j = 0; j < prov.TrainSet[i].Output.Length; j++)
    //                saidasTreinamento[i][j] = prov.TrainSet[i].Output[j];
    //        }

    //        entradasValidacao = new double[prov.ValidationSetLines][];
    //        saidasValidacao = new double[prov.ValidationSetLines][];
    //        for (int i = 0; i < prov.ValidationSetLines; i++)
    //        {
    //            entradasValidacao[i] = new double[prov.ValidationSet[i].Input.Length];
    //            saidasValidacao[i] = new double[prov.ValidationSet[i].Output.Length];

    //            for (int j = 0; j < prov.ValidationSet[i].Input.Length; j++)
    //                entradasValidacao[i][j] = prov.ValidationSet[i].Input[j];

    //            for (int j = 0; j < prov.ValidationSet[i].Output.Length; j++)
    //                saidasValidacao[i][j] = prov.ValidationSet[i].Output[j];
    //        }

    //        entradasTeste = new double[prov.TestSetlines][];
    //        saidasTeste = new double[prov.TestSetlines][];
    //        for (int i = 0; i < prov.TestSetlines; i++)
    //        {
    //            entradasTeste[i] = new double[prov.TestSet[i].Input.Length];
    //            saidasTeste[i] = new double[prov.TestSet[i].Output.Length];

    //            for (int j = 0; j < prov.TestSet[i].Input.Length; j++)
    //                entradasTeste[i][j] = prov.TestSet[i].Input[j];

    //            for (int j = 0; j < prov.TestSet[i].Output.Length; j++)
    //                saidasTeste[i][j] = prov.TestSet[i].Output[j];
    //        }

    //            RedeMLP mlp;
    //            mlp = new RedeMLP(entradasTreinamento,//array de entradas de treinamento
    //                saidasTreinamento,//array de saidas de treinamento
    //                entradasValidacao,//array de entradas de validacao
    //                saidasValidacao,//array de saidas de validacao
    //                0.7,//valor de alpha
    //                0.4,//valor de beta
    //                600,//maximo de ciclos
    //                20,//quantidade de neuronios escondida
    //                false,//booleano para definir se é previsao(true) ou classificacao(false)
    //                Algoritmos.BACKPROPAGATION);//constante q define o algoritmo a ser utilizado
    //        mlp.testar(entradasTeste, saidasTeste);
    //        Console.WriteLine("erros: " + mlp.getErros());
    //        Console.ReadLine();
    //        //Grafico grafico = new Grafico(mlp.getErros());

    //        /*Teste Estatistico*/
    ////        int totalTestes = 30;
    ////        for (int i = 0; i < totalTestes; i++) {
    ////            mlp = new MLP(dados.getEntradasTreinamento().getArray(),//array de entradas de treinamento
    ////                    dados.getSaidasTreinamento().getArray(),//array de saidas de treinamento
    ////                    dados.getEntradasValidacao().getArray(),//array de entradas de validacao
    ////                    dados.getSaidasValidacao().getArray(),//array de saidas de validacao
    ////                    0.7,//valor de alpha
    ////                    0.4,//valor de beta
    ////                    600,//maximo de ciclos
    ////                    20,//quatidade de neuronios escondida
    ////                    true,//booleano para definir se é previsao(true) ou classificacao(false)
    ////                    Algoritmos.BACKPROPAGATION);//constante q define o algoritmo a ser utilizado
    ////            mlp.testar(dados.getEntradasTeste().getArray(), dados.getSaidasTeste().getArray());
    ////        }

    //        /*Fim Teste Estatistico*/
        }
    }
}
