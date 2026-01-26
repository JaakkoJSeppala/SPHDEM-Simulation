using System;
using ShipDamperSim.Core;
using System.Linq;

namespace ShipDamperSim.Tests
{
    public static class KpiDemoTest
    {
        public static void RunKpiDemo(int runs = 20)
        {
            var parameters = new SimulationParameters(0, 0, 0);
            var benefits = new double[runs];
            double sum = 0, sumSq = 0;
            for (int i = 0; i < runs; i++)
            {
                var result = Simulator.Run(parameters);
                benefits[i] = result.BenefitPercent;
                sum += benefits[i];
                sumSq += benefits[i] * benefits[i];
            }
            double mean = sum / runs;
            double std = Math.Sqrt(sumSq / runs - mean * mean);

            Console.WriteLine($"KPI batch test: maximum roll angle benefit percentage over {runs} runs");
            Console.WriteLine($"Average benefit: {mean:F2} %");
            Console.WriteLine($"Standard deviation: {std:F2} %");
            Console.WriteLine($"Individual results: {string.Join(", ", benefits.Select(b => b.ToString("F1")))}");

            if (mean > 0)
                Console.WriteLine("BATCH TEST PASSED: On average, the damper reduces the roll.");
            else
                Console.WriteLine("BATCH TEST FAILED: On average, the damper does not reduce the roll.");
        }
    }
}
