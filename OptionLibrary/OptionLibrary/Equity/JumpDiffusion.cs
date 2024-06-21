using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearRegression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace OptionLibrary.Equity
{
    internal class JumpDiffusion
    {
        /*public static double CalculateOptionPrice(Option option, double lambda, double jumpMean, double jumpStdDev, int simulations, int steps)
        {
            double dt = option.YearsUntilExpiry / steps;
            Random rand = new Random();

            double NormalRandom(Random rand)
            {
                // Using Box-Muller transform to generate normal random variable
                double u1 = 1.0 - rand.NextDouble();
                double u2 = 1.0 - rand.NextDouble();
                return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            }

            double PoissonRandom(double lambda, Random rand)
            {
                double L = Math.Exp(-lambda);
                int k = 0;
                double p = 1.0;
                do
                {
                    k++;
                    p *= rand.NextDouble();
                }
                while (p > L);
                return k - 1;
            }

            double[] SimulatePath(double S0, double T, int steps, double mu, double sigma, double lambda, double jumpMean, double jumpStdDev)
            {
                double[] path = new double[steps + 1];
                path[0] = S0;

                for (int i = 1; i <= steps; i++)
                {
                    double dW = Math.Sqrt(dt) * NormalRandom(rand);
                    double dN = PoissonRandom(lambda * dt, rand);
                    double jump = (dN > 0) ? Math.Exp(jumpMean + jumpStdDev * NormalRandom(rand)) : 1.0;
                    path[i] = path[i - 1] * Math.Exp((mu - 0.5 * sigma * sigma) * dt + sigma * dW) * jump;
                }

                return path;
            }

            double sumPayoff = 0.0;
            for (int i = 0; i < simulations; i++)
            {
                double[] path = SimulatePath(option.Spot, option.YearsUntilExpiry, steps, option.RiskFreeRate - option.DividendYield, option.ImpliedVolatility, lambda, jumpMean, jumpStdDev);
                double ST = path[steps];
                double payoff = (option.PutCall == 'C') ? Math.Max(ST - option.Strike, 0) : Math.Max(option.Strike - ST, 0);
                sumPayoff += payoff;
            }

            return Math.Exp(-option.RiskFreeRate * option.YearsUntilExpiry) * sumPayoff / simulations;
        }*/

        public static double JumpDiffusionMonteCarlo(Option option, double numSimulations, double jumpIntensityPerYear, double avgJumpSize, double jumpSizeVolatility)
        {
            //Geomtric brownian motion, but for every step there is a chance a jump may occur. The nature of this jump is defined by the intensity, the avg size and the volatility of this size
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

                    if (rand.NextDouble() < jumpIntensityPerYear * dt)
                    {
                        double jumpSize = Math.Exp(avgJumpSize + jumpSizeVolatility * rand.NextGaussian()) - 1;
                        S *= (1 + jumpSize);  
                    }

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
        public static double JumpDiffusionBinomial(Option option, double jumpsPerYear, double avgJumpSize, double jumpSizeVolatility)
        {
            int Steps = (int)option.DaysUntilExpiry * 5;
            double TimePerStep = option.YearsUntilExpiry / Steps;

            double upvalue = Math.Exp(option.ImpliedVolatility * Math.Sqrt(TimePerStep));
            double downvalue = 1 / upvalue;
            double p = (Math.Exp((option.RiskFreeRate - option.DividendYield) * TimePerStep) - downvalue) / (upvalue - downvalue);
            double discountFactor = Math.Exp(-option.RiskFreeRate * TimePerStep);
            double pJump = 1 - Math.Exp(-jumpsPerYear * TimePerStep); 
            //Console.WriteLine("pJump: " + pJump);

            Random rand = new Random();

            double[,] Tree = new double[Steps + 1, Steps + 1];
            Tree[0, 0] = option.Spot;


            //BUILD TREE
            double numJ = 0;

            for (int i = 1; i <= Steps; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    double basePrice = (j == 0) ? Tree[i - 1, j] * upvalue : Tree[i - 1, j - 1] * downvalue;
                    Tree[i, j] = basePrice;

                    // Add jump logic

                    if (rand.NextDouble() < pJump)
                    {
                        double jumpMultiplier = Math.Exp(avgJumpSize + jumpSizeVolatility * rand.NextGaussian());
                        //Console.WriteLine("Jump multiplier: " + jumpMultiplier);
                        Tree[i, j] *= jumpMultiplier;
                        numJ++;
                    }
                }
            }
            Console.WriteLine(numJ);
            //CALCULATE PAYOFF AT TERMINAL NODES

            double[,] ReversedTree = new double[Steps + 1, Steps + 1];
            for (int i = 0; i <= Steps; i++)
            {
                switch (option.PutCall)
                {
                    case 'P': 
                        ReversedTree[Steps, i] = Math.Max(0, option.Strike - Tree[Steps, i]);
                        break;
                    case 'C': 
                        ReversedTree[Steps, i] = Math.Max(0, Tree[Steps, i] - option.Strike);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid option type: " + option.PutCall);
                }
            }


            //FIND OPTION VALUE

            for (int i = Steps - 1; i >= 0; i--)
            {
                for (int j = 0; j <= i; j++)
                {
                    double holdValue = discountFactor * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]);
                    if (option.EuroAme == 'E')
                        ReversedTree[i, j] = holdValue;
                    else if (option.EuroAme == 'A')
                    {
                        if (option.PutCall == 'P')
                            ReversedTree[i, j] = Math.Max(option.Strike - Tree[i, j], holdValue);
                        else if (option.PutCall == 'C')
                            ReversedTree[i, j] = Math.Max(Tree[i, j] - option.Strike, holdValue);
                    }
                }
            }
            return ReversedTree[0, 0];
        }
        public static double JDBAVG(Option option, double jumpsPerYear, double avgJumpSize, double jumpSizeVolatility)
        {
            double sum = 0;
            for (int i=0; i<100; i++)
            {
                sum += JumpDiffusionBinomial(option, jumpsPerYear, avgJumpSize, jumpSizeVolatility);
            }
            return sum / 100;

        }

        
    }
}
