using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing.Configuration;

namespace HarmonnySearch.Configuration
{
    public class RCHSConfiguration : HSConfiguration<byte>
    {
        public RCHSConfiguration(byte[] values, int seed, int maxWarmUpCicles,
            double maxInterConnectivity, double minSpectralRadious, ERCActivationFunctionType activationFunctionType, int memorySize, int maxHiddenNodes)
            :base(values, seed)
        {
            MaxWarmUpCicles = maxWarmUpCicles;

            if (maxInterConnectivity < 0 || maxInterConnectivity > 1)
                throw new ArgumentOutOfRangeException("O percentual de interconectividade deve estar entre 0 e 1");

            MaxInterConnectivity = maxInterConnectivity;

            if (minSpectralRadious < 0 || minSpectralRadious > 1)
                throw new ArgumentOutOfRangeException("O raio espectral deve estar entre 0 e 1");

            MinSpectralRadious = minSpectralRadious;
            ActivationFunctionType = activationFunctionType;
            MemorySize = memorySize;
            MaxHiddenNodes = maxHiddenNodes;
        }

        public int MaxHiddenNodes { get; private set; }
        public int MemorySize { get; private set; }
        public int MaxWarmUpCicles { get; private set; }
        public double MaxInterConnectivity { get; private set; }
        public double MinSpectralRadious { get; private set; }
        public ERCActivationFunctionType ActivationFunctionType { get; private set; }
    }
}
