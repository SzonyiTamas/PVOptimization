using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVOptimization_V1.Models
{
    public class Individual
    {
        public List<Panel> Panels { get; }
        public double Fitness { get; set; }

        public Individual(List<Panel> panels)
        {
            Panels = panels;
        }
        public Individual DeepCopy()
        {
            var copyPanels = new List<Panel>(Panels.Count);
            foreach (var p in Panels)
            {
                copyPanels.Add(new Panel(p.XMin, p.YMin, p.Rotated));
            }

            var clone = new Individual(copyPanels);
            clone.Fitness = this.Fitness;

            return clone;
        }
    }
}