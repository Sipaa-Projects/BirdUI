using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bird.UI.Events;

/// <summary>
/// Arguments for the Resize event.
/// </summary>
public class ResizeEventArgs : EventArgs
{
    /// <summary>
    /// The new size
    /// </summary>
    public Vector2 NewSize { get; set; }

}
