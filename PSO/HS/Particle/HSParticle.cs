using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing;
using ReservoirComputing.Configuration;

namespace HarmonnySearch.Particle
{
    public abstract class HSParticle<T>
    {
        protected HSParticle(T[] values, int seed, ERCActivationFunctionType activationFunctionType)
        {
            this.Values = values;
            this.Seed = seed;
            this._ActivationFunction = activationFunctionType;
        }

        public abstract EHSParticleType Type { get; }
        public T[] Values { get; protected set; }
        public double Fitness;
        protected ERCActivationFunctionType _ActivationFunction;
        public int Seed;        
    }
}