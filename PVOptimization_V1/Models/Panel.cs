using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVOptimization_V1.Models
{
    public class Panel
    {
        public const double PanelWidthPx = 100.0;
        public const double PanelHeightPx = 150.0;

        public bool Rotated { get; private set; }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width => Rotated ? PanelHeightPx : PanelWidthPx;
        public double Height => Rotated ? PanelWidthPx : PanelHeightPx;

        public double XMin => X;
        public double YMin => Y;
        public double XMax => X + Width;
        public double YMax => Y + Height;

        //EasyInstall -> CenterX, CenterY
        public double CenterX => XMin + (XMax - XMin) * 0.5;
        public double CenterY => YMin + (YMax - YMin) * 0.5;

        public Panel(double x, double y, bool rotated = false)
        {
            X = x;
            Y = y;
            Rotated = rotated;
        }

        public void ToggleOrientation() => Rotated = !Rotated;

        public void MoveBy(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }

        public void ClampToBounds(double minX, double minY, double maxX, double maxY)
        {
            if (XMax > maxX) X = Math.Max(minX, maxX - Width);
            if (YMax > maxY) Y = Math.Max(minY, maxY - Height);
            if (X < minX) X = minX;
            if (Y < minY) Y = minY;
        }
        public bool Overlaps(Panel other)
        {
            if (XMax <= other.XMin || other.XMax <= XMin) return false;
            if (YMax <= other.YMin || other.YMax <= YMin) return false;
            return true;
        }
    }
}

