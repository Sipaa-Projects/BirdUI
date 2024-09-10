using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bird.UI.Events;

public class RenderEventArgs : EventArgs
{
    public SKSurface SkiaSurface { get; set; }
}
