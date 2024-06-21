using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionLibrary
{
    internal class Stock
    {
        public double Beta {  get; set; }
        public double Volatility { get; set; }
        public Stock(double b, double v) {
            Beta = b;
            Volatility = v;
        }

    }
}
