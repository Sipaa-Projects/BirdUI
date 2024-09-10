using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace Bird.Platform.Win32;
internal class ApplicationEventLoop : IApplicationEventLoop
{
    public void Start()
    {
        MSG msg;
        bool running = true;

        while (running && !PlatformWindowManager.appeventhandler_shouldquit)
        {
            // Handle Windows messages
            while (PeekMessage(out msg, IntPtr.Zero, 0, 0, PM.PM_REMOVE))
            {
                if (msg.message == (uint)WindowMessage.WM_QUIT)
                {
                    running = false;
                    break;
                }

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }

            // Process window rendering
        }
    }
}
