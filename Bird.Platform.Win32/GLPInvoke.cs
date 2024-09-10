using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.Kernel32;
using Vanara.PInvoke;
using Vanara;
using Microsoft.VisualBasic;
using static Vanara.PInvoke.User32;
using static Vanara.PInvoke.Gdi32;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;

namespace Bird.Platform.Win32;

// Some P/Invoke from Windows's OpenGL implementation (opengl32)
internal class GLPInvoke
{
    public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
    public const uint GL_DEPTH_BUFFER_BIT = 0x00000100;
    public const int GL_FRAMEBUFFER_BINDING = 0x8CA6;

    /// <summary>
    /// Creates an OpenGL context for rendering onto an HDC.
    /// </summary>
    /// <param name="hdc">The HDC</param>
    /// <returns>An OpenGL handle</returns>
    [DllImport("opengl32.dll")]
    public static extern IntPtr wglCreateContext(IntPtr hdc);

    /// <summary>
    /// Delete an OpenGL context
    /// </summary>
    /// <param name="hglrc">The handle to an OpenGL context</param>
    /// <returns></returns>
    [DllImport("opengl32.dll")]
    public static extern bool wglDeleteContext(IntPtr hglrc);

    [DllImport("opengl32.dll")]
    public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

    [DllImport("opengl32.dll")]
    public static extern IntPtr wglGetProcAddress(string unnamedParam1);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glClearColor(float red, float green, float blue, float alpha);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glClear(uint mask);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glGetIntegerv(int pname, out int param);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glViewport(int x, int y, int width, int height);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glFlush();
}
internal class GLLoader
{
    public static IntPtr wglCreateContextAttribsARB;
    public static IntPtr wglChoosePixelFormatARB;
    static bool wglExtensionsInitialized;

    public delegate IntPtr wglCreateContextAttribsARB_d(IntPtr hdc, IntPtr hShareContext, IntPtr attribList);
    public delegate int wglChoosePixelFormatARB_d(IntPtr hdc, IntPtr piAttribIList, ref float pfAttribFList, uint nMaxFormats, out int piFormats, out uint nNumFormats);

    /// <summary>
    /// Ensure OpenGL 3.3 extensions are initialized
    /// </summary>
    public static void EnsureOpenGLExtensionsInitialized()
    {
        if (wglExtensionsInitialized)
            return;

        WNDCLASS windowClass = new()
        {
            style = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_OWNDC,
            lpfnWndProc = DefWindowProc,
            hInstance = GetModuleHandle(),
            lpszClassName = "BirdWin32_GLDummy"
        };

        if (RegisterClass(windowClass) == ATOM.INVALID_ATOM)
        {
            ShowErrorAndExit("Failed to register GL extensions dummy class");
        }

        HWND dummyWindow = CreateWindowEx(0, windowClass.lpszClassName, "Dummy OpenGL Window", 0, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, 0, 0, windowClass.hInstance, 0);
        if (dummyWindow == HWND.NULL)
        {
            ShowErrorAndExit("Failed to create GL dummy window");
        }

        HDC dummyDC = GetDC(dummyWindow);

        PIXELFORMATDESCRIPTOR pfd = new()
        {
            nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            iPixelType = PFD_TYPE.PFD_TYPE_RGBA,
            dwFlags = PFD_FLAGS.PFD_DRAW_TO_WINDOW | PFD_FLAGS.PFD_SUPPORT_OPENGL | PFD_FLAGS.PFD_DOUBLEBUFFER,
            cColorBits = 32,
            cAlphaBits = 8,
            cDepthBits = 24,
            cStencilBits = 8
        };

        int pixelFormat = ChoosePixelFormat(dummyDC, pfd);
        if (pixelFormat == 0)
        {
            ShowErrorAndExit("Failed to get the recommended pixel format");
        }

        if (!SetPixelFormat(dummyDC, pixelFormat, ref pfd))
        {
            ShowErrorAndExit("Failed to set the pixel format");
        }

        IntPtr dummyContext = GLPInvoke.wglCreateContext(dummyDC.DangerousGetHandle());
        if (dummyContext == IntPtr.Zero)
        {
            ShowErrorAndExit("Failed to create a dummy OpenGL context");
        }

        if (!GLPInvoke.wglMakeCurrent(dummyDC.DangerousGetHandle(), dummyContext))
        {
            ShowErrorAndExit("Failed to activate dummy OpenGL context");
        }

        wglCreateContextAttribsARB = GLPInvoke.wglGetProcAddress("wglCreateContextAttribsARB");
        wglChoosePixelFormatARB = GLPInvoke.wglGetProcAddress("wglChoosePixelFormatARB");

        // Clean up dummy resources
        GLPInvoke.wglMakeCurrent(dummyDC.DangerousGetHandle(), IntPtr.Zero);
        GLPInvoke.wglDeleteContext(dummyContext);
        ReleaseDC(dummyWindow, dummyDC);
        DestroyWindow(dummyWindow);

        wglExtensionsInitialized = true;
    }

    public const int WGL_DRAW_TO_WINDOW_ARB = 0x2001;
    public const int WGL_ACCELERATION_ARB = 0x2003;
    public const int WGL_SUPPORT_OPENGL_ARB = 0x2010;
    public const int WGL_DOUBLE_BUFFER_ARB = 0x2011;
    public const int WGL_PIXEL_TYPE_ARB = 0x2013;
    public const int WGL_COLOR_BITS_ARB = 0x2014;
    public const int WGL_DEPTH_BITS_ARB = 0x2022;
    public const int WGL_STENCIL_BITS_ARB = 0x2023;

    public const int WGL_FULL_ACCELERATION_ARB = 0x2027;
    public const int WGL_TYPE_RGBA_ARB = 0x202B;

    public const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
    public const int WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092;
    public const int WGL_CONTEXT_PROFILE_MASK_ARB = 0x9126;
    public const int WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;

    public const int GL_TRUE = 1;

    private static IntPtr ArrayToUnmanaged(int[] arr)
    {
        IntPtr unmanagedArray = Marshal.AllocHGlobal(arr.Length * sizeof(int));
        Marshal.Copy(arr, 0, unmanagedArray, arr.Length);
        return unmanagedArray;
    }

    private static void ShowErrorAndExit(string message)
    {
        MessageBox(HWND.NULL, $"and bird have a broken GL leg... ({message})", "Roses are red, violets are blue...");
        Environment.Exit(1);
    }

    public IntPtr InitOpenGL(IntPtr dc)
    {
        int[] pixelFormatAttribs = {
            WGL_DRAW_TO_WINDOW_ARB, GL_TRUE,
            WGL_SUPPORT_OPENGL_ARB, GL_TRUE,
            WGL_DOUBLE_BUFFER_ARB, GL_TRUE,
            WGL_ACCELERATION_ARB, WGL_FULL_ACCELERATION_ARB,
            WGL_PIXEL_TYPE_ARB, WGL_TYPE_RGBA_ARB,
            WGL_COLOR_BITS_ARB, 32,
            WGL_DEPTH_BITS_ARB, 24,
            WGL_STENCIL_BITS_ARB, 8,
            0
        };

        EnsureOpenGLExtensionsInitialized();

        wglChoosePixelFormatARB_d wcpfarb = Marshal.GetDelegateForFunctionPointer<wglChoosePixelFormatARB_d>(wglChoosePixelFormatARB);
        wglCreateContextAttribsARB_d wccaarb = Marshal.GetDelegateForFunctionPointer<wglCreateContextAttribsARB_d>(wglCreateContextAttribsARB);

        IntPtr unmanagedAttribs = ArrayToUnmanaged(pixelFormatAttribs);
        try
        {
            int pixelFormat;
            uint numFormats;
            float smth = 0;
            wcpfarb(dc, unmanagedAttribs, ref smth, 1, out pixelFormat, out numFormats);

            if (numFormats == 0)
            {
                ShowErrorAndExit("Failed to set the OGL 33 pixel format");
            }

            PIXELFORMATDESCRIPTOR pfd = new();
            DescribePixelFormat(dc, pixelFormat, (uint)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(), ref pfd);
            if (!SetPixelFormat(dc, pixelFormat, ref pfd))
            {
                ShowErrorAndExit("Failed to set the OGL 33 pixel format");
            }

            int[] gl33Attribs = {
                WGL_CONTEXT_MAJOR_VERSION_ARB, 3,
                WGL_CONTEXT_MINOR_VERSION_ARB, 3,
                WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
                0
            };

            IntPtr gl33AttribsUnmanaged = ArrayToUnmanaged(gl33Attribs);
            try
            {
                IntPtr rc = wccaarb(dc, IntPtr.Zero, gl33AttribsUnmanaged);
                if (!GLPInvoke.wglMakeCurrent(dc, rc))
                {
                    ShowErrorAndExit("Failed to activate the OGL 33 context");
                }

                return rc;
            }
            finally
            {
                Marshal.FreeHGlobal(gl33AttribsUnmanaged);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(unmanagedAttribs);
        }
    }
}
