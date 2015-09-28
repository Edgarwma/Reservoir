using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing.Configuration;
using HarmonnySearch.Particle;
using DataManagement;
using HarmonnySearch.Evaluation;
using HarmonnySearch.Configuration;
using ReservoirComputing;
using ReservoirComputing.Evaluation;

namespace HarmonnySearch.HS
{
    public class RCHS : HS<byte>
    {
        public RCHS(double maxInterConnectivity, int maxWarmUpCicles, double minSpectralRadious, DataProvider prov, int seed,
            int memorySize, int maxHiddenNodes, int maxEvaluations, ERCActivationFunctionType activationFunctionType,
            EHSEvaluationFunctionType evaluationFunctionType, EEvaluationInfo performanceInfo)
            : base(prov, seed, memorySize, maxHiddenNodes, maxEvaluations, activationFunctionType, evaluationFunctionType, performanceInfo)
        {
            _MaxWarmUpCicles = maxWarmUpCicles;
            _MaxInterConnectivity = maxInterConnectivity;
            _MinSpectralRadious = minSpectralRadious;

            _ParticleArrayValuesSize = RCHSParticle.InterconnectivityArraySize + _MaxWarmUpCicles + _MaxHiddenNodes + RCHSParticle.SpectralRadiousArraySize;

            _Initialize();
        }

        double _MaxInterConnectivity;
        int _MaxWarmUpCicles;
        double _MinSpectralRadious;
        int _ParticleArrayValuesSize;

       
                
        private bool _ValidateParticleValues(List<byte> values)
        {            
            //Validando ciclos de aquecimento
            bool cicles = values.GetRange(0, _MaxWarmUpCicles).Exists(v => v == 1);
            if (!cicles)
                return false;

            //Validando interconectividade
            bool inter = values.GetRange(_MaxWarmUpCicles, RCHSParticle.InterconnectivityArraySize).Exists(v => v == 1);
            if (!inter)
                return false;

            //Validando neurônios na camada escondida
            bool neuro = values.GetRange(_MaxWarmUpCicles + RCHSParticle.InterconnectivityArraySize, _MaxHiddenNodes).Exists(v => v == 1);
            if (!neuro)
                return false;

            //Validando raio espectral
            bool spectral = values.GetRange(_MaxWarmUpCicles + _MaxHiddenNodes + RCHSParticle.InterconnectivityArraySize, RCHSParticle.SpectralRadiousArraySize).Exists(v => v == 1);
            if (!spectral)
                return false;

           return true;
        }

        public override EHarmonySearchType Type
        {
            get { return EHarmonySearchType.RCHamonySearch; }
        }

        protected override void _Initialize()
        {           
            int warmUpCiclesStep = _MaxWarmUpCicles > _MemorySize ? _MaxWarmUpCicles / _MemorySize : _MemorySize / _MaxWarmUpCicles;
            int interConnectivityStep = RCHSParticle.InterconnectivityArraySize > _MemorySize ? RCHSParticle.InterconnectivityArraySize / _MemorySize : _MemorySize / RCHSParticle.InterconnectivityArraySize;
            int hiddenNodesStep = _MaxHiddenNodes > _MemorySize ? _MaxHiddenNodes / _MemorySize : _MemorySize / _MaxHiddenNodes;
            int spectralRadiousStep = RCHSParticle.SpectralRadiousArraySize > _MemorySize ? RCHSParticle.SpectralRadiousArraySize / _MemorySize : _MemorySize / RCHSParticle.SpectralRadiousArraySize;

            int warmUpCiclesCount = warmUpCiclesStep;
            int interConnectivityCount = interConnectivityStep;
            int hiddenNodesCount = hiddenNodesStep;
            int spectralRadiousCount = spectralRadiousStep;

            List<byte> warmUpValues = null;
            List<byte> interConnectivityValues = null;
            List<byte> hiddenNodesValues = null;
            List<byte> spectralRadiousValues = null;
            List<byte> values = null;

            List<List<byte>> warmUps = new List<List<byte>>(_MemorySize);
            List<List<byte>> interConnectivities = new List<List<byte>>(_MemorySize);
            List<List<byte>> hiddenNodes = new List<List<byte>>(_MemorySize);
            List<List<byte>> spectralRadious = new List<List<byte>>(_MemorySize);

            _Memory = new List<HSParticle<byte>>(_MemorySize);

            for (int i = 0; i < _MemorySize; i++)
            {
                warmUpValues = new List<byte>(_MaxWarmUpCicles);
                values = new List<byte>(_ParticleArrayValuesSize);

                for (int j = 0; j < _MaxWarmUpCicles; j++)
                {
                    if (j < warmUpCiclesCount)
                        warmUpValues.Add(1);
                    else
                        warmUpValues.Add(0);
                }

                warmUpValues.Shuffle(_Rand);
                values.AddRange(warmUpValues);

                interConnectivityValues = new List<byte>(RCHSParticle.InterconnectivityArraySize);

                for (int j = 0; j < RCHSParticle.InterconnectivityArraySize; j++)
                {
                    if (j < interConnectivityCount)
                        interConnectivityValues.Add(1);
                    else
                        interConnectivityValues.Add(0);
                }

                interConnectivityValues.Shuffle(_Rand);
                values.AddRange(interConnectivityValues);

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

                spectralRadiousValues = new List<byte>(RCHSParticle.SpectralRadiousArraySize);

                for (int j = 0; j < RCHSParticle.SpectralRadiousArraySize; j++)
                {
                    if (j < spectralRadiousCount)
                        spectralRadiousValues.Add(1);
                    else
                        spectralRadiousValues.Add(0);
                }

                spectralRadiousValues.Shuffle(_Rand);
                values.AddRange(spectralRadiousValues);                  

                if (!_ValidateParticleValues(values))
                    throw new ArgumentOutOfRangeException("Particula criada com valores inválidos");

                warmUps.Add(warmUpValues);
                interConnectivities.Add(interConnectivityValues);
                hiddenNodes.Add(hiddenNodesValues);
                spectralRadious.Add(spectralRadiousValues);

                warmUpCiclesCount += warmUpCiclesStep;
                interConnectivityCount += interConnectivityStep;
                hiddenNodesCount += hiddenNodesStep;
                spectralRadiousCount += spectralRadiousStep;          
            }

            warmUps.Shuffle(_Rand);
            interConnectivities.Shuffle(_Rand);
            hiddenNodes.Shuffle(_Rand);
            spectralRadiousValues.Shuffle(_Rand);

            for (int i = 0; i < _MemorySize; i++)
            {
                values = new List<byte>(_ParticleArrayValuesSize);

                values.AddRange(warmUps[i]);
                values.AddRange(interConnectivities[i]);
                values.AddRange(hiddenNodes[i]);
                values.AddRange(spectralRadious[i]);

                if (!_ValidateParticleValues(values))
                    throw new ArgumentOutOfRangeException("Particula criada com valores inválidos");

                _Memory.Add(HSParticleFactory.CreateRCParticle(_Prov, new RCHSConfiguration(values.ToArray(), _Seed, _MaxWarmUpCicles, _MaxInterConnectivity, _MinSpectralRadious, _ActivationFunctionType, _MemorySize, _MaxHiddenNodes)));
            }
        }

        protected override void _Evaluate(HSParticle<byte> particle)
        {
            base._Evaluate(particle);

            double fitness = 0;
            RC rc = null;
            RCHSParticle p = null;
            RCConfiguration config = null;

            p = particle as RCHSParticle;
            config = p.Config;
            
            rc = new RC(_Prov.TrainSet, _Prov.ValidationSet, _Prov.TestSet, config);
            rc.Run();            
            
            RCEvaluator eval = new RCEvaluator(rc, _Prov, _PerformanceInfo);
            eval.Evaluate();

            if (_Prov.ExecutionType == EExecutionType.Predction)
                fitness = _EvaluateFunctionPrediction(eval.ValidationEMQ, eval.ValidationDEV, config, p);
            else
                fitness = _EvaluateFunctionClassification(eval.ValidationSR, eval.ValidationDEV, config, p); // TODO: Ver se faz sentido ser variância da taxa de acerto

            particle.Fitness = fitness;
            ((RCHSParticle)particle).Evaluator = eval;
        }

        private double _EvaluateFunctionClassification(double[] SR, double[] DEV, RCConfiguration config, RCHSParticle particle)
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
                    fitness = meanSR + (2 * meanDEV * config.HidenNodesNumber / _Prov.ValidationSetLines);
                    //fitness = SR + (2 * VAR * particle.GetFlagCountFromSubListValues((int)_MaxInputNodes) / _Prov.ValidationSetLines);
                    break;
            }

            return fitness;
        }
        private double _EvaluateFunctionPrediction(double[] EMQ, double[] DEV, RCConfiguration config, RCHSParticle particle)
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
                        (2 * meanDEV * config.HidenNodesNumber / _Prov.ValidationSetLines) +
                        (config.HidenNodesNumber / _MaxHiddenNodes) +
                        (config.WarmUpCicles / _MaxWarmUpCicles) +
                        (_MinSpectralRadious / config.SpectralRadious) ;
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


            RCHSConfiguration config = new RCHSConfiguration(values.ToArray(), _Seed, _MaxWarmUpCicles, _MaxInterConnectivity, _MinSpectralRadious, _ActivationFunctionType, _MemorySize, _MaxHiddenNodes);

            return HSParticleFactory.CreateRCParticle(_Prov, config);
        }
    }
}
