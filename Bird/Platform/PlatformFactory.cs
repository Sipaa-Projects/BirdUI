using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bird.Platform;

/// <summary>
/// The platform factory allows instancing platform-dependant objects
/// </summary>
public static class PlatformFactory
{
    static string GetCurrentPlatformName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Win32";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux"; // Most Linux system & users uses X.Org or Wayland.
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "Mac";

        return "Unknown";
    }

    public static T Instanciate<T>(object[]? args)
    {
        if (!typeof(T).IsInterface)
            throw new Exception("T must be an interface.");

        // We assume that each interface type's name start with 'I'
        string AssemblyName = typeof(SupportedPlatforms).Namespace + "." + GetCurrentPlatformName(); // This would build up the namespace "Bird.Platform.Win32", for example
        string TypeName = typeof(SupportedPlatforms).Namespace + "." + GetCurrentPlatformName() + "." + typeof(T).Name.Substring(1); // This would build up the namespace "Bird.Platform.Win32.PlatformWindow", for example

        Type t = Type.GetType(TypeName + ", " + AssemblyName);
        if (t != null && typeof(T).IsAssignableFrom(t)) {
            T t2 = (T)Activator.CreateInstance(t, args);
            return t2;
        }

        throw new Exception($"We could not find an implementation of {typeof(T).Name} for the {GetCurrentPlatformName()} platform. (T == null: {t == null})");
    }
}
