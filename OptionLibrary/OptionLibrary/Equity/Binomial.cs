using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionLibrary.Equity
{
    internal class Binomial
    {
        public static double BinomialWithDividends(Option option)
        {
            int Steps = (int)option.DaysUntilExpiry * 5;
            double TimePerStep = option.YearsUntilExpiry / Steps;//More like TimeInDaysPerStep - 5 steps per day
            double upvalue = Math.Exp(option.ImpliedVolatility * Math.Sqrt(TimePerStep));
            double downvalue = 1 / upvalue;
            double p = (Math.Exp((option.RiskFreeRate - option.DividendYield) * TimePerStep) - downvalue) / (upvalue - downvalue);
            double discountFactor = Math.Exp(-option.RiskFreeRate * TimePerStep);

            //BUILD TREE

            double[,] Tree = new double[Steps + 1, Steps + 1];
            Tree[0, 0] = option.Spot;
            for (int i = 1; i <= Steps; i++)
            {
                Tree[i, 0] = Tree[i - 1, 0] * upvalue;
                for (int j = 1; j <= i; j++)
                {
                    Tree[i, j] = Tree[i - 1, j - 1] * downvalue;
                }
            }

            //CALC OPTION VALUE AT EACH FINAL STEP

            double[,] ReversedTree = new double[Steps + 1, Steps + 1];
            for (int i = 0; i <= Steps; i++)
            {
                if (option.PutCall == 'P') ReversedTree[Steps, i] = Math.Max(0, option.Strike - Tree[Steps, i]);
                else if (option.PutCall == 'C') ReversedTree[Steps, i] = Math.Max(0, Tree[Steps, i] - option.Strike);
            }

            //FIND OPTION PRICE AT ROOT NODE

            for (int i = Steps - 1; i >= 0; i--)
            {
                for (int j = 0; j <= Steps - 1; j++)
                {
                    if (option.EuroAme == 'E') ReversedTree[i, j] = discountFactor * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]);
                    else if (option.EuroAme == 'A')
                    {
                        double holdValue = discountFactor * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]);

                        if (option.PutCall == 'P') ReversedTree[i, j] = Math.Max(option.Strike - Tree[i, j], holdValue);
                        else if (option.PutCall == 'C') ReversedTree[i, j] = Math.Max(Tree[i, j] - option.Strike, holdValue);
                    }
                }
            }
            return ReversedTree[0, 0];
        }
    }
}
