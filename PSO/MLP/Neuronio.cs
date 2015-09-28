using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLP
{
    public class Neuronio
    {

        private Double valor;
        private double[] pesosAtuais;
        private double[] pesosAntigos;

        //    private Random geradorPesos;
        public Neuronio()
        {
        }

        public Neuronio(double valor)
        {
            this.valor = valor;
        }

        public double[] getPesos()
        {
            return pesosAtuais;
        }

        public void setPesos(double[] pesos)
        {
            this.pesosAntigos = this.copiarArray(this.pesosAtuais);//this.pesosAtuais.clone();
            this.pesosAtuais = pesos;

        }

        public double[] getPesosAntigos()
        {
            return pesosAntigos;
        }

        public void setPesosAntigos(double[] pesosAntigos)
        {

            this.pesosAntigos = pesosAntigos;
        }

        public Double getValor()
        {
            return valor;
        }

        public void setValor(Double valor)
        {
            this.valor = valor;
        }

        public string imprimirPesos()
        {
            string retorno = string.Empty;
            retorno += "[ ";
            for (int i = 0; i < this.pesosAtuais.Length; i++)
            {
                retorno += "" + this.pesosAtuais[i] + ", ";
            }
            retorno += "]";

            return retorno;
        }

        public void gerarPesosAleatorios(int qtd, Random rand)
        {
            this.pesosAtuais = new double[qtd];
            this.pesosAntigos = new double[qtd];
            double val;
            
            for (int i = 0; i < qtd; i++)
            {
                //do
                //{
                //    val = rand.NextDouble();
                //} while (val > 0.5);

                //if (rand.NextDouble() <= 0.5)
                //    val = -(val);

                //this.pesosAtuais[i] = val;
                this.pesosAtuais[i] = 1 - rand.NextDouble() * 2;
                this.pesosAntigos[i] = 0.0;
            }
        }

        public void atualizarPesos(double[] deltas, double alpha, double beta)
        {
            double[] temp = this.copiarArray(this.pesosAtuais);//this.pesosAtuais.clone();
            double peso;
            for (int i = 0; i < this.pesosAtuais.Length; i++)
            {
                peso = temp[i] + (alpha * deltas[i] * this.valor) + (beta * (temp[i] - this.pesosAntigos[i]));
                this.pesosAtuais[i] = peso;
            }
            this.pesosAntigos = this.copiarArray(temp);

        }

        public Neuronio copy()
        {
            Neuronio retorno = new Neuronio();
            retorno.setPesos(this.copiarArray(this.pesosAtuais));
            retorno.setPesosAntigos(this.copiarArray(this.pesosAntigos));
            retorno.setValor(this.getValor());
            return retorno;
        }

        private double[] copiarArray(double[] dados)
        {
            if (dados != null)
            {
                double[] copia = new double[dados.Length];
                for (int i = 0; i < dados.Length; i++)
                {
                    copia[i] = dados[i];
                }
                return copia;
            }
            else
            {
                return null;
            }
        }

        //private double arredondar(double valor)
        //{
        //    DecimalFormat formato = new DecimalFormat("0.0000000000");
        //    Double retorno = Double.parseDouble((formato.format(valor)).replace(",", "."));
        //    return retorno;
        //}

    }
}
