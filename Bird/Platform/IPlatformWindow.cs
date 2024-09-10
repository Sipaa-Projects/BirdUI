using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bird.Platform;

public interface IPlatformWindow : IDisposable
{
    public IntPtr Handle { get; protected set; }
    public List<Action<object, EventArgs>> PWRender { get; protected set; }
    public List<Action<object, (int width, int height)>> PWResize { get; protected set; }
    public Vector2 Size { get; set; }
    public string Title { get; set; }

    public void Show();
    public void Hide();
    public void Close();

    public void OnRender();
    public void OnResizing(int neww, int newh);
}
