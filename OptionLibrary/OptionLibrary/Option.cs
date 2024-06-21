using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionLibrary
{
    internal class Option
    {
        //Nature
        public char EuroAme {  get; set; }
        public char PutCall { get; set; }
        //Metrics
        public double Strike { get; set; }
        public double Spot { get; set; }
        public double RiskFreeRate { get; set; }
        public double ImpliedVolatility { get; set; }
        public double DaysUntilExpiry { get; set; }
        public double YearsUntilExpiry { get; set; }
        public double DividendYield { get; set; }
        private double TheoreticalValue { get; set; }
        //Greeks
        private double Delta { get; set; }
        private double Gamma { get; set; }
        private double Theta { get; set; }
        private double Vega { get; set; }
        private double Rho { get; set; }

        public Option(char euroAme, char putCall, double strike, double spot, double riskFreeRate, double impliedVolatility, double daysUntilExpiry, double dividendYield)
        {
            EuroAme = euroAme;
            PutCall = putCall;
            Strike = strike;
            Spot = spot;
            RiskFreeRate = riskFreeRate;
            ImpliedVolatility = impliedVolatility;
            DaysUntilExpiry = daysUntilExpiry;
            YearsUntilExpiry = DaysUntilExpiry / 365.25;
            DividendYield = dividendYield;
        }
        public void SetTheoreticalValue(double value)
        {
            TheoreticalValue = value;
        }
        public double GetTheoreticalValue()
        {
            return TheoreticalValue;
        }
    }
}
