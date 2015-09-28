using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing;
using System.IO;
using HarmonnySearch.Particle;
using ReservoirComputing.Configuration;
using HarmonnySearch.HS;
using DataManagement;
using HarmonnySearch.Evaluation;
using ReservoirComputing.Evaluation;

namespace HarmonnySearch.HS
{
    public abstract class HS<T>
    {
        public HS(DataProvider prov, int seed, int memorySize, int maxHiddenNodes, int maxEvaluations,
            ERCActivationFunctionType activationFunctionType, EHSEvaluationFunctionType evaluationFunctionType, EEvaluationInfo performanceInfo)
        {
            _MaxEvaluations = maxEvaluations;
            _MemorySize = memorySize;
            _MaxHiddenNodes = maxHiddenNodes;
            _MaxInputNodes = prov.InputsN;            
            _Prov = prov;
            _Seed = seed;
            _Rand = new Random(_Seed);
            _ActivationFunctionType = activationFunctionType;
            _EvaluationFunctionType = evaluationFunctionType;
            _PerformanceInfo = performanceInfo;
        }
        
        protected List<HSParticle<T>> _Memory = null;
        protected int _MaxEvaluations = -1;
        protected int _MaxHiddenNodes = -1;
        protected int _MaxInputNodes = -1;
        protected int _MemorySize = -1;
        protected int _Seed = -1;
        protected int _Evaluations = 0;

        protected Random _Rand = null;
        protected DataProvider _Prov = null;
        protected ERCActivationFunctionType _ActivationFunctionType;
        protected EHSEvaluationFunctionType _EvaluationFunctionType;
        protected EEvaluationInfo _PerformanceInfo;
        double HMCR = 0.90;

        public abstract EHarmonySearchType Type { get; }

        protected abstract void _Initialize();

        public HSResult<T> Run() 
        {
            int iteration = 0;
            HSParticle<T> p = null;

            // Avaliação inicial de todos os indivíduos
            for (int i = 0; i < _MemorySize; i++)
                _Evaluate(_Memory[i]);

            _Memory.SortAscending();

            for (iteration = 0; (_Evaluations < _MaxEvaluations) && !_StoppingCriterion(); iteration++)
            {
                 p = _GenerateNewHSParticle();

                _Evaluate(p);
                _Memory.Add(p);
                _Memory.SortAscending();

                if (_Memory.Count > _MemorySize)
                    _Memory.RemoveAt(_MemorySize);
            }

            return new HSResult<T> (_Memory[0], iteration, _Evaluations);
        }

        /// <summary>
        /// Critério de parada.
        /// Se o critério foi atingido: Verdadeiro;
        /// caso contrário: Falso
        /// </summary>
        /// <returns></returns>
        protected abstract bool _StoppingCriterion();

        protected int _GetMemoryIndexForNewHarmony()
        {
            int rand = -1;
            
            if(_Rand.NextDouble() < HMCR)
                rand = _Rand.Next(_Memory.Count);
            
            return rand;
        }

        protected abstract HSParticle<T> _GenerateNewHSParticle();
        
        protected virtual void _Evaluate(HSParticle<T> particle)
        {
            _Evaluations++;
        }
    }

}
