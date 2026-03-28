using PVOptimization_V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVOptimization_V1.GA
{
    public static class EnergyEstimator
    {
        public static (double EdayKwh, double F, double AvgPanel, double AvgRoof, double SystemKwp) EstimateDailyProductionKwh(Individual best, RoofGrid grid, double panelWp, double hPoaKwhPerM2, double pr)
        {

            int n = best.Panels.Count;
            double panelKwp = panelWp / 1000.0;
            double systemKwp = n * panelKwp;

            int w = grid.Width;
            int h = grid.Height;

            double roofSum = grid.RectSum(0, 0, w - 1, h - 1);
            int forbidden = grid.RectForbiddenCount(0, 0, w - 1, h - 1);
            int allowed = (w * h) - forbidden;

            double avgRoof = roofSum / allowed;

            double sumPanelAvg = 0.0;
            double eday = 0.0;
            int used = 0;

            foreach (var p in best.Panels)
            {
                grid.ToIndexBounds(
                    p.XMin, p.YMin, p.XMax, p.YMax,
                    out int ix0, out int iy0, out int ix1, out int iy1);

                int count = (ix1 - ix0 + 1) * (iy1 - iy0 + 1);
                if (count <= 0) continue;

                double sum = grid.RectSum(ix0, iy0, ix1, iy1);
                double panelAvg = sum / count;

                sumPanelAvg += panelAvg;
                used++;

                double localFactor = panelAvg / avgRoof;
                if (localFactor < 0) localFactor = 0;
                if (localFactor > 2) localFactor = 2;

                eday += panelKwp * hPoaKwhPerM2 * pr * localFactor;
            }

            double avgPanel = sumPanelAvg / used;

            double f = avgRoof > 1e-9 ? (avgPanel / avgRoof) : 0.0;
            if (f < 0) f = 0;
            if (f > 2) f = 2;

            return (eday, f, avgPanel, avgRoof, systemKwp);
        }
    }
}
