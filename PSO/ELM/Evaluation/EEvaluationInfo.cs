using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReservoirComputing.Evaluation
{
    [Flags]
    public enum EEvaluationInfo
    {
        None = 0,

        EMQ = 1,

        RMSE = 2,

        SR = 4,

        EPMA = 8,

        DEV = 16,

        EvaluateLikeONS = 32,

        EPMAForPowerPlant = 64,
    }
}
