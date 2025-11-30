using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Vst3HostSharp.ArchitectureDetector;
using static Vst3HostSharp.Logging;
using static Vst3HostSharp.StructsAndEnums;

namespace Vst3HostSharp;

public class Vst3PluginInfo : INotifyPropertyChanged
{
   public event PropertyChangedEventHandler? PropertyChanged;
   private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

   public int ClassCount = 0;
   public string Vst3FilePath = "";
   public string VstFileName => Path.GetFileName(Vst3FilePath);
   public string InnerVst3FilePath = "";

   public bool Initialized => myVst3Plugin != null && myVst3Plugin.Initialized;
   public bool HasEditor => myVst3Plugin != null && myVst3Plugin.HasEditor;
   public bool IsEditorViewCreated => myVst3Plugin != null && myVst3Plugin.IsEditorViewCreated;

 
   public Vst3Plugin myVst3Plugin = null!;

   public Vst3PluginInfo(string filePath)
   {

      if (File.Exists(filePath))
      {
         Vst3FilePath = filePath;
         InnerVst3FilePath = filePath;
         //LogOutput("filePath exists as a file");

      }
      else if (Directory.Exists(filePath))
      {  //LogOutput("filePath DOES NOT exist: checking as directory");

         string[] vst3Contents = [];
         if (OperatingSystem.IsWindows())
            vst3Contents = Directory.GetFiles(Path.Combine(filePath, "Contents", "x86_64-win"));
         else if (OperatingSystem.IsMacOS())
            vst3Contents = Directory.GetFiles(Path.Combine(filePath, "Contents", "MacOS"));


         if (vst3Contents.Length > 0)
         {
            InnerVst3FilePath = vst3Contents.Where(s =>
            {
               return Path.GetFileNameWithoutExtension(s) == Path.GetFileNameWithoutExtension(filePath);
            }).FirstOrDefault()!;
                        

            if (InnerVst3FilePath != null)
            {
               //LogOutput("Got InnerVst3FilePath: " + InnerVst3FilePath);

               if (OperatingSystem.IsWindows())
                  Vst3FilePath = InnerVst3FilePath;
               else if (OperatingSystem.IsMacOS())
                  Vst3FilePath = filePath;
            }
            else
            {
               LogOutput("Couldn't get InnerVst3FilePath");
               Vst3FilePath = "Unable to find inner vst3 path: (" + filePath + ")";
               return;
            }
         }
      }


      //Probe file first with independent process:
      int pluginProbeResult = ProbePlugin(InnerVst3FilePath);
      if (pluginProbeResult != 0)
      {
         Vst3FilePath = "Unloadable: " + VstFileName;
         if (pluginProbeResult == -5)  // wrong architecture
            PluginType = VstType.VstFailedLoad;
         else
            PluginType = VstType.VstCrashedLoad;
         return;
      }

      //LogOutput("probed plugin result is 0: " + pluginProbeResult);

      try
      {
         //LogOutput("Will try to get vst3pluginInfo for: " + Vst3FilePath + " (" + VstFileName + ")");
         GetVst3PluginInfo();
         DisposePointer();
      }
      catch (Exception ex)
      {
         LogOutput("Error getting vst3info: " + ex.Message + " ::: " + VstFileName);
         Vst3FilePath = "____ERROR WITH : " + VstFileName;
         Debug.WriteLine("\n\nERROR getting vst3info: " + VstFileName);
      }

   }

   static int ProbePlugin(string pluginPath)
   {
      //First filter out unsupported architectures
      if (OperatingSystem.IsMacOS())
         if (!MachO.GetMacArchitectures(pluginPath).Contains(MachO.MacCpuType.ARM64)) return -5;
      else if (OperatingSystem.IsWindows())
         if (GetWinPeMachine(pluginPath) != WinMachineType.AMD64) return -5;

      //Architecture is ok, so try to load with independent process: 
      try
      {
         string exePath = "";

         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            exePath = Path.Combine(AppContext.BaseDirectory, "ProbeVst3_win.exe");
         else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            exePath = Path.Combine(AppContext.BaseDirectory, "ProbeVst3_mac");

         var psi = new ProcessStartInfo
         {
            FileName = exePath,
            ArgumentList = { pluginPath },
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
         };

         using var proc = Process.Start(psi)!;
         proc.WaitForExit();
         
         return proc.ExitCode;

      }
      catch { return -1;  }

   }


   public VstType PluginType = VstType.VstUnknown;
   public VstType PluginTypeString => (VstType)PluginType;

   public bool CanLoadVst => this.PluginType is VstType.VstEffect or VstType.VstInstrument;

   bool _disposed = false;
   IntPtr _nativeProxyHandle = IntPtr.Zero;
   internal IntPtr PluginFactoryPtr;

   public void DisposePointer()
   {
      if (_disposed) return;
      _disposed = true;

      if (PluginFactoryPtr != IntPtr.Zero)
         NativeMethods.ReleasePluginFactory(PluginFactoryPtr);
      if (_nativeProxyHandle != IntPtr.Zero)
         NativeLibrary.Free(_nativeProxyHandle);
      PluginFactoryPtr = IntPtr.Zero;
      _nativeProxyHandle = IntPtr.Zero;

   }

   internal List<ManagedPClassInfo> GetManagedClassInfos(int classCount)
   {
      List<ManagedPClassInfo> classInfos = [];

      PClassInfoW pClassInfoW;
      PClassInfo2 pClassInfo2;
      PClassInfo pClassInfo;

      for (int classno = 0; classno < classCount; classno++)
      {
         int result = NativeMethods.GetClassInfosNew(PluginFactoryPtr, classno, out pClassInfoW, out pClassInfo2, out pClassInfo);

         switch (result)
         {
            case 3:
               classInfos.Add(new ManagedPClassInfo(pClassInfoW));
               break;
            case 2:
               classInfos.Add(new ManagedPClassInfo(pClassInfo2));
               break;
            case 1:
               classInfos.Add(new ManagedPClassInfo(pClassInfo));
               break;
         }
      }

      return classInfos;

   }


   public string Name { get; set; } = "";
   public string Category { get; set; } = "";
   public string SubCategories { get; set; } = "";
   public string Vendor { get; set; } = "";
   public string Version { get; set; } = "";
   public int Cardinality { get; set; } = 0;
   public string SdkVersion { get; set; } = "";
   public uint ClassFlags { get; set; } = 0;

   [DllImport("libdl.dylib")]
   internal static extern IntPtr dlopen(string path, int mode);

   [DllImport("libdl.dylib")]
   internal static extern IntPtr dlerror();

   public const int RTLD_LAZY = 0x1;
   public const int RTLD_NOW = 0x2;

   static void TestDlopenFromHost(string path)
   {
      var h = dlopen(path, RTLD_NOW);
      if (h == IntPtr.Zero)
      {
         var errPtr = dlerror();
         if (errPtr != IntPtr.Zero)
            LogOutput("dlerror: " + Marshal.PtrToStringAnsi(errPtr));
      }
   }

   private void GetVst3PluginInfo()
   {

      try
      {
         //LogOutput("will try to load native library: " + InnerVst3FilePath);

         bool loadedHandle = NativeLibrary.TryLoad(InnerVst3FilePath, out _nativeProxyHandle);

         if (!loadedHandle)
         {
            LogOutput("failed to get native proxy handle");
            PluginType = VstType.VstFailedLoad;
            return;
         }

         //LogOutput("got native proxy handle");

         if (_nativeProxyHandle != IntPtr.Zero)
         {
            var getPluginFactoryPtr = NativeLibrary.GetExport(_nativeProxyHandle, "GetPluginFactory");

            if (getPluginFactoryPtr == IntPtr.Zero)
            {
               PluginType = VstType.VstFailedLoad;
               return;
            }

            PluginFactoryPtr = Marshal.GetDelegateForFunctionPointer<NativeMethods.GetPluginFactoryDelegate>(getPluginFactoryPtr)();

            try
            {
               ClassCount = NativeMethods.CountClassesNew(PluginFactoryPtr);

               if (ClassCount == 0)
                  PluginType = VstType.VstFailedLoad;
               else
               {
                  List<ManagedPClassInfo> mClassInfos = GetManagedClassInfos(ClassCount);

                  var firstValidClassInfo = mClassInfos.FirstOrDefault(mc => !string.IsNullOrEmpty(mc.Name));
                  Name = firstValidClassInfo?.Name ?? string.Empty;
                  firstValidClassInfo = mClassInfos.FirstOrDefault(mc => !string.IsNullOrEmpty(mc.Category));
                  Category = firstValidClassInfo?.Category ?? string.Empty;
                  firstValidClassInfo = mClassInfos.FirstOrDefault(mc => !string.IsNullOrEmpty(mc.SubCategories));
                  SubCategories = firstValidClassInfo?.SubCategories ?? string.Empty;
                  PluginType = SubCategories.ToLower().Contains("instrument") ? VstType.VstInstrument : (SubCategories.ToLower().Contains("fx") ? VstType.VstEffect : VstType.VstUnknown);
                  firstValidClassInfo = mClassInfos.FirstOrDefault(mc => !string.IsNullOrEmpty(mc.Vendor));
                  Vendor = firstValidClassInfo?.Vendor ?? string.Empty;
                  firstValidClassInfo = mClassInfos.FirstOrDefault(mc => !string.IsNullOrEmpty(mc.Version));
                  Version = firstValidClassInfo?.Version ?? string.Empty;
                  Cardinality = mClassInfos[0].Cardinality;
                  firstValidClassInfo = mClassInfos.FirstOrDefault(mc => !string.IsNullOrEmpty(mc.SdkVersion));
                  SdkVersion = firstValidClassInfo?.SdkVersion ?? string.Empty;
                  ClassFlags = mClassInfos[0].ClassFlags;

               }
            }
            catch (Exception ex) { PluginType = VstType.VstUnknown; ClassCount = -1000; Debug.WriteLine(ex.Message); LogOutput("error  getting class count"); }
         }
         else
            PluginType = VstType.VstFailedLoad;
      }
      catch (Exception ex) { LogOutput("FAILED LOAD"); Debug.WriteLine("\nVst3HostSharp: Error trying to load vst3: " + ex.Message + "\n"); PluginType = VstType.VstFailedLoad; }

   }

   public string GetInfoList()
   {
      string retString = "";
      int index = 0;
      foreach (ManagedPClassInfo mClassInfo in GetManagedClassInfos(ClassCount))
      {
         retString += string.Join("\n",
          [
            (index + 1).ToString(),
            "Name= " + mClassInfo.Name,
            "Category= " + mClassInfo.Category,
            "SubCats= " + mClassInfo.SubCategories,
            "Vendor= " + mClassInfo.Vendor,
            "Version= " + mClassInfo.Version,
            "Cardinality: " + mClassInfo.Cardinality.ToString(),
            "ClassFlags: " + mClassInfo.ClassFlags.ToString(),
            "Cid= " + mClassInfo.Cid.ToString(),
            "SdkVer= " + mClassInfo.SdkVersion
         ]);
      }
      return retString.TrimEnd('\n');

   }



}