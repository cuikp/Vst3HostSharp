using System.Runtime.InteropServices;

namespace Vst3HostSharp;

internal partial class NativeMethods
{
   [LibraryImport("Vst3Pont", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial void SetLogFilePath(string path);

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate IntPtr GetPluginFactoryDelegate();

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial void FreeString(IntPtr str);

   //Plugin
   [LibraryImport("Vst3Pont", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int LoadVst3Ext(string path, ref int hasEditor);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int CountClassesNew(IntPtr pluginFactory);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial void ReleasePluginFactory(IntPtr pluginFactory);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetClassInfosNew(IntPtr pluginFactory, int index, out PClassInfoW pinfoW, out PClassInfo2 pinfo2, out PClassInfo pinfo);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int DisposePlugin(int pluginId);

   //AudioProcessor
   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int SetupAudioProcessor(int pluginId, int bufferSize, ref IntPtr processDataPtr, int parameterCount, double sampleRate);

   ////internal static extern int Process(int pluginId, IntPtr procData);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int Process(int pluginId);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetBusInfo(int pluginID, int mediaType, int busdirection, int index, ref BusInfo businfo);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetBusCount(int pluginID, int mediaType, int busdirection);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int ActivateBus(int pluginID, int mediaType, int busdirection, int index, byte state);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetStateOfPlugin(int pluginId, out IntPtr stateStream, out long streamSize);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int ReleaseStateStream(IntPtr stateStream);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int SetStateOfPlugin(int pluginId, IntPtr stateStream, int streamSize);
   
   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetBusArrangement(IntPtr nativeAudProc);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int SetProcessing(int pluginId, int state);


   //PARAMETERS
   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetParameterCount(int pluginId);
   
   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetParameterInfo(int pluginId, int index, ref ParameterInfo paramInfo);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial double GetParameterNormalized(int pluginId, uint id);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int SetParameterNormalized(int pluginId, uint id, double paramValue);
   
   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int AddParameterData(int pluginId, UInt32 paramID, double newValue);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetParameterData(int pluginId, int index, out UInt32 paramID, out double value);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetOutputParameterChangesCount(int pluginId);

   
   //EVENTS
   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetEventCount(int pluginId);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetEvent(int pluginId, int index, ref Event evnt);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int AddEvent(int pluginId, ref Event evnt);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int ClearEvents(int pluginId);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetMidiControllerAssignment(int pluginId, int busIndex, Int16 channel, int ctrlNumber, out UInt32 id);
   


   //PLUGVIEW
   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int CreateAndShowEditor(int pluginId, IntPtr parentHandle, int platformType, IntPtr resizeCallback, IntPtr endEditCallback);
   //internal static partial int CreateAndShowEditor(int pluginId, IntPtr parentHandle, int platformType);

   [LibraryImport("Vst3Pont")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int GetEditorSize(int pluginId, ref int Width, ref int Height);

   [LibraryImport("Vst3Bridge")]
   [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
   internal static partial int CanResize(IntPtr plugViewPtr);

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   public delegate void ResizeViewCallback(IntPtr newSize);

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   public delegate void EndEditCallback();



}


