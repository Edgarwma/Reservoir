using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReservoirComputing.Configuration
{
    public abstract class Configuration
    {
        public Configuration(int hiddenNodesNumber, ERCActivationFunctionType activationFunctionType)
        {
            HidenNodesNumber = hiddenNodesNumber;
            ActivationFunctionType = activationFunctionType;
        }

        public int HidenNodesNumber { get; protected set; }
        public ERCActivationFunctionType ActivationFunctionType { get; protected set; }
    }
}