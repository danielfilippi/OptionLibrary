using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace OptionLibrary.Equity
{
    internal class GBM
    {
        /*public static double MonteCarloWithDividends(Option option, Stock stock)
        {
            Random rand = new Random();
            double equityMarketRiskPremium = 0.05; //https://indialogue.io/clients/reports/public/5d9da61986db2894649a7ef2/5d9da63386db2894649a7ef5
            double expectedStockReturn = option.RiskFreeRate + (stock.Beta * equityMarketRiskPremium);
            double dailyVolatility = stock.Volatility / Math.Sqrt(365);
            double expDailyReturn = expectedStockReturn / 365;
            int numSims = 10000;
            double[] simmedValues = new double[numSims];

            *//*for (int i = 0; i < numSims; i++) //for european, just for now
            {
                double simulatedPrice = option.Spot;
                for (int j = 0; j < option.DaysUntilExpiry; j++)
                {
                    double percentile = rand.NextDouble();
                    double dailyReturn = Normal.InvCDF(expDailyReturn, dailyVolatility, percentile);
                    simulatedPrice *= (1 + dailyReturn);
                    //Console.WriteLine($"Percentile: {percentile}, Daily Return: {dailyReturn}");
                    //Console.WriteLine(simulatedPrice);
                }
                simmedValues[i]= simulatedPrice;

            }*//*

            double dt = 1.0 / 365; // Daily time step

            for (int i = 0; i < numSims; i++) // For European options
            {
                double simulatedPrice = option.Spot;
                for (int j = 0; j < option.DaysUntilExpiry; j++)
                {
                    double Z = Normal.Sample(rand, 0, 1); // Generate a standard normal variable
                    simulatedPrice *= Math.Exp((expDailyReturn - 0.5 * Math.Pow(dailyVolatility, 2)) * dt + dailyVolatility * Math.Sqrt(dt) * Z);
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
        }*/

        public static double GBMWithDividends(Option option, int numSimulations)
        {
            Random rand = new Random();
            int Steps = (int)option.DaysUntilExpiry * 5;
            double dt = option.YearsUntilExpiry / Steps;
            double sqrtDt = Math.Sqrt(dt);
            double discountFactor = Math.Exp(-option.RiskFreeRate * option.YearsUntilExpiry);

            double payoffSum = 0.0;

            for (int i = 0; i < numSimulations; i++)
            {
                double S = option.Spot;
                for (int j = 0; j < Steps; j++)
                {
                    double dW = sqrtDt * rand.NextGaussian();
                    S *= Math.Exp((option.RiskFreeRate - option.DividendYield - 0.5 * Math.Pow(option.ImpliedVolatility, 2)) * dt + option.ImpliedVolatility * dW);
                }

                if (option.PutCall == 'C')
                {
                    payoffSum += Math.Max(0, S - option.Strike);
                }
                else if (option.PutCall == 'P')
                {
                    payoffSum += Math.Max(0, option.Strike - S);
                }
            }

            double optionPrice = discountFactor * payoffSum / numSimulations;
            return optionPrice;
        }
    }
    public static class RandomExtensions
    {
        // Generate a normally distributed random number using the Box-Muller transform
        public static double NextGaussian(this Random rand)
        {
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
    }
}
