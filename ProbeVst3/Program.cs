using System.Runtime.InteropServices;

class ProbeVst3
{
   
   static int Main(string[] args)
   {
      if (args.Length == 0)
         return 2; // bad arguments

      string path = args[0];
      try
      {
         nint nativeProxyHandle;
         bool loadedHandle = NativeLibrary.TryLoad(path, out nativeProxyHandle);
         return loadedHandle ? 0 : 1;
      }
      catch {return -1;}
            
   }
}


