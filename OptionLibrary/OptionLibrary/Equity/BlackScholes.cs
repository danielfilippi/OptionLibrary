using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionLibrary.Equity
{
    internal class BlackScholes
    {
        public static double BlackScholesWithDividends(Option option)
        {
            //if(option.EuroAme!='E') Console.WriteLine("Warning: This is not a European option. Black-Scholes will NOT give you a totally accurate price.");
            //if (option.EuroAme != 'E') throw new Exception("Option of type: " + option.EuroAme + " is incompatible with the Black-Scholes model.");
            double d1 = (Math.Log(option.Spot / option.Strike) +
                     (option.RiskFreeRate - option.DividendYield +
                     0.5 * Math.Pow(option.ImpliedVolatility, 2)) *
                     option.YearsUntilExpiry) /
                     (option.ImpliedVolatility * Math.Sqrt(option.YearsUntilExpiry));
            double d2 = d1 - option.ImpliedVolatility * Math.Sqrt(option.YearsUntilExpiry);

            double dividendDiscountFactor = Math.Exp(-option.DividendYield * option.YearsUntilExpiry);
            double strikeDiscountFactor = Math.Exp(-option.RiskFreeRate * option.YearsUntilExpiry);

            if (option.PutCall == 'C')
                return (option.Spot * dividendDiscountFactor * CND(d1)) -
                       (option.Strike * strikeDiscountFactor * CND(d2));
            else
                return (option.Strike * strikeDiscountFactor * CND(-d2)) -
                       (option.Spot * dividendDiscountFactor * CND(-d1));

        }
        private static double CND(double x) //cumulative normal distribution
        {
            double L = Math.Abs(x);
            double K = 1 / (1 + 0.2316419 * L);
            double w = 1 - 1 / Math.Sqrt(2 * Math.PI) * Math.Exp(-L * L / 2) *
                       (K * (0.31938153 + K * (-0.356563782 + K * (1.781477937 + K * (-1.821255978 + 1.330274429 * K)))));
            return x < 0 ? 1.0 - w : w;
        }
    }
}
