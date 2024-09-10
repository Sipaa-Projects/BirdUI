using Bird.Platform;
using Bird.UI;
using SkiaSharp;
using System.Diagnostics;

namespace BirdUI.Sample;

internal class Program
{
    public static void AddWindow()
    {
        IPlatformWindow pw = PlatformFactory.Instanciate<IPlatformWindow>(null);
        GRGlFramebufferInfo fbi = new GRGlFramebufferInfo
        {
            FramebufferObjectId = 0,
            Format = 32856 // GL_RGBA8
        };

        // Create a backend render target for the specific window
        GRBackendRenderTarget brt = new GRBackendRenderTarget(300, 300, 0, 8, fbi);
        GRContext ctx = GRContext.CreateGl();

        // Create a new surface for the window using the shared GRContext
        SKSurface sf = SKSurface.Create(ctx, brt, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        // Handle window resize to update the render target
        pw.PWResize.Add((s, e) =>
        {
            Debug.WriteLine("PWRR");
            brt = new GRBackendRenderTarget(e.width, e.height, 0, 8, fbi);
            sf = SKSurface.Create(ctx, brt, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        });

        // Render callback for the window
        pw.PWRender.Add((s, e) =>
        {
            Debug.WriteLine("PWR");
            sf.Canvas.Clear(SKColors.LightGray);

            using SKPaint p = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };

            sf.Canvas.DrawText("Bird v0.0.1 - SkiaSharp 2.88.8 - OpenGL 3.3", new(10, 10 + 10), p);
            sf.Canvas.DrawRoundRect(new SKRoundRect(new SKRect(brt.Width / 2 - 50, brt.Height / 2 - 50, brt.Width / 2 + 50, brt.Height / 2 + 50), 10.0f), p);

            sf.Flush();
        });

        pw.Show();
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        //AddWindow();
        //AddWindow();

        Window w = new();
        w.Title = "Bird UI - Test";
        w.Render += (s, e) =>
        {
            var c = e.SkiaSurface.Canvas;
            c.Clear(SKColors.LightGray);

            using SKPaint p = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };

            c.DrawText("Bird v0.0.1 - SkiaSharp 2.88.8 - OpenGL 3.3", new(10, 10 + 10), p);
            c.DrawRoundRect(new SKRoundRect(new SKRect(w.Size.X / 2 - 50, w.Size.Y / 2 - 50, w.Size.X / 2 + 50, w.Size.Y / 2 + 50), 10.0f), p);

            e.SkiaSurface.Flush();
        };
        w.Show();

        IApplicationEventLoop ev = PlatformFactory.Instanciate<IApplicationEventLoop>(null);
        ev.Start();
    }
}
