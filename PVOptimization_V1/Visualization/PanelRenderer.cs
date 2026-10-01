using PVOptimization_V1.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PVOptimization_V1.Visualization;

public static class PanelRenderer
{
    private static readonly Rgba32 OutlineColor = new(0, 0, 0, 255);

    public static void RenderPanelsOnImage(RoofGrid grid, Individual individual, string baseImagePath, string outputPath, float strokePx = 2f)
    {
        using var image = Image.Load<Rgba32>(baseImagePath);

        float scaleX = (float)(image.Width / (grid.MaxX - grid.MinX));
        float scaleY = (float)(image.Height / (grid.MaxY - grid.MinY));
        float offsetX = (float)(-grid.MinX * scaleX);
        float offsetY = (float)(-grid.MinY * scaleY);

        image.Mutate(ctx =>
        {
            foreach (var panel in individual.Panels)
            {
                float x0 = (float)(panel.XMin * scaleX + offsetX);
                float y0 = (float)(panel.YMin * scaleY + offsetY);
                float x1 = (float)(panel.XMax * scaleX + offsetX);
                float y1 = (float)(panel.YMax * scaleY + offsetY);

                var rect = new RectangleF(x0, y0, MathF.Max(1, x1 - x0), MathF.Max(1, y1 - y0));
                ctx.Draw(OutlineColor, strokePx, rect);
            }
        });

        try
        {
            image.Save(outputPath);
        }
        catch (IOException)
        {
            Console.WriteLine($"Failed to save the image to: {outputPath}");
            Console.WriteLine("Close the opened image file, then run the program again.");
        }
    }
}
