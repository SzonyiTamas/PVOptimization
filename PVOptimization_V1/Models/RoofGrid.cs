using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PVOptimization_V1.Models
{
    public class RoofGrid
    {
        public double MinX { get; }
        public double MinY { get; }
        public double MaxX { get; }
        public double MaxY { get; }

        private readonly double[,] prefix;
        
        private readonly int[,] forbiddenPrefix;

        private RoofGrid(double[,] prefix,int[,] forbiddenPrefix, double minX, double minY, double maxX, double maxY)
        {
            this.prefix = prefix;
            this.forbiddenPrefix = forbiddenPrefix;
            MinX = minX; 
            MinY = minY; 
            MaxX = maxX; 
            MaxY = maxY;
        }

        public static RoofGrid FromCsv()
        {
            using var reader = new StreamReader(Path.Combine("Data","roof_avg_kontyolt_kemennyel_forbidden.csv"));
            var header = reader.ReadLine();
            var cols = header.Split(',').Select(s => s.Trim()).ToArray();

            int? Ix(string name)
            {
                name = name.Trim().ToLowerInvariant();

                for (int i = 0; i < cols.Length; i++)
                {
                    var c = cols[i].Trim().ToLowerInvariant();
                    if (c == name)
                        return i;
                }
                return null;
            }

            int? iXC = Ix("center_x");
            int? iYC = Ix("center_y");
            int? iVal = Ix("value");

            var raw = new List<(double xc, double yc, double val)>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var p = line.Split(',');
                if (!TryParse(p[(int)iXC!], out double xc)) continue;
                if (!TryParse(p[(int)iYC!], out double yc)) continue;
                if (!TryParse(p[(int)iVal!], out double v)) continue;
                raw.Add((xc, yc, v));
            }

            double minX = raw.Min(t=>t.xc);
            double maxX = raw.Max(t=>t.xc);
            double minY = raw.Min(t => t.yc);
            double maxY = raw.Max(t=>t.yc);

            int nx = (int)Math.Round((maxX - minX)) + 1;
            int ny = (int)Math.Round((maxY - minY)) + 1;

            var prefix = new double[nx + 1, ny + 1];
            var forb = new int[nx + 1, ny + 1]; 

            foreach (var (xc, yc, val) in raw)
            {
                int ix = (int)Math.Round((xc - minX));
                int iy = (int)Math.Round((yc - minY));
                
                double add = val == -1.0 ? 0.0 : val;
                prefix[ix + 1, iy + 1] += add;
                
                if (val == -1.0)
                    forb[ix + 1, iy + 1] += 1;
            }

            for (int iy = 1; iy <= ny; iy++)
            {
                for (int ix = 1; ix <= nx; ix++)
                {
                    prefix[ix, iy] += prefix[ix - 1, iy] + prefix[ix, iy - 1] - prefix[ix - 1, iy - 1];
                    
                    forb[ix, iy] += forb[ix - 1, iy] + forb[ix, iy - 1] - forb[ix - 1, iy - 1];
                }
            }
            
            return new RoofGrid(prefix, forb, minX, minY, maxX, maxY);
        }

        private bool RectOverlapsForbidden(int ix0, int iy0, int ix1, int iy1)
        {
            ix0++; iy0++; ix1++; iy1++;
            int cnt = forbiddenPrefix[ix1, iy1] - forbiddenPrefix[ix0 - 1, iy1] - forbiddenPrefix[ix1, iy0 - 1] + forbiddenPrefix[ix0 - 1, iy0 - 1];
            return cnt > 0;
        }

        public bool RectOverlapsForbidden(double xMin, double yMin, double xMax, double yMax)
        {
            ToIndexBounds(xMin, yMin, xMax, yMax, out int ix0, out int iy0, out int ix1, out int iy1);
            return RectOverlapsForbidden(ix0, iy0, ix1, iy1);
        }

        public double RectSum(int ix0, int iy0, int ix1, int iy1)
        {
            ix0++; iy0++; ix1++; iy1++;
            return prefix[ix1, iy1] - prefix[ix0 - 1, iy1] - prefix[ix1, iy0 - 1] + prefix[ix0 - 1, iy0 - 1];
        }

        public void ToIndexBounds(double xMin, double yMin, double xMax, double yMax,
                                  out int ix0, out int iy0, out int ix1, out int iy1)
        {
            ix0 = (int)Math.Ceiling(xMin - MinX);
            iy0 = (int)Math.Ceiling(yMin - MinY);
            ix1 = (int)Math.Floor(xMax - MinX);
            iy1 = (int)Math.Floor(yMax - MinY);

        }

        private static bool TryParse(string s, out double val)
        {
            s = s.Trim();
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out val)) return true;
            var s2 = s.Replace(',', '.');
            return double.TryParse(s2, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
        }
    }
}
