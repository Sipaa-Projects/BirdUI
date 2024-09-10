using Bird.Platform;
using Bird.UI.Events;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bird.UI;

/// <summary>
/// A Window is a top-level container managed by the operating system/window server in which you can create controls & draw.
/// </summary>
public class Window
{
    IPlatformWindow pfw;
    GRBackendRenderTarget grbrt;
    GRContext grctx;
    GRGlFramebufferInfo grglfbi;
    SKSurface sf;
    Vector2 _sz = new(300, 300);

    #region Properties
    /// <summary>
    /// The size of the window's content.
    /// </summary>
    public Vector2 Size { get => _sz; set {
            if (pfw != null)
                pfw.Size = value;

            _sz = value;
        }
    }
    public string Title
    {
        get => pfw.Title;
        set => pfw.Title = value;
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Gets fired when the window got a new size.
    /// </summary>
    public event EventHandler<ResizeEventArgs> Resize;

    /// <summary>
    /// Gets fired when the window should be repainted.
    /// You can render using Skia, and use OpenGL if you need 3D graphics.
    /// </summary>
    public event EventHandler<RenderEventArgs> Render;
    #endregion

    public Window()
    {
        pfw = PlatformFactory.Instanciate<IPlatformWindow>(null);
        pfw.PWResize.Add(new(OnResize));
        pfw.PWRender.Add(new(OnRender));

        // Create the Skia surface

        grglfbi = new GRGlFramebufferInfo
        {
            FramebufferObjectId = 0,
            Format = 32856 // GL_RGBA8
        };

        grbrt = new GRBackendRenderTarget((int)_sz.X, (int)_sz.Y, 0, 8, grglfbi);
        grctx = GRContext.CreateGl();

        sf = SKSurface.Create(grctx, grbrt, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    }

    protected virtual void OnResize(object sender, (int neww, int newh) sz)
    {
        _sz = new(sz.neww, sz.newh);

        grbrt = new GRBackendRenderTarget(sz.neww, sz.newh, 0, 8, grglfbi);
        sf = SKSurface.Create(grctx, grbrt, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        if (Resize != null)
            Resize.Invoke(this, new() { NewSize = new(sz.neww, sz.newh) });
    }

    protected virtual void OnRender(object sender, EventArgs ev)
    {
        if (Render != null)
            Render.Invoke(this, new() { SkiaSurface = sf });
    }

    public void Show() { pfw.Show(); }
    public void Hide() { pfw.Hide(); }
}
