using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataManagement;
using ReservoirComputing.Configuration;
using ReservoirComputing.Evaluation;
using HarmonnySearch.Evaluation;
using HarmonnySearch.Configuration;
using HarmonnySearch.Particle;
using ReservoirComputing;

namespace HarmonnySearch.HS
{
    public class ELMHS : HS<byte>
    {
        public ELMHS(DataProvider prov, int seed, int memorySize, int maxHiddenNodes, int maxEvaluations, ERCActivationFunctionType activationFunctionType,
            EHSEvaluationFunctionType evaluationFunctionType, EEvaluationInfo performanceInfo)
            : base(prov, seed, memorySize, maxHiddenNodes, maxEvaluations, activationFunctionType, evaluationFunctionType, performanceInfo)
        {
            _MaxInputNodes = prov.InputsN;

            _ParticleArrayValuesSize = _MaxInputNodes + _MaxHiddenNodes;

            _Initialize();
        }

        int _ParticleArrayValuesSize;

        private ELMHSConfiguration _GenerateNewRandomELMHSConfiguration()
        {
            ELMHSConfiguration config = null;
            
            List<byte> values = new List<byte>(_ParticleArrayValuesSize);

            for (int i = 0; i < _ParticleArrayValuesSize; i++)
                values.Add(0);

            do
            {
                for (int i = 0; i < _ParticleArrayValuesSize; i++)
                {
                    if(i == _MaxInputNodes)
                        values.Sort();

                    values[i] = (byte)_Rand.Next(0, 2);
                }
            }
            while(!_ValidateParticleValues(values));

            config = new ELMHSConfiguration(values.ToArray(), _Seed, values.GetRange(0, _MaxInputNodes).Count<byte>(p => p == 1), _ActivationFunctionType);

            return config; 
        }

        private bool _ValidateParticleValues(List<byte> values)
        {            
            //Validando neurônios na camada de entrada
            bool inputs = values.GetRange(0, _MaxInputNodes).Exists(v => v == 1);
            if (!inputs)
                return false;

            //Validando neurônios na camada escondida
            bool neuro = values.GetRange(_MaxInputNodes, _MaxHiddenNodes).Exists(v => v == 1);
            if (!neuro)
                return false;

           return true;
        }

        public override EHarmonySearchType Type
        {
            get { return EHarmonySearchType.ELMHamonySearch; }
        }

        protected override void _Initialize()
        {           
            int inputNodesStep = _MaxInputNodes > _MemorySize ? _MaxInputNodes / _MemorySize : _MemorySize / _MaxInputNodes;
            int hiddenNodesStep = _MaxHiddenNodes > _MemorySize ? _MaxHiddenNodes / _MemorySize : _MemorySize / _MaxHiddenNodes;

            int inputNodesCount = inputNodesStep;
            int hiddenNodesCount = hiddenNodesStep;


            List<byte> inputNodesValues = null;
            List<byte> hiddenNodesValues = null;
            List<byte> values = null;

            List<List<byte>> inputNodes = new List<List<byte>>(_MemorySize);
            List<List<byte>> hiddenNodes = new List<List<byte>>(_MemorySize);

            _Memory = new List<HSParticle<byte>>(_MemorySize);

            for (int i = 0; i < _MemorySize; i++)
            {
                values = new List<byte>(_ParticleArrayValuesSize);
                inputNodesValues = new List<byte>(_MaxInputNodes);

                for (int j = 0; j < _MaxInputNodes; j++)
                {
                    if (j < inputNodesCount)
                        inputNodesValues.Add(1);
                    else
                        inputNodesValues.Add(0);
                }

                inputNodesValues.Sort();
                values.AddRange(inputNodesValues);

                hiddenNodesValues = new List<byte>(_MaxHiddenNodes);

                for (int j = 0; j < _MaxHiddenNodes; j++)
                {
                    if (j < hiddenNodesCount)
                        hiddenNodesValues.Add(1);
                    else
                        hiddenNodesValues.Add(0);
                }

                hiddenNodesValues.Shuffle(_Rand);
                values.AddRange(hiddenNodesValues);                           

                if (!_ValidateParticleValues(values))
                    throw new ArgumentOutOfRangeException("Particula criada com valores inválidos");

                inputNodes.Add(inputNodesValues);
                hiddenNodes.Add(hiddenNodesValues);

                inputNodesCount += inputNodesStep;
                hiddenNodesCount += hiddenNodesStep;
            }

            inputNodes.Shuffle(_Rand);
            hiddenNodesValues.Shuffle(_Rand);

            for (int i = 0; i < _MemorySize; i++)
            {
                values = new List<byte>(_ParticleArrayValuesSize);

                values.AddRange(inputNodes[i]);
                values.AddRange(hiddenNodes[i]);

                if (!_ValidateParticleValues(values))
                    throw new ArgumentOutOfRangeException("Particula criada com valores inválidos");

                _Memory.Add(HSParticleFactory.CreateELMParticle(_Prov, new ELMHSConfiguration(values.ToArray(), _Seed, _MaxInputNodes, _ActivationFunctionType)));
            }   
        }

        protected override void _Evaluate(HSParticle<byte> particle)
        {
            base._Evaluate(particle);

            double fitness = 0;
            RC rc = null;
            ELMHSParticle p = null;
            RCConfiguration config = null;

            p = particle as ELMHSParticle;
            config = p.Config;

            rc = new RC(config.Prov.TrainSet, config.Prov.ValidationSet, config.Prov.TestSet, config);
            rc.Run();

            RCEvaluator eval = new RCEvaluator(rc, config.Prov, _PerformanceInfo);
            eval.Evaluate();

            if (_Prov.ExecutionType == EExecutionType.Predction)
                fitness = _EvaluateFunctionPrediction(eval.ValidationEMQ, eval.ValidationDEV, config, p);
            else
                fitness = _EvaluateFunctionClassification(eval.ValidationSR, eval.ValidationDEV, config, p); // TODO: Ver se faz sentido ser variância da taxa de acerto

            particle.Fitness = fitness;
            ((ELMHSParticle)particle).Evaluator = eval;
        }

        private double _EvaluateFunctionClassification(double[] SR, double[] DEV, RCConfiguration config, ELMHSParticle particle)
        {
            double fitness = 0;

            double meanSR= 0;
            double meanDEV = 0;

            for (int i = 0; i < SR.Length; i++)
            {
                meanSR += SR[i];
                meanDEV += DEV[i];
            }

            meanSR = meanSR / SR.Length;
            meanDEV = meanDEV / DEV.Length;

            switch (_EvaluationFunctionType)
            {
                case EHSEvaluationFunctionType.Weight:
                    double EMQWeight = 5;
                    double InputWeight = 2;
                    double HiddenNodesWeight = 3;

                    if (meanSR != 0)
                    {
                        fitness = (EMQWeight * meanSR) /
                                 (
                                   (InputWeight * (_MaxInputNodes / config.Prov.DataSet[0].Input.Length))
                                   +
                                   (HiddenNodesWeight * (_MaxHiddenNodes / config.HidenNodesNumber))
                                 );
                    }
                    break;

                case EHSEvaluationFunctionType.PSE:
                    fitness = meanSR + (2 * meanDEV * config.HidenNodesNumber / config.Prov.ValidationSetLines);
                    //fitness = SR + (2 * VAR * particle.GetFlagCountFromSubListValues((int)_MaxInputNodes) / _Prov.ValidationSetLines);
                    break;
            }

            return fitness;
        }
        private double _EvaluateFunctionPrediction(double[] EMQ, double[] DEV, RCConfiguration config, ELMHSParticle particle)
        {
            double fitness = 0;
            double meanEMQ = 0;
            double meanDEV = 0;

            for (int i = 0; i < EMQ.Length; i++)
            {
                meanEMQ += EMQ[i];
                meanDEV += DEV[i];
            }

            meanEMQ = meanEMQ / EMQ.Length;
            meanDEV = meanDEV / DEV.Length;

            switch (_EvaluationFunctionType)
            {
                case EHSEvaluationFunctionType.Weight:
                    fitness = meanEMQ;
                    break;

                case EHSEvaluationFunctionType.PSE:
                    fitness = meanEMQ +
                        (2 * meanDEV * config.HidenNodesNumber / config.Prov.ValidationSetLines)+
                        (config.HidenNodesNumber / _MaxHiddenNodes);
                    break;
            }

            return fitness;
        }

        protected override bool _StoppingCriterion()
        {
            double mean = 0;
            foreach (HSParticle<byte> p in _Memory)
                mean += p.Fitness;

            mean = mean / _MemorySize;
            if (mean / _Memory[_Memory.Count - 1].Fitness >= 0.9)
                return true;

            return false;
        }

        protected override HSParticle<byte> _GenerateNewHSParticle()
        {
            List<byte> values = new List<byte>(_ParticleArrayValuesSize);

            do
            {
                for (int i = 0; i < _ParticleArrayValuesSize; i++)
                {
                    int index = _GetMemoryIndexForNewHarmony();

                    if (index == -1)
                        values.Add((byte)Math.Round(_Rand.NextDouble()));
                    else
                        values.Add(_Memory[index].Values[i]);

                }
            } while (!_ValidateParticleValues(values));


            ELMHSConfiguration config = new ELMHSConfiguration(values.ToArray(), _Seed, _MaxInputNodes, _ActivationFunctionType);

            return HSParticleFactory.CreateELMParticle(_Prov, config);
        }
    }  
}
