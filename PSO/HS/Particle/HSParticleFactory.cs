using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonnySearch.Configuration;
using DataManagement;

namespace HarmonnySearch.Particle
{
    public class HSParticleFactory
    {
        public static RCHSParticle CreateRCParticle(DataProvider prov, RCHSConfiguration config)
        {
            return new RCHSParticle(prov, config.Values, config.Seed, config.MaxWarmUpCicles,
                            config.MaxInterConnectivity, config.MinSpectralRadious, config.ActivationFunctionType, config.MemorySize, config.MaxHiddenNodes);
        }

        public static ELMHSParticle CreateELMParticle(DataProvider prov, ELMHSConfiguration config)
        {
            int inputNodes = 0;

            for (int i = 0; i < config.MaxInputNodesNumber; i++)
            {
                if (config.Values[i] == 1)
                    inputNodes++;
            }                    

            return new ELMHSParticle(prov.ShrinkProvider(inputNodes), config.Values, config.Seed, config.MaxInputNodesNumber, config.ActivationFunctionType);
        }
    }
}