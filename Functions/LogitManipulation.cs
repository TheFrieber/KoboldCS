using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koboldcs.Funcs
{
    public static class LogitManipulation
    {
        // Convert user-friendly percentage (0 to 100%) into logit bias
        public static float PercentageToLogitBias(float percentage)
        {
            // Ensure percentage is between 0 and 100
            if (percentage <= 0f)
                return float.NegativeInfinity; // 0% -> negative infinity (no chance for the token)
            if (percentage == 0.5f)
                return 0f; // Return default, no additions
            if (percentage == 1f)
                return float.PositiveInfinity; // 100% -> no bias (normal probability)

            // Convert percentage to a fraction (e.g., 50% -> 0.5)
            float p = percentage;

            // Apply the logit bias formula: ln(p / (1 - p))
            float raw_score = (float)Math.Log(p / (1f - p));

            return raw_score;
        }
    }
}
