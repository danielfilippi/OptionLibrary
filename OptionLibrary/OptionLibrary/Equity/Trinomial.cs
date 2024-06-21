using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionLibrary.Equity
{
    internal class Trinomial
    {
        public static double TrinomialWithDividends(Option option)
        {
            int Steps = (int)option.DaysUntilExpiry * 5;
            double TimePerStep = option.YearsUntilExpiry / Steps;//More like TimeInDaysPerStep - 5 steps per day
             //double upvalue = Math.Exp(option.ImpliedVolatility * Math.Sqrt(TimePerStep));
            double upvalue = Math.Exp(option.ImpliedVolatility * Math.Sqrt(2 * TimePerStep));
            double downvalue = 1 / upvalue;
            double middlevalue = 1;
            // Calculate probabilities
            double e_rt2 = Math.Exp((option.RiskFreeRate - option.DividendYield) * TimePerStep / 2);
            double e_vol2 = Math.Exp(option.ImpliedVolatility * Math.Sqrt(TimePerStep / 2));
            double p_u = Math.Pow((e_rt2 - 1 / e_vol2) / (e_vol2 - 1 / e_vol2), 2);
            double p_d = Math.Pow((e_vol2 - e_rt2) / (e_vol2 - 1 / e_vol2), 2);
            double p_m = 1 - p_u - p_d;

            double discountFactor = Math.Exp(-option.RiskFreeRate * TimePerStep);


            //BUILD TREE
            double[,] Tree = new double[Steps + 1, 2 * Steps + 1];  // Adjust the size to accommodate max index `2 * Steps`
            Tree[0, Steps] = option.Spot;  // Start in the middle of the array

            for (int i = 0; i < Steps; i++)
            {
                for (int j = Math.Max(0, Steps - i); j <= Math.Min(2 * Steps, Steps + i); j++)
                {
                    if (j + 1 <= 2 * Steps) //bound checks
                    {  
                        Tree[i + 1, j + 1] = Tree[i, j] * upvalue;
                    }
                    Tree[i + 1, j] = Tree[i, j]; //middle value - removed "* middlevalue" since its useless
                    if (j - 1 >= 0)
                    {  
                        Tree[i + 1, j - 1] = Tree[i, j] * downvalue; 
                    }
                }
            }


            //CALC OPTION VALUE AT EACH FINAL STEP
            double[,] ReversedTree = new double[Steps + 1, 2 * Steps + 1];
            for (int i = 0; i <= 2*Steps; i++)
            {
                if (option.PutCall == 'P') ReversedTree[Steps, i] = Math.Max(0, option.Strike - Tree[Steps, i]);
                else if (option.PutCall == 'C') ReversedTree[Steps, i] = Math.Max(0, Tree[Steps, i] - option.Strike);
            }

            //FIND OPTION PRICE AT ROOT NODE
            for (int i = Steps - 1; i >= 0; i--)
            {
                for (int j = -i; j <= i; j++)
                {
                    int index = Steps + j;
                    if (option.EuroAme == 'E')
                    {
                        ReversedTree[i, index] = discountFactor *
                                                 (p_u * ReversedTree[i + 1, index + 1] +
                                                  p_m * ReversedTree[i + 1, index] +
                                                  p_d * ReversedTree[i + 1, index - 1]);
                    }
                    else if (option.EuroAme == 'A')
                    {
                        double holdValue = discountFactor *
                                           (p_u * ReversedTree[i + 1, index + 1] +
                                            p_m * ReversedTree[i + 1, index] +
                                            p_d * ReversedTree[i + 1, index - 1]);

                        if (option.PutCall == 'P')
                            ReversedTree[i, index] = Math.Max(option.Strike - Tree[i, index], holdValue);
                        else if (option.PutCall == 'C')
                            ReversedTree[i, index] = Math.Max(Tree[i, index] - option.Strike, holdValue);
                    }
                }
            }

            return ReversedTree[0, Steps];


        }

    }
}
