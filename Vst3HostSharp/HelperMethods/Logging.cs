namespace Vst3HostSharp;

public class Logging
{
   internal static string outputLogFile = "";
   internal static bool OutputLogPathSet = false;

   public static void SetOutputLogPath(string outputLogPath)
   {
      outputLogFile = Path.Combine(outputLogPath, "output.log");
      NativeMethods.SetLogFilePath(outputLogFile);
      OutputLogPathSet = true;
   }

   internal static void LogOutput(string outputLine)
   {
      if (!OutputLogPathSet) return;
      File.AppendAllText(outputLogFile, "VST3HOSTSHARP:: " + outputLine + "\n");
   }
}