using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bird.Platform;

public interface IApplicationEventLoop
{
    /// <summary>
    /// Start the event loop.
    /// </summary>
    public void Start();
}
