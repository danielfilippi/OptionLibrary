
using OptionLibrary;
using OptionLibrary.Equity;

using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        /*ORDER OF MODELS
         * GBM
         * JUMP DIFFUSION
         * LOCAL VOLATILITY
         * HESTON
         * 
         * 
         * 
         * 
         * 
         */
        //Option option = new Option('A', 'C', 100, 100, (double)5/100, (double)20/100, (double)21, (double)2/100);
        Stock stock = new Stock(1, 0.2);

        Option option = new Option('A', 'C', 100, 100, 0.05, 0.20, 182, 0);

        //Option option = new Option('A', 'C', 185, 183.7, (double)5 / 100, (double)24.9 / 100, (double)31, 0);
        //Stock stock = new Stock(1.14, 0.2);

        //Option option = new Option('A', 'C', 150, 100, (double)5 / 100, (double)50 / 100, (double)31, 0);

        Stopwatch sw = Stopwatch.StartNew();
        option.SetTheoreticalValue(Binomial.BinomialWithDividends(option));
        sw.Stop();
        Console.WriteLine("Binomial model value: $" + option.GetTheoreticalValue() +"\nIn "+sw.ElapsedMilliseconds + " ms");

        Stopwatch sw2 = Stopwatch.StartNew();
        option.SetTheoreticalValue(BlackScholes.BlackScholesWithDividends(option));
        sw2.Stop();
        Console.WriteLine("Black scholes model value: $" + option.GetTheoreticalValue()+ "\nIn "+sw2.ElapsedMilliseconds + " ms");

        Stopwatch sw3 = Stopwatch.StartNew();
        option.SetTheoreticalValue(Trinomial.TrinomialWithDividends(option));
        sw3.Stop();
        Console.WriteLine("Trinomial model value: $" + option.GetTheoreticalValue() + "\nIn " + sw3.ElapsedMilliseconds + " ms");

        Stopwatch sw4 = Stopwatch.StartNew();
        //option.SetTheoreticalValue(GBM.GBMWithDividends(option, 10000));
        sw4.Stop();
        Console.WriteLine("Geometric Brownian Motion model value: $" + option.GetTheoreticalValue() + "\nIn " + sw4.ElapsedMilliseconds + " ms");

        Stopwatch sw5 = Stopwatch.StartNew();
        option.SetTheoreticalValue(JumpDiffusion.JumpDiffusionBinomial(option, 0.5, -0.01, 0.07));
        //option.SetTheoreticalValue(JumpDiffusion.JumpDiffusionMonteCarlo(option,50000, 0.3, -0.01, 0.2));

        sw5.Stop();
        Console.WriteLine("Jump Diffusion model value: $" + option.GetTheoreticalValue() + "\nIn " + sw5.ElapsedMilliseconds + " ms");

        Stopwatch sw6 = Stopwatch.StartNew();
        option.SetTheoreticalValue(CustomMC.custom(option, stock));
        sw6.Stop();
        Console.WriteLine("Custom model value: $" +option.GetTheoreticalValue() + "\nIn "+sw6.ElapsedMilliseconds + " ms");
    }
}