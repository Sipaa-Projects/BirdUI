using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.Gdi32;

namespace Bird.Platform.Win32;

/// <summary>
/// Provides the OpenGL context for Windows systems.
/// </summary>
public class WindowsGLProvider : IPlatformGLProvider
{
    static bool glInitialized = false;

    public WindowsGLProvider()
    {
        if (!glInitialized)
        {
            //OpenGL.Create(SharpGL.Version.OpenGLVersion.OpenGL3_3, RenderContextType.NativeWindow);
        }
    }
}
