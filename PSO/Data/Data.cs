using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataManagement
{
    /// <summary>
    /// Classe básica de dados.
    /// Encapsula o par entrada-saída.
    /// </summary>
    public class Data
    {
        #region Constructor(s)
        public Data(double[] realInput, double[] realOutput)
        {
            RealInput = realInput;
            RealOutput = realOutput;
            Input = realInput.Clone() as double[];
            Output = realOutput.Clone() as double[];
        }
        #endregion

        #region Properties
        public double[] Input { get; set; }
        public double[] Output { get; set; }
        public double[] RealInput { get; private set; }
        public double[] RealOutput { get; private set; }
        #endregion

        public Data Clone()
        {
            Data d = new Data(this.RealInput.Clone() as double[], this.RealOutput.Clone() as double[]);
            
            d.Input = this.Input.Clone() as double[];
            d.Output = this.Output.Clone() as double[];
            
            return d;
        }
    }
} 