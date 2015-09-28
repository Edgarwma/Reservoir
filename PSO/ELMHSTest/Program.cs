using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing;
using System.IO;
using System.ComponentModel;
using DataManagement;
using ReservoirComputing.Configuration;
using HarmonnySearch.Evaluation;
using ReservoirComputing.Evaluation;
using HarmonnySearch.HS;
using HarmonnySearch.Particle;

namespace Simulator
{
    class Program
    {  
        static void Main(string[] args)
        {
            try
            {
                string dataSetPath = @"C:\Users\Edgar\Desktop\Dados\Itaipu\itaipu_serie.csv";
                string simulationResultPath = @"C:\Users\Edgar\Desktop\Dados\Itaipu\result\";

                Simulation s = new Simulation();
                //s.Simulate(dataSetPath, simulationResultPath);

                dataSetPath = @"C:\Users\Edgar\Desktop\Dados\Sobradinho\sobradinho_serie.csv";
                simulationResultPath = @"C:\Users\Edgar\Desktop\Dados\Sobradinho\result\";
                s = new Simulation();
                s.Simulate(dataSetPath, simulationResultPath);

                //dataSetPath = @"C:\Users\Edgar\Desktop\Dados\Furnas\furnas_serie.csv";
                //simulationResultPath = @"C:\Users\Edgar\Desktop\Dados\Furnas\result\";
                //s = new Simulation();
                //s.Simulate(dataSetPath, simulationResultPath);

                //dataSetPath = @"C:\Users\Edgar\Desktop\Dados\Tres Marias\tres_marias_serie.csv";
                //simulationResultPath = @"C:\Users\Edgar\Desktop\Dados\Tres Marias\result\";
                //s = new Simulation();
                //s.Simulate(dataSetPath, simulationResultPath);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("Fim Simulação");
                Console.ReadLine();
            }
        }                 
    }
}
