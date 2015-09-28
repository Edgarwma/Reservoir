using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing;
using ReservoirComputing.Configuration;
using ReservoirComputing.Evaluation;
using DataManagement;

namespace HarmonnySearch.Particle
{
    public class RCHSParticle : HSParticle<byte>
    {
        public RCHSParticle(DataProvider prov, byte[] values, int seed, int maxWarmUpCicles, double maxInterConnectivity,
            double minSpectralRadious, ERCActivationFunctionType activationFunctionType, int memorysize, int maxHiddenNodes)
            :base(values, seed, activationFunctionType)
        {
            _Prov = prov;
            _MaxWarmUpCicles = maxWarmUpCicles;
            _MaxInterConnectivity = maxInterConnectivity;
            _MinSpectralRadious = minSpectralRadious;
            _MemorySize = memorysize;
            _MaxHiddenNodes = maxHiddenNodes;
        }

        private int _MaxHiddenNodes;
        private int _MemorySize;
        private int _MaxWarmUpCicles;
        private double _MaxInterConnectivity;
        private double _MinSpectralRadious;
        private DataProvider _Prov;

        /// <summary>
        /// Quantidade de elementos possíveis de interconectividade no array de valores
        /// </summary>
        public static int InterconnectivityArraySize = 50;

        /// <summary>
        /// Quantidade de elementos possíveis de raio espectral no array de valores
        /// </summary>
        public static int SpectralRadiousArraySize = 25;

        public override EHSParticleType Type
        {
            get { return EHSParticleType.RCParticle; }
        }

        public RCEvaluator Evaluator { get; set; }

        public RCConfiguration Config
        {
            get
            {
                int warmUpCicles = 0;
                int hiddenNodesNumber = 0;
                double interConnectivity = 0;
                double spectralRadious = 1;

                double interConnectivityFactor = _MaxInterConnectivity / InterconnectivityArraySize;
                double spectralRadiousFactor = (1 -_MinSpectralRadious) / SpectralRadiousArraySize;
                
                int interConnectivityIndex = _MaxWarmUpCicles + InterconnectivityArraySize;
                int spectralRadiousIndex = _MaxWarmUpCicles + InterconnectivityArraySize + _MaxHiddenNodes;


                for (int i = 0; i < _MaxWarmUpCicles; i++)
                {
                    if (Values[i] == 1)
                        warmUpCicles++;
                }

                for (int i = _MaxWarmUpCicles; i < interConnectivityIndex; i++)
                {
                    if (Values[i] == 1)
                        interConnectivity += interConnectivityFactor;
                }

                for (int i = interConnectivityIndex; i < spectralRadiousIndex; i++)
                {
                    if (Values[i] == 1)
                        hiddenNodesNumber++;
                }

                for (int i = spectralRadiousIndex; i < Values.Length; i++)
                {
                    if (Values[i] == 1)
                        spectralRadious -= spectralRadiousFactor;
                }
                
                interConnectivity = Math.Round(interConnectivity, 4);
                spectralRadious = Math.Round(spectralRadious, 4);

                if (interConnectivity > _MaxInterConnectivity)
                    interConnectivity = _MaxInterConnectivity;

                if (spectralRadious < _MinSpectralRadious)
                    spectralRadious = _MinSpectralRadious;

                return new RCConfiguration(_Prov, this.Seed, hiddenNodesNumber, interConnectivity, warmUpCicles,
                                             spectralRadious, _ActivationFunction);
            }
        }
    }
}