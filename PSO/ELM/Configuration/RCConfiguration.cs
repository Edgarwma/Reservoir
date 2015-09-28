using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing;
using DataManagement;

namespace ReservoirComputing.Configuration
{
    public class RCConfiguration : Configuration
    {
        public RCConfiguration(DataProvider prov, int seed, int hidenNodesNumber, double interconnectivity, int warmUpCicles, 
            double spectralRadious, ERCActivationFunctionType activationFunctionType)
            :base(hidenNodesNumber, activationFunctionType)
        {
            Prov = prov;
            Interconnectivity = interconnectivity;
            WarmUpCicles = warmUpCicles;
            Seed = seed;
            SpectralRadious = spectralRadious;
        }
        
        public int Seed { get; private set; }
        public double Interconnectivity { get; private set; }
        public int WarmUpCicles { get; private set; }
        public double SpectralRadious { get; private set; }
        public DataProvider Prov { get; private set; }
    }
}