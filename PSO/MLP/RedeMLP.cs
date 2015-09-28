using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataManagement;

namespace MLP
{
    public class RedeMLP
    {
        public double melhorTaxaDeAcerto = 0;
        public int melhorQtdEscondidos;
        public double melhorAlpha;
        public double melhorBeta; 

        public double melhorEMA;
        public double melhorEMAN = Double.MaxValue;
        public double melhorEMQ;
        public double melhorEPMA = Double.MaxValue;

        public double mediaEMAs;
        public double somaEMAs = 0;
        public double mediaEMANs;
        public double somaEMANs = 0;
        public double mediaEMQs;
        public double somaEMQs = 0;
        public double mediaEPs;
        public double somaEPs = 0;
        public double mediaEPMAs;
        public double somaEPMAs = 0;

        public double mediaTaxaDeAcerto;
        public double somaTaxasDeAcerto = 0;


        DataProvider prov;
        public double[][] entradasTreino;
        public double[][] saidasTreino;
        public double[][] entradasValidacao;
        public double[][] saidasValidacao;
        public double[][] entradasTeste;
        public double[][] saidasTeste;
        public double[][] saidasCalculadasTeste;
        int maxCiclos;
        public int qtdEscondidos;
        double alpha;
        double beta;
        double EMI;
        double EMQ;
        double EMIV;
        double EMQV;
        double EPA;
        double EPMA;
        double EMA;
        double EMAN;

        public static double PotenciaInstalada;
        List<List<Neuronio>> neuronios;
        List<List<double>> erros;//0-erros do treinamento, 1-erros da validacao
        List<List<double>> reaisprevistos;//0-reais, 1-previstos
        List<List<Neuronio>> melhorCaso;
        int qtdAcerto;
        public int Ciclo;
        double melhorEMQRede;
        EnumTipoExecucao tipoExecucao;
        String algoritmo;
        int _Seed = -1;
        Random _Rand;

        public RedeMLP(DataProvider prov, double[][] entradasTreino, double[][] saidasTreino, double[][] entradasValidacao, double[][] saidasValidacao, double alpha, double beta, int maxCiclos, int qtdEscondidos, EnumTipoExecucao tipoExecucao, String algoritmo, int seed)
        {
            this.prov = prov;
            this.entradasTreino = entradasTreino;
            this.saidasTreino = saidasTreino;
            this.entradasValidacao = entradasValidacao;
            this.saidasValidacao = saidasValidacao;
            this.alpha = alpha;
            this.beta = beta;
            this.maxCiclos = maxCiclos;
            this.qtdEscondidos = qtdEscondidos;
            this.neuronios = new List<List<Neuronio>>();
            this.melhorCaso = new List<List<Neuronio>>();
            this.EMI = 0.0;
            this.EMQ = 0.0; // antes estava 10 aqui, não sei o motivo, não é necessário
            this.melhorEMQRede = 10.0;
            this.erros = new List<List<double>>();
            this.erros.Add(new List<double>());//array de erros de treinamento
            this.erros.Add(new List<double>());//array de erros de validacao
            this.reaisprevistos = new List<List<double>>(); ;
            this.reaisprevistos.Add(new List<double>());//array de valores reais
            this.reaisprevistos.Add(new List<double>());//array de valores previstos

            this.qtdAcerto = 0;
            this.Ciclo = 0;
            this.tipoExecucao = tipoExecucao;
            this.EPA = 0.0;
            this.EPMA = 0.0;
            this.algoritmo = algoritmo;
            this._Seed = seed;
            this._Rand = new Random(_Seed);
            
            if (this.algoritmo.Equals(Algoritmos.BACKPROPAGATION))
            {
                //Console.WriteLine(">>> BACKPROPAGATION <<<");
                this.initBP();
            }
            else if (this.algoritmo.Equals(Algoritmos.CLANAPSO))
            {
                //Console.WriteLine(">>> ClanAPSO <<<");
                this.initClanAPSO();
            }
            else
            {
                //Console.WriteLine("ALGORITMO INEXISTENTE");
            }
        }

        public List<List<double>> getErros()
        {
            return erros;
        }

        public List<List<double>> getReaisPrevistos()
        {
            return reaisprevistos;
        }

        public EnumTipoExecucao getTipoExecucao()
        {
            return tipoExecucao;
        }

        //modelo para iniciar a rede
        private void initClanAPSO()
        {
            List<Neuronio> camadaEntrada = new List<Neuronio>();
            //        double[] pesosTempEntrada = new double[this.qtdEscondidos];
            //        double[] pesosTempEscondida = new double[this.saidasTreino[0].Length];
            //        for (int i = 0; i < pesosTempEntrada.Length; i++) {
            //            pesosTempEntrada[i] = 0.0;
            //        }
            //
            //        for (int i = 0; i < pesosTempEscondida.Length; i++) {
            //            pesosTempEscondida[i] = 0.0;
            //        }

            camadaEntrada.Add(new Neuronio(1.0));//inserindo o Bias

            for (int i = 0; i < this.entradasTreino[0].Length; i++)
            {
                camadaEntrada.Add(new Neuronio());
            }

            List<Neuronio> camadaEscondida = new List<Neuronio>();

            camadaEscondida.Add(new Neuronio(1.0));//inserindo o Bias

            for (int i = 0; i < this.qtdEscondidos; i++)
            {
                camadaEscondida.Add(new Neuronio());
            }

            List<Neuronio> camadaSaida = new List<Neuronio>();
            for (int i = 0; i < this.saidasTreino[0].Length; i++)
            {
                camadaSaida.Add(new Neuronio());
            }

            this.neuronios.Add(camadaEntrada);
            this.neuronios.Add(camadaEscondida);
            this.neuronios.Add(camadaSaida);

            // Tinha esse definirMelhorCaso() mas não é necessário
            //this.definirMelhorCaso();
        }

        //inicio da rede para rodar com BackPropagation
        private void initBP()
        {
            List<Neuronio> camadaEntrada = new List<Neuronio>();

            camadaEntrada.Add(new Neuronio(1.0));//inserindo o Bias

            for (int i = 0; i < this.entradasTreino[0].Length; i++)
            {
                camadaEntrada.Add(new Neuronio(0.0));
                camadaEntrada[i].gerarPesosAleatorios(this.qtdEscondidos, _Rand);
            }

            camadaEntrada[this.entradasTreino[0].Length].gerarPesosAleatorios(this.qtdEscondidos, _Rand);

            List<Neuronio> camadaSaida = new List<Neuronio>();
            for (int i = 0; i < this.saidasTreino[0].Length; i++)
            {
                camadaSaida.Add(new Neuronio());//SE FOR NECESSARIO IMPRIMIR OS PESOS Ã‰ PRECISO CRIAR NULOS
            }

            List<Neuronio> camadaEscondida = new List<Neuronio>();
            camadaEscondida.Add(new Neuronio(1.0));//inserindo o Bias

            for (int i = 0; i < this.qtdEscondidos; i++)
            {
                camadaEscondida.Add(new Neuronio(0.0));
                camadaEscondida[i].gerarPesosAleatorios(camadaSaida.Count, _Rand);
            }

            camadaEscondida[this.qtdEscondidos].gerarPesosAleatorios(camadaSaida.Count, _Rand);

            this.neuronios.Add(camadaEntrada);
            this.neuronios.Add(camadaEscondida);
            this.neuronios.Add(camadaSaida);

            // Tinha esse definirMelhorCaso() mas não é necessário
            //this.definirMelhorCaso();

            this.treinarBP();
        }

        private void definirMelhorCaso()
        {
            this.melhorCaso = new List<List<Neuronio>>();
            this.melhorEMQRede = this.EMQ;
            for (int i = 0; i < this.neuronios.Count; i++)
            {
                this.melhorCaso.Add(new List<Neuronio>());
                for (int j = 0; j < this.neuronios[i].Count; j++)
                {
                    this.melhorCaso[i].Add((Neuronio)this.neuronios[i][j].copy());
                }
            }
        }

        private double aplicarFuncaoAtivacao(double x)
        {//função tangente hiperbólica
            double y = 0.0;
            //y = (Math.Exp(2 * x) - 1) / (Math.Exp(2 * x) + 1);
            y = 1 / (1 + (Math.Pow(Math.E, -(x))));
            return y;
        }

        private double derivarFuncaoAtivavao(double y)
        {//derivada para a funcao sigmóide logística
            double retorno = 0.0;
            //retorno = 2 / (Math.Exp(2 * y) + 1);
            retorno = y * (1 - y);
            return retorno;
        }

        private void definirValores(double[] valores, List<Neuronio> camada)
        {
            if (valores.Length != camada.Count)
            {
                for (int i = 1; i < camada.Count; i++)
                {//Comeca em 1 para pular o bias que jÃ¡ estÃ¡ definido com 1.0
                    camada[i].setValor(valores[i - 1]);
                }
            }
            else
            {
                for (int i = 0; i < camada.Count; i++)
                {
                    camada[i].setValor(valores[i]);
                }
            }
        }

        private void aplicarFaseFoward(double[] entradas)
        {

            List<Neuronio> camadaEntrada = this.neuronios[0];
            List<Neuronio> camadaEscondida = this.neuronios[1];
            List<Neuronio> camadaSaida = this.neuronios[2];
            double somatorio = 0.0;
            double[] fnet = new double[this.qtdEscondidos];//tamanho de fnet é o tamanho da camada escondida -1(bias) = this.qtdEscondidos
            double[] fnetSaida = new double[camadaSaida.Count];
            Neuronio neuronio;
            this.definirValores(entradas, this.neuronios[0]);

            for (int i = 0; i < camadaEscondida.Count - 1; i++)
            {
                somatorio = 0.0;

                for (int j = 0; j < camadaEntrada.Count; j++)
                {
                    neuronio = camadaEntrada[j];
                    somatorio += (neuronio.getValor() * neuronio.getPesos()[i]);
                }

                fnet[i] = this.aplicarFuncaoAtivacao(somatorio);
            }

            this.definirValores(fnet, camadaEscondida);

            for (int m = 0; m < camadaSaida.Count; m++)
            {
                somatorio = 0.0;
                for (int n = 0; n < camadaEscondida.Count; n++)
                {
                    neuronio = camadaEscondida[n];
                    somatorio += (neuronio.getValor() * neuronio.getPesos()[m]);
                }
                fnetSaida[m] = this.aplicarFuncaoAtivacao(somatorio);
            }
            this.definirValores(fnetSaida, camadaSaida);
        }

        private void aplicarFaseBackPropagation(double[] desejados)
        {

            List<Neuronio> camadaEntrada = this.neuronios[0];
            List<Neuronio> camadaEscondida = this.neuronios[1];
            List<Neuronio> camadaSaida = this.neuronios[2];
            double[] deltaSaida = new double[this.neuronios[2].Count];
            double[] deltaEscondida = new double[this.qtdEscondidos];
            double erro = 0.0;
            Neuronio neuronio;
            this.EMI = 0.0;
            for (int i = 0; i < camadaSaida.Count; i++)
            {
                neuronio = camadaSaida[i];
                erro = desejados[i] - neuronio.getValor();
                this.EMI += Math.Pow(erro, 2.0);
                deltaSaida[i] = (this.derivarFuncaoAtivavao(neuronio.getValor()) * erro);
            }
            this.EMQ += this.EMI / camadaSaida.Count;

            for (int j = 1; j < camadaEscondida.Count; j++)
            {
                erro = 0.0;
                neuronio = camadaEscondida[j];
                for (int k = 0; k < camadaSaida.Count; k++)
                {
                    erro += deltaSaida[k] * neuronio.getPesos()[k];
                }
                deltaEscondida[j - 1] = (this.derivarFuncaoAtivavao(neuronio.getValor()) * erro);
            }

            /*AJUSTE DOS PESOS*/
            ///AJUSTE CAMADA ESCONDIDA
            for (int m = 0; m < camadaEscondida.Count; m++)
            {
                camadaEscondida[m].atualizarPesos(deltaSaida, this.alpha, this.beta);
            }

            //AJUSTE CAMADA ENTRADA
            for (int n = 0; n < camadaEntrada.Count; n++)
            {
                camadaEntrada[n].atualizarPesos(deltaEscondida, this.alpha, this.beta);
            }

            /*FIM DE AJUSTE DOS PESOS*/
        }

        public double treinarClanAPSO(double[] pesos)
        {
            double retorno = 0;

            this.EMQ = 0.0;
            List<Neuronio> camadaEntrada = this.neuronios[0];
            List<Neuronio> camadaEscondida = this.neuronios[1];
            List<Neuronio> camadaSaida = this.neuronios[2];
            /*Definindo os pesos da rede*/
            List<List<double[]>> listaPesos = this.tratarPesosClan(pesos);
            for (int i = 0; i < listaPesos[0][0].Length; i++)
            {
                // Console.WriteLine("listaPesos.get(0).get(0)[i]: "+listaPesos.get(0).get(0)[i]);
            }

            for (int i = 0; i < camadaEntrada.Count; i++)
            {
                //Console.WriteLine("pesosFora: " + listaPesos.get(0).get(i));
                camadaEntrada[i].setPesos(listaPesos[0][i]);
                //            Console.WriteLine("pesos NeuronioEntrada:"+camadaEntrada.get(i).imprimirPesos());
            }
            for (int i = 0; i < camadaEscondida.Count; i++)
            {
                camadaEscondida[i].setPesos(listaPesos[1][i]);
                //            Console.WriteLine("pesos NeuronioEscondida:"+camadaEscondida.get(i).imprimirPesos());
            }
            /*fim pesos*/

            Neuronio neuronio;
            double erro;
            for (int i = 0; i < this.entradasTreino.Length; i++)
            {
                this.aplicarFaseFoward(this.entradasTreino[i]);
                this.EMI = 0.0;
                for (int j = 0; j < camadaSaida.Count; j++)
                {
                    neuronio = camadaSaida[j];
                    erro = this.saidasTreino[i][j] - neuronio.getValor();
                    this.EMI += Math.Pow(erro, 2.0);

                }
                this.EMQ += this.EMI / camadaSaida.Count;
            }
            this.EMQ /= this.saidasTreino.Length;

            retorno = this.EMQ;
            this.definirMelhorCaso();
            return retorno;

        }

        private List<Double> convertToList(double[] pesos)
        {

            List<Double> retorno = new List<Double>();
            for (int i = 0; i < pesos.Length; i++)
            {
                retorno.Add(pesos[i]);
            }
            //        Console.WriteLine("retorno = " + retorno);
            return retorno;
        }

        private List<List<double[]>> tratarPesosClan(double[] pesos)
        {
            List<Neuronio> camadaEntrada = this.neuronios[0];
            List<Neuronio> camadaEscondida = this.neuronios[1];
            List<Neuronio> camadaSaida = this.neuronios[2];
            List<Double> pesosList = this.convertToList(pesos);

            List<List<double[]>> retorno = new List<List<double[]>>();
            int qtdPesosNeuronioEntrada = camadaEscondida.Count - 1;
            int qtdPesosNeuronioEscondido = camadaSaida.Count;

            double[] listaPesos;
            retorno.Add(new List<double[]>());
            for (int i = 0; i < camadaEntrada.Count; i++)
            {
                listaPesos = new double[qtdPesosNeuronioEntrada];
                for (int j = 0; j < qtdPesosNeuronioEntrada; j++)
                {
                    listaPesos[j] = pesosList[0];
                    pesosList.RemoveAt(0);//pega sempre o primeiro indice do array
                }
                retorno[0].Add(listaPesos);
            }
            retorno.Add(new List<double[]>());
            for (int i = 0; i < camadaEscondida.Count; i++)
            {
                listaPesos = new double[qtdPesosNeuronioEscondido];
                for (int j = 0; j < qtdPesosNeuronioEscondido; j++)
                {
                    listaPesos[j] = pesosList[0];
                    pesosList.RemoveAt(0);//pega sempre o primeiro indice do array
                }
                retorno[1].Add(listaPesos);
            }

            return retorno;
        }

        /**
         * Funcao para ralizar o calculo de erro para validacao do ClanApso
         */
        public double calcularErroValidacaoClan()
        {
            double retorno = 0;
            this.validarTreinamento();
            this.EMQV /= this.saidasValidacao.Length;
            retorno = this.EMQV;
            return retorno;
        }




        private double[] converterArray(List<Double> dados)
        {
            double[] retorno = new double[dados.Count];
            for (int i = 0; i < dados.Count; i++)
            {
                double item = dados[i];
                retorno[i] = item;

            }
            return retorno;

        }

        private void treinarBP()
        {
            bool flagTreino = true;
            bool flagDiferenca = false;
            bool flagAumento = false;
            int qtdAumentos = 0;
            int qtdDiferençaEstagnada = 0;

            //Console.WriteLine("Neurônios na camada escondida: " + this.qtdEscondidos);
           // Console.WriteLine("Alpha: " + this.alpha);
            //Console.WriteLine("Beta: " + this.beta);

            while (flagTreino)
            {

                for (int j = 0; j < this.entradasTreino.Length; j++)
                {
                    this.aplicarFaseFoward(this.entradasTreino[j]);
                    this.aplicarFaseBackPropagation(this.saidasTreino[j]);
                }

                this.validarTreinamento();

                // Por que fazer essas divisões aqui?
                this.EMQ /= this.saidasTreino.Length;
                this.EMQV /= this.saidasValidacao.Length;

                this.erros[0].Add(this.EMQ);
                this.erros[1].Add(this.EMQV);

                if (this.Ciclo > 0)
                {
                    double emqAtual = this.erros[1][this.erros[1].Count - 1];
                    double emqAnterior = this.erros[1][this.erros[1].Count - 2];
                    double diferenca = (emqAnterior - emqAtual);
                    diferenca = (diferenca / emqAnterior);

                    // Se chegar ao número máximo de ciclos, parar
                    if (this.Ciclo >= this.maxCiclos)
                    {

                        flagTreino = false;
                        if (emqAtual < this.melhorEMQRede)
                        {
                            this.definirMelhorCaso();
                        }
                    }
                    // Se a diferença entre os EMQs começar a estagnar, parar se/quando acontecer por 30 vezes
                    else if (Math.Abs(diferenca) < 0.001)
                    {
                        // Ativa a contagem de diferenças
                        if (!flagDiferenca)
                        {
                            qtdDiferençaEstagnada = 0;
                            flagDiferenca = true;

                            if (emqAtual < this.melhorEMQRede)
                            {
                                this.definirMelhorCaso();
                            }
                        }
                        qtdDiferençaEstagnada++;
                        if (qtdDiferençaEstagnada == 30)
                        {//contara 30 vezes consecutivas
                            flagTreino = false;
                        }

                    }
                    // Se o EMQ começar a aumentar, parar se/quando acontecer por 30 vezes
                    else if (emqAtual > emqAnterior)
                    {
                        // Ativa a contagem de aumentos
                        if (!flagAumento)
                        {
                            qtdAumentos = 0;
                            flagAumento = true;
                        }
                        qtdAumentos++;
                        if (qtdAumentos == 30)
                        {//contara 30 x consecutivas
                            flagTreino = false;
                        }
                    }                    
                    else
                    {

                        flagAumento = false;
                        flagDiferenca = false;

                        if (emqAtual < this.melhorEMQRede)
                        {
                            this.definirMelhorCaso();
                        }
                    }
                }
                this.Ciclo++;
                //Console.WriteLine("Ciclo: [" + (this.ciclo) + "]    >>> EMQ: " + this.EMQ);
                
                this.EMQ = 0.0;
                this.EMQV = 0.0;
                
            }

            //Console.WriteLine("Total de Ciclos: " + this.ciclo );           
        }

        private void validarTreinamento()
        {

            for (int i = 0; i < this.entradasValidacao.Length; i++)
            {
                this.aplicarFaseFoward(this.entradasValidacao[i]);
                this.calcularErroValidacao(this.saidasValidacao[i]);
            }
        }

        private void calcularErroValidacao(double[] desejados)
        {
            double erro = 0.0;
            double desejado = 0.0;
            double calculado = 0.0;
            this.EMIV = 0.0;
            for (int i = 0; i < desejados.Length; i++)
            {
                desejado = desejados[i];
                calculado = this.neuronios[this.neuronios.Count - 1][i].getValor();
                erro = desejado - calculado;
                this.EMIV += Math.Pow(erro, 2.0);
            }
            this.EMQV += this.EMIV / desejados.Length;
        }

        public void testar(double[][] entradasTeste, double[][] saidasTeste)
        {
            this.entradasTeste = entradasTeste;
            this.saidasTeste = saidasTeste;
            
            saidasCalculadasTeste = new double[saidasTeste.Length][];
            for (int i = 0; i < saidasCalculadasTeste.Length; i++)
            {
                saidasCalculadasTeste[i] = new double[saidasTeste[0].Length];
            }

            for (int i = 0; i < entradasTeste.Length; i++)
            {
                this.aplicarFaseFoward(entradasTeste[i]);
                saidasCalculadasTeste[i] = this.retornarSaidasPrevisao();
            }
        }

        private double[] retornarSaidasPrevisao()
        {
            List<Neuronio> camadaSaida = this.neuronios[2];
            double[] saidas = new double[camadaSaida.Count];

            for (int i = 0; i < camadaSaida.Count; i++)
            {
                saidas[i] = camadaSaida[i].getValor();
            }

            return saidas;
        }

        private void verificarPrevisaoEPMA(double[][] calculadas, double[][] desejadas)
        {

            // só o EPMA que faz em cima dos dados desnormalizados.
            this.EPA = 0.0;
            double erro;
            double denominador;
            for (int i = 0; i < calculadas.Length; i++)
            {

                //imprimirArray(calculadas[i], desejadas[i]);
                for (int j = 0; j < calculadas[0].Length; j++)
                {
                    erro = desejadas[i][j] - calculadas[i][j];

                    if (desejadas[i][j] == 0)
                        denominador = 0.000000001;
                    else
                        denominador = desejadas[i][j];

                    this.EPA += ((Math.Abs(erro) / denominador) * 100);
                }
            }

            this.EPMA = this.EPA / calculadas.Length;
           // Console.WriteLine("EPMA Previsão: " + this.EPMA);
        }

        private void verificarPrevisaoEMQ(double[][] calculadas, double[][] desejadas)
        {

            double erro;
            double[] errosMomentos = new double[calculadas[0].Length];
            this.EMI = 0;

            // Fiz dessa forma, guardando os erros num array para só depois fazer as
            // somas e dividir pela quantidade de itens no array (média dos erros),
            // para conseguir visualizar melhor os resultados. Na prática, dava para
            // fazer apenas somando todos os erros e dividindo por 
            // calculadas[0].Length * calculadas.Length no final.
            for (int i = 0; i < calculadas.Length; i++)
            {
                for (int j = 0; j < calculadas[0].Length; j++)
                {
                    erro = desejadas[i][j] - calculadas[i][j];
                    errosMomentos[j] += Math.Pow(erro, 2.0);
                    erro = 0;
                }
            }

            for (int i = 0; i < calculadas[0].Length; i++)
            {
                errosMomentos[i] = errosMomentos[i] / calculadas.Length;
                this.EMI += errosMomentos[i];
            }

            this.EMQ = this.EMI / calculadas[0].Length;
            //Console.WriteLine("EMQ Previsão: " + this.EMQ);
            //Console.WriteLine("EP (raiz do EMQ): " + Math.Sqrt(this.EMQ));
        }

        private void verificarPrevisaoNEMA(double[][] calculadas, double[][] desejadas)
        {

            double erro;
            double[] errosMomentos = new double[calculadas[0].Length];
            this.EMA = 0;

            // Fiz dessa forma, guardando os erros num array para só depois fazer as
            // somas e dividir pela quantidade de itens no array (média dos erros),
            // para conseguir visualizar melhor os resultados. Na prática, dava para
            // fazer apenas somando todos os erros e dividindo por 
            // calculadas[0].Length * calculadas.Length no final.
            for (int i = 0; i < calculadas.Length; i++)
            {
                for (int j = 0; j < calculadas[0].Length; j++)
                {
                    erro = desejadas[i][j] - calculadas[i][j];
                    errosMomentos[j] += Math.Abs(erro);
                    erro = 0;
                }
            }

            for (int i = 0; i < calculadas[0].Length; i++)
            {
                errosMomentos[i] = errosMomentos[i] / calculadas.Length;
                this.EMA += errosMomentos[i];
            }

            this.EMA = this.EMA / calculadas[0].Length;
            this.EMAN = this.EMA / PotenciaInstalada;
            //Console.WriteLine("EMA: " + this.EMA);
            //Console.WriteLine("NEMA: " + this.EMAN);
        }

        private double[] retornarSaidasClassificacao()
        {//O maior leva tudo
            List<Neuronio> camadaSaida = this.neuronios[2];
            double[] saidas = new double[camadaSaida.Count];
            int idMax = 0;
            double temp = camadaSaida[0].getValor();

            if (camadaSaida.Count > 1)
            {
                for (int i = 1; i < camadaSaida.Count; i++)
                {
                    if (camadaSaida[i].getValor() > temp)
                    {
                        temp = camadaSaida[i].getValor();

                        idMax = i;
                    }
                }
                for (int j = 0; j < saidas.Length; j++)
                {
                    if (j == idMax)
                    {//BUG QUANDO NAO HOUVER NORMALIZACAO DEVESSE MUDAR AS SAIDAS PARA 1.0 E 0.0
                        //saidas[j] = 1.0;
                        saidas[j] = 0.9;
                    }
                    else
                    {
                        //                    saidas[j] = 0.0;
                        saidas[j] = 0.1;
                    }
                }
            }
            else
            {
                if (temp < 0.4)
                {
                    saidas[0] = 0.1;
                }
                else
                {
                    saidas[0] = 0.9;
                }

            }

            return saidas;
        }

        private void verificarClassificacao(double[] calculado, double[] desejado)
        {
            int temp = 0;
            for (int i = 0; i < calculado.Length; i++)
            {
                if (calculado[i] == desejado[i])
                {
                    temp++;
                }
            }

            this.qtdAcerto += temp;

            // Da maneira como estava implementado, só contava como acerto se acertasse 100% do dia
            //if (temp == calculado.Length) {
            //    this.qtdAcerto++;
            //}
        }

        // transforma as matrizes de valores reais e valores calculados List<List<double>> para plotar o gráfico
        private void prepararDadosParaPlotarGraficoPrevisao(double[][] calculadas, double[][] desejadas)
        {
            int MaxIndex = calculadas[0].Length - 1;
            for (int i = 0; i < calculadas.Length; i++) //EDGAR TODO
            {
                //for (int j = 0; j < calculadas[0].Length; j++)
                {
                    this.reaisprevistos[0].Add(desejadas[i][MaxIndex]); // reais
                    this.reaisprevistos[1].Add(calculadas[i][MaxIndex]); // previstos
                }
            }
        }

        private void imprimirArray(double[] calculado, double[] desejado)
        {
            String impressaoCalculado = "calculado: ";
            String impressaoDesejado = "desejado : ";
            for (int i = 0; i < calculado.Length; i++)
            {
                impressaoCalculado += calculado[i] + ",";
                impressaoDesejado += desejado[i] + ",";
                //            if (calculado[i] == desejado[i]) {
                //                temp++;
                //            }
            }
            //        if (temp == calculado.Length) {
            //            this.qtdAcerto++;
            //            Console.WriteLine("OK");
            //        } else {
            //            Console.WriteLine("FAIL");
            //        }
           // Console.WriteLine(impressaoCalculado);
           // Console.WriteLine(impressaoDesejado);

        }

        private void imprimirPesos()
        {
            for (int i = 0; i < this.neuronios.Count - 1; i++)
            {
                for (int j = 0; j < this.neuronios[i].Count; j++)
                {
                    //Console.WriteLine("neuronio[" + i + "][" + j + "]: " + this.neuronios[i][j].imprimirPesos());
                }
            }
        }
    }
}
