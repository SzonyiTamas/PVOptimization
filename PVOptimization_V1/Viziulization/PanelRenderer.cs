using System;
using System.IO;
using PVOptimization_V1.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace PVOptimization_V1.Visualization
{
    public static class PanelRenderer
    {
        public static void RenderPanelsOnImage(RoofGrid grid, Individual individual, string baseImagePath, string outputPath, float strokePx = 2f)
        {
            using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(baseImagePath);

            double gx0 = grid.MinX, gy0 = grid.MinY;
            double gx1 = grid.MaxX, gy1 = grid.MaxY;
            float sx = (float)(img.Width / (gx1 - gx0));
            float sy = (float)(img.Height / (gy1 - gy0));
            float ox = (float)(-gx0 * sx);
            float oy = (float)(-gy0 * sy);

            var outline = new Rgba32(0, 0, 0, 255);

            img.Mutate(ctx =>
            {
                foreach (var p in individual.Panels)
                {
                    float x0 = (float)(p.XMin * sx + ox);
                    float y0 = (float)(p.YMin * sy + oy);
                    float x1 = (float)(p.XMax * sx + ox);
                    float y1 = (float)(p.YMax * sy + oy);

                    var rect = new RectangleF(x0, y0, MathF.Max(1, x1 - x0), MathF.Max(1, y1 - y0));

                    ctx.Draw(outline, strokePx, rect);
                }
            });

            try
            {
                img.Save(outputPath);
            }
            catch (IOException)
            {
                Console.WriteLine($"Nem sikerült menteni a képet ide: {outputPath}");
                Console.WriteLine("Zárja be a megnyitott képfájlt, majd futtassa újra a programot.");
            }
        }
    }
}
