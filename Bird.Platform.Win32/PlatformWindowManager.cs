using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.User32;
using Vanara.PInvoke;

namespace Bird.Platform.Win32;

/// <summary>
/// PlatformWindow is used to keep track of windows.
/// It is the reason why you can create an instance of ApplicationEventLoop and it will work.
/// </summary>
internal class PlatformWindowManager
{
    public static List<IPlatformWindow> windows = new List<IPlatformWindow>();
    public const string className = "BirdUIWindow";
    static WindowProc proc;
    static bool wndClassInitialized = false;
    public static bool appeventhandler_shouldquit = false;
    
    private static int LOWORD(int value)
    {
        return value & 0xFFFF;
    }

    // Extract the higher 16 bits (HIWORD)
    private static int HIWORD(int value)
    {
        return (value >> 16) & 0xFFFF;
    }

    private static nint WndProc(HWND hwnd, uint msg, nint wparam, nint lparam)
    {
        IPlatformWindow w = GetFromHandle(hwnd.DangerousGetHandle());
        if (w == null)
            return DefWindowProc(hwnd, msg, wparam, lparam);

        switch ((WindowMessage)msg)
        {
            case WindowMessage.WM_DESTROY:
                w.Dispose();
                windows.Remove(w);

                if (PlatformWindowManager.windows.Count == 0)
                    appeventhandler_shouldquit = true;
                break;

            case WindowMessage.WM_PAINT:
                w.OnRender();
                break;

            case WindowMessage.WM_SIZE:
                w.OnResizing(LOWORD(lparam.ToInt32()), HIWORD(lparam.ToInt32()));
                break;

            case WindowMessage.WM_SETCURSOR:
                SetCursor(LoadCursor(HINSTANCE.NULL, IDC_ARROW));
                break;
        }
        return DefWindowProc(hwnd, msg, wparam, lparam);
    }

    public static IPlatformWindow GetFromHandle(IntPtr hwnd)
    {
        foreach (var win in windows)
        {
            if (win.Handle == hwnd)
                return win;
        }
        return null;
    }

    public static void EnsureWindowClass()
    {
        if (!wndClassInitialized) { 
            HINSTANCE hinst = new HINSTANCE(Kernel32.GetModuleHandle().DangerousGetHandle());

            var wndproc = new WindowProc(WndProc);
            proc = wndproc;

            WNDCLASSEX wex = new();
            wex.cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX));
            wex.lpfnWndProc = wndproc;
            wex.hInstance = hinst;
            wex.hCursor = HCURSOR.NULL;
            wex.hIcon = HICON.NULL;
            wex.lpszClassName = className;
            wex.style = WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_OWNDC;
            RegisterClassEx(wex);

            wndClassInitialized = true;
        }
    }
}
