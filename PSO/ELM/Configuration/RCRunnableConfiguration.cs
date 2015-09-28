using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataManagement;

namespace ReservoirComputing.Configuration
{
    public class RCRunnableConfiguration : Configuration
    {
        public RCRunnableConfiguration(double[][] w, double[][] B, double[][] I, double[] H0, Data[] dataSet,
            int hidenNodesNumber, ERCActivationFunctionType activationFunctionType)
            : base(hidenNodesNumber, activationFunctionType)
        {
            this.w = w;
            this.B = B;
            this.I = I;
            this.H0 = H0;
            DataSet = dataSet;
        }

        public double[][] w { get; set; }
        public double[][] B { get; set; }
        public double[][] I { get; set; }
        public double[] H0 { get; set; }
        public Data[] DataSet { get; set; }
    }
}
