using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataManagement;
using ReservoirComputing.Configuration;
using ReservoirComputing.Evaluation;

namespace HarmonnySearch.Particle
{
    public class ELMHSParticle : HSParticle<byte>
    {
        public ELMHSParticle(DataProvider prov, byte[] values, int seed, int maxInputNodes, ERCActivationFunctionType activationFunctionType)
            :base(values, seed, activationFunctionType)
        {
            _Prov = prov;
            _MaxInputNodes = maxInputNodes;
        }

        private int _MaxInputNodes;
        private DataProvider _Prov;

        public override EHSParticleType Type
        {
            get { return EHSParticleType.RCParticle; }
        }

        public RCEvaluator Evaluator { get; set; }

        public RCConfiguration Config
        {
            get
            {
                int inputNodesNumber = 0;
                int hiddenNodesNumber = 0;

                for (int i = 0; i < _MaxInputNodes; i++)
                {
                    if (Values[i] == 1)
                        inputNodesNumber++;
                }

                for (int i = _MaxInputNodes; i < Values.Length; i++)
                {
                    if (Values[i] == 1)
                        hiddenNodesNumber++;
                }
                
                return new RCConfiguration(_Prov, this.Seed, hiddenNodesNumber, 0, 1, 0, _ActivationFunction);
            }
        }
    }
}
