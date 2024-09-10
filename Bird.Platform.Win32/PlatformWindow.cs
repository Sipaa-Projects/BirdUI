using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Vanara.PInvoke.User32;
using Vanara.PInvoke;
using System.Diagnostics;
using System.Numerics;

namespace Bird.Platform.Win32;

public class PlatformWindow : IPlatformWindow
{
    public IntPtr Handle { get; set; } = IntPtr.Zero;
    public List<Action<object, EventArgs>> PWRender { get; set; } = new();
    public List<Action<object, (int width, int height)>> PWResize { get; set; } = new();
    public Vector2 Size
    {
        get => _size; set
        {
            if (Handle != IntPtr.Zero)
            {
                SetWindowPos(Handle, HWND.NULL, 0, 0, (int)value.X, (int)value.Y, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE);
                _size = value;
            }
        }
    }
    public string Title
    {
        get => _title;
        set
        {
            if (Handle != IntPtr.Zero)
            {
                SetWindowText(Handle, value);
                _title = value;
            }
        }
    }

    private string _title = "Bird";
    private Vector2 _size = new(300, 300);
    private bool resizing = false;
    private HDC dc;
    private IntPtr hglrc;

    public unsafe PlatformWindow()
    {
        PlatformWindowManager.EnsureWindowClass(); // Ensure the window class has been created

        HINSTANCE hinst = new HINSTANCE(Kernel32.GetModuleHandle().DangerousGetHandle()); // Retrive the HINSTANCE

        // Create the window handle
        Handle = CreateWindowEx(
            (WindowStylesEx)0,
            PlatformWindowManager.className, "Bird", 
            WindowStyles.WS_OVERLAPPEDWINDOW, 
            CW_USEDEFAULT, CW_USEDEFAULT, (int)Size.X, (int)Size.Y, 
            HWND.NULL, HMENU.NULL, hinst, nint.Zero).DangerousGetHandle();

        dc = GetDC(Handle);

        GLLoader gll = new();
        hglrc = gll.InitOpenGL(dc.DangerousGetHandle());

        //GLPInvoke.glViewport(0, 0, 800, 600); // For now we will use a 800x600 viewport

        PlatformWindowManager.windows.Add(this);
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    ~PlatformWindow()
    {
        Close();
    }

    /// <summary>
    /// Show up the window on the screen.
    /// </summary>
    public void Show()
    {
        ShowWindow(Handle, ShowWindowCommand.SW_SHOW); // Do not change the window state
    }

    /// <summary>
    /// Hides the window.
    /// </summary>
    public void Hide()
    {
        ShowWindow(Handle, ShowWindowCommand.SW_HIDE); // Do not change the window state
    }

    /// <summary>
    /// A user can also dispose a window, which is equal to closing a window
    /// </summary>
    public void Dispose() => Close();

    /// <summary>
    /// Close a window for good.
    /// </summary>
    public void Close()
    {
        GLPInvoke.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero); // an OpenGL context will be automatically restored when the windows are re-rendered
        GLPInvoke.wglDeleteContext(Handle);

        DestroyWindow(Handle);
        PlatformWindowManager.windows.Remove(this);
    }

    public void OnRender()
    {
        // Switch to the window's GL context
        GLPInvoke.wglMakeCurrent(dc.DangerousGetHandle(), hglrc);

        //GLPInvoke.glClearColor(1f, 1f, 1f, 1.0f);
        //GLPInvoke.glClear(GLPInvoke.GL_COLOR_BUFFER_BIT | GLPInvoke.GL_DEPTH_BUFFER_BIT);

        foreach (var ev in PWRender)
        {
            ev.Invoke(this, new());
        }

        GLPInvoke.glFlush();
        Gdi32.SwapBuffers(dc);
    }

    public void OnResizing(int neww, int newh)
    {
        _size = new(neww, newh);
        foreach (var ev in PWResize)
        {
            ev.Invoke(this, (neww, newh));
        }
    }
}
