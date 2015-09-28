using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLP.Evaluation
{
    [Flags]
    public enum EMLPEvaluationInfo
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
