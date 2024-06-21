using MathNet.Numerics.Distributions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionLibrary.Equity
{
    internal class CustomMC
    {
        public static double custom(Option option, Stock stock)
        {
            Random rand = new Random();
            double equityMarketRiskPremium = 0.05;
            double expectedStockReturn = option.RiskFreeRate + (stock.Beta * equityMarketRiskPremium);
            double dailyVolatility = stock.Volatility / Math.Sqrt(365);
            double expDailyReturn = Math.Pow(1 + expectedStockReturn, 1.0 / 365.0) - 1;
            int numSims = 50000;
            double[] simmedValues = new double[numSims];

            for (int i = 0; i < numSims; i++)
            {
                double simulatedPrice = option.Spot;
                for (int j = 0; j < option.DaysUntilExpiry; j++)
                {
                    double dailyReturn = expDailyReturn + dailyVolatility * Normal.InvCDF(0, 1, rand.NextDouble());
                    simulatedPrice *= (1 + dailyReturn);
                }
                simmedValues[i] = simulatedPrice;
            }
            double payoffSum = 0;

            foreach (double simPrice in simmedValues)
            {
                if (option.PutCall == 'C')
                {
                    payoffSum += Math.Max(0, simPrice - option.Strike);
                }
                else if (option.PutCall == 'P')
                {
                    payoffSum += Math.Max(0, option.Strike - simPrice);
                }
            }

            // Discount the average payoff back to the present value
            double averagePayoff = payoffSum / numSims;
            double optionPrice = Math.Exp(-option.RiskFreeRate * option.YearsUntilExpiry) * averagePayoff;

            return optionPrice;

        }
    }
}
