using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ReservoirComputing;

namespace PSO
{
    public class PSO
    {
        public PSO(List<Particula> particulas, Configuracao config, string dataSetPath)
        {
            _Particulas = particulas;
            _NumeroMaximoAvaliacoesFuncao = config.NumeroMaximoAvaliacoesFuncao;
            _W = config.W;
            _C1 = config.C1;
            _C2 = config.C2;
            _TipoTopologia = config.TipoTopologia;
            _VariarC1C2 = config.VariarC1C2;
            _TaxaVariacaoC1C2 = config.TaxaVariacaoC1C2;
            _LimiteVariacaoC1C2 = config.LimiteVariacaoC1C2;
            _VariarW = config.VariarW;
            _TaxaVariacaoW = config.TaxaVariacaW;
            _LimiteVariacaoW = config.LimiteVariacaoW;
            _TipoVel = config.TipoAtualizacaoVelocidade;
            _DataSetPath = dataSetPath;

            if(_TipoVel == ETipoAtualizacaoVelocidade.Clerc)
            {
                double fi = _C1 + _C2;
                _W = 2.0 / Math.Abs(2 - fi - Math.Sqrt(fi * fi - 4 * fi));
             }

            _MontarTopologiaLocal();
        }

        private List<Particula> _Particulas;
        private ETipoTopologia _TipoTopologia;
        private ETipoAtualizacaoVelocidade _TipoVel;
        private int _NumeroMaximoAvaliacoesFuncao;
        private double _W;
        private double _C1;
        private double _C2;
        private Particula _GBest;
        private Dictionary<int, Par> _Topologia;
        private bool _VariarC1C2;
        private double _TaxaVariacaoC1C2;
        private int _LimiteVariacaoC1C2;
        private bool _VariarW;
        private double _TaxaVariacaoW;
        private int _LimiteVariacaoW;
        private double PosMax = 1.0;
        private double PosMin = 0;
        private double VMax = 0.25;
        private Particula _ParticulaFocal;
        private int count = 0;


        private string _DataSetPath;
        public static double inputA = 0.15;
        public static double inputB = 0.85;
        public static double outputA = 0.15;
        public static double outputB = 0.85;
        public static int MaxHiddenNodes =70;

        private void _Rastringin (Particula p)
        {
            int n = p.PosicaoAtual.Length;
            double s = 0;
            for (int i = 0; i < n; i++)
            {
                s = s + (Math.Pow(p.PosicaoAtual[i], 2) - 10 * Math.Cos(2 * Math.PI * p.PosicaoAtual[i]));
            }
            count++;
            p.Fitness = s;

            // Variação de C1 e C2
            if (_VariarC1C2)
            {
                if (count % _LimiteVariacaoC1C2 == 0)
                {
                    _C1 = _C1 - _TaxaVariacaoC1C2;
                    _C2 = _C2 + _TaxaVariacaoC1C2; 
                }
            }

            // Variação de W
            if (_VariarW)
            {
                if (count % _LimiteVariacaoW == 0)
                    _W = _W - _TaxaVariacaoW;
            }
        }

        private void _ELM(Particula p)
        {
            DataProvider prov = new DataProvider(_DataSetPath, EExecutionType.Predction, Util.Random);

            RCConfiguration config = _GetELMConfigurationFromPSOParticle(p, prov);

            config.Prov.MaxValue = prov.MaxValue.Clone() as double[];
            config.Prov.MinValue = prov.MinValue.Clone() as double[];
            config.Prov.NormalizeData(inputA, inputB, outputA, outputB);

            config.Prov.ShuffleDataSet(1);
            config.Prov.SplitData();

            RC elm = new RC(config.Prov.TrainSet, config.Prov.ValidationSet, config);

            try
            {
                elm.Run();
            }
            catch
            { }


            RCEvaluator eval = new RCEvaluator(elm, EEvaluationInfo.EMQ | EEvaluationInfo.DEV | EEvaluationInfo.EPMA);

            eval.Evaluate();


            double fitness = eval.TrainEPMA + (2 * eval.ValidationDEV * p.GetFlagCountFromSubListValues((int)config.Prov.InputsN) / config.Prov.ValidationSetLines) + (config.HidenNodesNumber / MaxHiddenNodes);

            p.Eval = eval;
            p.Fitness = fitness;
            p.Config = config;

            count++;
            
            // Variação de C1 e C2
            if (_VariarC1C2)
            {
                if (count % _LimiteVariacaoC1C2 == 0)
                {
                    _C1 = _C1 - _TaxaVariacaoC1C2;
                    _C2 = _C2 + _TaxaVariacaoC1C2;
                }
            }

            // Variação de W
            if (_VariarW)
            {
                if (count % _LimiteVariacaoW == 0)
                    _W = _W - _TaxaVariacaoW;
            } 
        }

        private RCConfiguration _GetELMConfigurationFromPSOParticle(Particula particle, DataProvider dProv)
        {
            int inputNodesLengh = particle.GetFlagCountFromSubListValues((int)dProv.InputsN);

            if (inputNodesLengh == 0)
            {
                particle.PosicaoAtual[0] = 1;
                inputNodesLengh = particle.GetFlagCountFromSubListValues((int)dProv.InputsN);
            }

            Data[] dataSet = new Data[dProv.DataSetLines];
            int index = 0;

            for (int i = 0; i < dProv.DataSetLines; i++)
            {
                index = 0;
                dataSet[i] = new Data(new double[inputNodesLengh], new double[dProv.OutputsN]);
                dataSet[i].Output = dProv.DataSet[i].Output;

                for (int k = 0; k < dProv.InputsN; k++)
                {
                    if (particle.PosicaoAtual[k] == 1)
                    {
                        dataSet[i].Input[index] = dProv.DataSet[i].Input[k];
                        index++;
                    }
                }
            }

            
            DataProvider prov = new DataProvider(dataSet, EExecutionType.Predction, Util.Random);
            int hiddenNodes = particle.GetHiddenNodes((int)dProv.InputsN, MaxHiddenNodes);

            if (hiddenNodes == 0)
            {
                particle.PosicaoAtual[(int)dProv.InputsN] = 1;
                hiddenNodes = particle.GetHiddenNodes((int)dProv.InputsN, MaxHiddenNodes);
            }

            prov.MaxClassificationClass = dProv.MaxClassificationClass;

            return new RCConfiguration(prov, hiddenNodes, 1, 0.05, 1, EActivationFunctionType.SigmoidLogistic, EExecutionType.Predction);
        }

        private void _MontarTopologiaLocal()
        {
            if (_TipoTopologia == ETipoTopologia.Local)
            {
                _Topologia = new Dictionary<int, Par>();

                foreach (Particula part in _Particulas)
                {
                    if (part.Indice == 0)
                        _Topologia.Add(part.Indice, new Par(_Particulas.Count - 1, 1));
                    else if (part.Indice == _Particulas.Count - 1)
                        _Topologia.Add(part.Indice, new Par(part.Indice - 1, 0));
                    else
                        _Topologia.Add(part.Indice, new Par(part.Indice - 1, part.Indice + 1));
                }
            }
        }

        private double _GetMelhorPosicaoGlobal(int indiceParticula, int indiceDimensao)
        {
            double val = 0;

            switch (_TipoTopologia)
            {
                case ETipoTopologia.Global:
                    val = _GBest.MelhorPosicao[indiceDimensao];
                    break;
                case ETipoTopologia.Local:
                    Particula a, b;
                    a = _Particulas.Find(p => p.Indice == _Topologia[indiceParticula].Primeiro);
                    b = _Particulas.Find(p => p.Indice == _Topologia[indiceParticula].Segundo);

                    val = a.MelhorFitness <= b.MelhorFitness ? a.MelhorPosicao[indiceDimensao] : b.MelhorPosicao[indiceDimensao];
                    break;
                default:
                    break;
            }

            return val;
        }

        private void _AtualizarVelocidadeParticulas()
        {
            #region Se for focal
            if (_TipoTopologia == ETipoTopologia.Focal)
            {
                // Seleção da partícula focal (a seleção é aleatória).
                Random randomParticulaFocal = Util.Random;
                int indiceParticulaFocal = randomParticulaFocal.Next(_Particulas.Count - 1);
                _ParticulaFocal = _Particulas[indiceParticulaFocal];

                // Seleção da melhor partícula. A partícula focal sem movimentará de acordo com ela.
                _Particulas.Sort(delegate(Particula i1, Particula i2) { return i1.Fitness.CompareTo(i2.Fitness); });
                _GBest = _Particulas[0];

                // Movimentação da partícula focal de acordo com a melhor partícula encontrada.
                for (int i = 0; i < _ParticulaFocal.Velocidade.Length; i++)
                {
                    switch (_TipoVel)
                    {
                        case ETipoAtualizacaoVelocidade.Classica:
                            _ParticulaFocal.Velocidade[i] = _W * _ParticulaFocal.Velocidade[i] +
                        _C1 * Util.Random.NextDouble() * (_ParticulaFocal.MelhorPosicao[i] - _ParticulaFocal.PosicaoAtual[i]) +
                        _C2 * Util.Random.NextDouble() * (_GBest.MelhorPosicao[i] - _ParticulaFocal.PosicaoAtual[i]);
                        break;
                        case ETipoAtualizacaoVelocidade.Clerc:
                            _ParticulaFocal.Velocidade[i] = _W * (_ParticulaFocal.Velocidade[i] +
                        _C1 * Util.Random.NextDouble() * (_ParticulaFocal.MelhorPosicao[i] - _ParticulaFocal.PosicaoAtual[i]) +
                        _C2 * Util.Random.NextDouble() * (_GBest.MelhorPosicao[i] - _ParticulaFocal.PosicaoAtual[i]));
                        break;                       
                    }                    

                    if (_ParticulaFocal.Velocidade[i] > VMax)
                        _ParticulaFocal.Velocidade[i] = VMax;
                    else if (_ParticulaFocal.Velocidade[i] < -VMax)
                        _ParticulaFocal.Velocidade[i] = -VMax;
                }

                // Movimentação de demais partículas baseando-se em _GBest porque precisam se movimentar de acordo com o que a focal fez.
                foreach (Particula particula in _Particulas)
                {
                    for (int i = 0; i < particula.Velocidade.Length; i++)
                    {
                        particula.Velocidade[i] = _W * particula.Velocidade[i] +
                            _C1 * Util.Random.NextDouble() * (particula.MelhorPosicao[i] - particula.PosicaoAtual[i]) +
                            _C2 * Util.Random.NextDouble() * (_GBest.MelhorPosicao[i] - particula.PosicaoAtual[i]);

                        if (particula.Velocidade[i] > VMax)
                            particula.Velocidade[i] = VMax;
                        else if (particula.Velocidade[i] < -VMax)
                            particula.Velocidade[i] = -VMax;
                    }
                }
            }
            #endregion
            #region Global ou Local
            else
            {
                foreach (Particula particula in _Particulas)
                {
                    // _GBest será atualizando a cada movimento de partícula
                    if (_TipoTopologia == ETipoTopologia.Global)
                    {
                        _Particulas.Sort(delegate(Particula i1, Particula i2) { return i1.Fitness.CompareTo(i2.Fitness); });
                        _GBest = _Particulas[0];
                    }

                    for (int i = 0; i < particula.Velocidade.Length; i++)
                    {
                        particula.Velocidade[i] = _W * particula.Velocidade[i] +
                            _C1 * Util.Random.NextDouble() * (particula.MelhorPosicao[i] - particula.PosicaoAtual[i]) +
                            _C2 * Util.Random.NextDouble() * (_GetMelhorPosicaoGlobal(particula.Indice, i) - particula.PosicaoAtual[i]);

                        if (particula.Velocidade[i] > VMax)
                            particula.Velocidade[i] = VMax;
                        else if (particula.Velocidade[i] < -VMax)
                            particula.Velocidade[i] = -VMax;
                    }
                }
            }
            #endregion
        }

        private void _AtualizarPosicaoParticulas()
        {
            foreach (Particula particula in _Particulas)
            {
                for (int i = 0; i < particula.PosicaoAtual.Length; i++)
                {
                    particula.PosicaoAtual[i] += particula.Velocidade[i];
                    if (particula.PosicaoAtual[i] > PosMax)
                    {
                        particula.PosicaoAtual[i] = PosMax;
                        particula.Velocidade[i] = -particula.Velocidade[i];
                    }
                    else if (particula.PosicaoAtual[i] < PosMin)
                    {
                        particula.PosicaoAtual[i] = PosMin;
                        particula.Velocidade[i] = -particula.Velocidade[i];
                    }
                    else
                    {
                        particula.PosicaoAtual[i] = Math.Round(particula.PosicaoAtual[i], 0);
 
                    }
                }               
            }
        }

        private void _AvaliarParticulas()
        {
            foreach (Particula p in _Particulas)
            {
                //_Rastringin(p);
                _ELM(p);
                p.AtualizarMelhorPosicaoParticula();
            }
        }

        public Particula Executar(TextWriter notificador)
        {
            notificador.WriteLine("Fitness Avaliacoes");

            foreach (Particula p in _Particulas)
            {
                //_Rastringin(p);
                _ELM(p);
                p.AtualizarMelhorPosicaoParticula(true);
            }

            do
            {
                _AtualizarVelocidadeParticulas();

                _AtualizarPosicaoParticulas();

                _AvaliarParticulas();

                if ((double)count / 100.0 > 1)
                {
                    _Particulas.Sort(delegate(Particula i1, Particula i2) { return i1.MelhorFitness.CompareTo(i2.MelhorFitness); });
                    _GBest = _Particulas[0];

                    Console.WriteLine(_GBest.MelhorFitness.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + " " + count);
                    foreach (Particula part in _Particulas)
                        notificador.WriteLine(part.MelhorFitness.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))  + " " + count);
                }

                if (count > _NumeroMaximoAvaliacoesFuncao)
                    break;
            }
            while (true);
                
            return _GBest;
        }
    }
}
