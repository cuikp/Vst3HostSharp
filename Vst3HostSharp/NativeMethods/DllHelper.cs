using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Vst3HostSharp;

public static partial class DllHelper
{
  
   [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
   [return: MarshalAs(UnmanagedType.Bool)]
   private static partial bool SetDllDirectory(string lpPathName);

   [LibraryImport("libdl.dylib", StringMarshalling = StringMarshalling.Utf16)]
   private static partial IntPtr dlopen(string path, int mode);

   [LibraryImport("libdl.dylib")]
   private static partial int dlclose(IntPtr handle);

   private const int RTLD_NOW = 2; // Load the library immediately
   
   public static void SetLibrarySearchPath(string path)
   {
      
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
         if (!SetDllDirectory(path))
         {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to set DLL directory.");
         }
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
         IntPtr handle = dlopen(path, RTLD_NOW);
         if (handle == IntPtr.Zero)
         {
            throw new InvalidOperationException("Failed to set library search path.");
         }
         int result = dlclose(handle); // Optionally close the handle if not needed
      }


   }
}
