using PVOptimization_V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVOptimization_V1.Viziulization
{
    internal class ToConsole
    {
        public static void ResultsToConsole(Individual bestResult, double eday, double f, double avgPanel, double avgRoof, double systemKwp)
        {

            Console.Clear();
            Console.WriteLine("\n============== Results of the optimal panel placement ==============\n");

            Console.WriteLine($"Panels:          {bestResult.Panels.Count}");
            Console.WriteLine($"Quality score:   {f:F3}");
            Console.WriteLine($"System:          {systemKwp:F2} kWp");
            Console.WriteLine($"Estimated daily: {eday:F1} kWh\n");

            Console.WriteLine("\n==================== Coordinates of the panels ====================\n");

            Console.WriteLine($"{"#",3} {"TL_X",8} {"TL_Y",8} {"BR_X",8} {"BR_Y",8}");

            int index = 1;
            foreach (var p in bestResult.Panels.OrderBy(x => x.XMin))
            {
                Console.WriteLine($"{index,3} {p.XMin,8:F2} {p.YMin,8:F2} {p.XMax,8:F2} {p.YMax,8:F2}");
                index++;
            }
            Console.WriteLine("\n\n\n");
        }
    }
}
