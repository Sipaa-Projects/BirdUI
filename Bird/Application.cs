using Bird.Platform;

namespace Bird;

public class Application
{
    /// <summary>
    /// An event which gets fired when the application is ready. It gets invoked before entering the event loop.
    /// </summary>
    public Action Ready { get; private set; }
    
    /// <summary>
    /// The constructor.
    /// </summary>
    private Application() { }
    
    /// <summary>
    /// Run the application
    /// </summary>
    public void Run()
    {
        if (Ready != null)
            Ready.Invoke();

        IApplicationEventLoop ev = PlatformFactory.Instanciate<IApplicationEventLoop>(null);
        ev.Start();
    }
}
