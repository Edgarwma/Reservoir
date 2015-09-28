using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing.Configuration;

namespace HarmonnySearch.Configuration
{
    public class ELMHSConfiguration : HSConfiguration<byte>
    {
        public ELMHSConfiguration(byte[] values, int seed, int maxInputNodesNumber, ERCActivationFunctionType activationFunctionType)
            :base(values, seed)
        {
            MaxInputNodesNumber = maxInputNodesNumber;
            ActivationFunctionType = activationFunctionType;
        }

        public int MaxInputNodesNumber { get; private set; }
        public ERCActivationFunctionType ActivationFunctionType { get; private set; }
    }
}
