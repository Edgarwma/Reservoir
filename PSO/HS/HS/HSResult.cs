using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonnySearch.Particle;

namespace HarmonnySearch.HS
{
    public class HSResult<T>
    {
        public HSResult(HSParticle<T> particle, int stopIteration, int stopEvaluations)
        {
            BestParticle = particle;
            StopIteration = stopIteration;
            StopEvaluations = stopEvaluations;
        }

        public HSParticle<T> BestParticle { get; protected set; }
        public int StopIteration { get; protected set; }
        public int StopEvaluations { get; protected set; }
    }
}
