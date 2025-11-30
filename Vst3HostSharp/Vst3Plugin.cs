using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Vst3HostSharp.Logging;
using static Vst3HostSharp.StructsAndEnums;

namespace Vst3HostSharp;

public partial class Vst3Plugin(string filePath) : IDisposable, INotifyPropertyChanged
{
   public event PropertyChangedEventHandler? PropertyChanged;
   private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

   public delegate void PluginSize_ChangedHandler(double newWidth, double newHeight);
   public event PluginSize_ChangedHandler? PluginSize_Changed;

   public delegate void EndEdit_CalledHandler();
   public event EndEdit_CalledHandler? EndEdit_Called;

   public string VstFilePath { get; private set; } = filePath;

   public byte[] StateChunk = null!;
   public ManagedAudioProcessor AudioProcessor = null!;

   public int ClassCount = 0;
   public int OutputBussesCount = 0;
   public int InputBussesCount = 0;
   internal int[] OutputChannelCounts = [];
   internal int[] InputChannelCounts = [];
   internal int[] OutputBusTypes = [];
   internal int[] InputBusTypes = [];
   internal string[] OutputBusNames = [];
   internal string[] InputBusNames = [];

   private int myPluginId = -1;
   public bool HasEditor { get; set; } = false;
   internal bool Initialized = false;
   public bool IsEditorViewCreated = false;

   private bool _IsActive = true;
   public bool IsActive { get => _IsActive; set { _IsActive = value; NotifyPropertyChanged(nameof(IsActive)); } }

   private string _PluginLoadResults = "";
   public string PluginLoadResults { get => _PluginLoadResults; set { _PluginLoadResults = value; NotifyPropertyChanged(nameof(PluginLoadResults)); } }
   public List<string> DebugLines { get; set; } = [];

   private VstType _PluginType = VstType.VstUnknown;
   public VstType PluginType { get => _PluginType; set { _PluginType = value; NotifyPropertyChanged(nameof(PluginTypeString)); } }
   public VstType PluginTypeString => (VstType)PluginType;

   public int GetParameterCount() { return NativeMethods.GetParameterCount(myPluginId); }

   public ManagedParameterInfo GetParameterInfo(int index)
   {
      ParameterInfo paramInfo = new();
      NativeMethods.GetParameterInfo(myPluginId, index, ref paramInfo);
      ManagedParameterInfo mParamInfo = new(paramInfo);
      return mParamInfo;
   }

   public double GetParameterNormalized(uint id) { return NativeMethods.GetParameterNormalized(myPluginId, id); }
   public int SetParameterNormalized(uint id, double paramValue) { return NativeMethods.SetParameterNormalized(myPluginId, id, paramValue); }

   public int GetMidiControllerAssignment(int busIndex, Int16 channel, int ctrlNumber, out UInt32 id)
   {
      return NativeMethods.GetMidiControllerAssignment(myPluginId, busIndex, channel, ctrlNumber, out id);
   }

   public object? PlugViewParent;

   bool _disposed;


   public void Dispose()
   {
      Debug.WriteLine("Calling dispose on " + this.VstFilePath);

      if (_disposed) return;
      _disposed = true;

      int disposeResult = NativeMethods.DisposePlugin(myPluginId);

      AudioProcessor?.Dispose();

      endEditCallback = null;
      resizeViewCallback = null;

      GC.SuppressFinalize(this);

      Debug.WriteLine("vst disposed");

   }

   ~Vst3Plugin()
   {
      Dispose(); 
   }

   public IntPtr KeepPluginDllHandle = IntPtr.Zero;

   public void InitializePlugin(int bufferSize, double sampleRate, IntPtr hostBufferPtr)
   {
      NativeMethods.SetLogFilePath(outputLogFile);

      int hasEditor = 0;

      LogOutput("\ninitializing plugin...");
      LogOutput("got hostBufferPtr: " + hostBufferPtr);

      int newPluginId = NativeMethods.LoadVst3Ext(this.VstFilePath, ref hasEditor);

      LogOutput("got new pluginid = " + newPluginId);

      if (newPluginId != -1)
      {
         myPluginId = newPluginId;

         GeneralBufferSize = bufferSize;
         SampleRate = sampleRate;

         CreateAudioProcessor(hostBufferPtr);

         Initialized = true;
         HasEditor = hasEditor == 1;
      }


   }

   public bool IsPlugViewAttached = false;
   private NativeMethods.ResizeViewCallback? resizeViewCallback;
   private NativeMethods.EndEditCallback? endEditCallback;

   public bool ShowEditor(IntPtr platformHandle, int platformType)
   {

      resizeViewCallback = new NativeMethods.ResizeViewCallback(OnResizeView);
      IntPtr resizeViewCallbackPtr = Marshal.GetFunctionPointerForDelegate(resizeViewCallback);
      endEditCallback = new NativeMethods.EndEditCallback(OnEndEdit);
      IntPtr endEditCallbackPtr = Marshal.GetFunctionPointerForDelegate(endEditCallback);

      int showResult = NativeMethods.CreateAndShowEditor(myPluginId, platformHandle, platformType, resizeViewCallbackPtr, endEditCallbackPtr);

      return showResult == 0;

   }

   private void OnResizeView(IntPtr newSize)
   {
      ViewRect vRect = Marshal.PtrToStructure<ViewRect>(newSize);

      Debug.WriteLine("ResizeView callback invoked");

      PluginSize_Changed?.Invoke((double)vRect.right, (double)vRect.bottom);

   }

   private void OnEndEdit() { EndEdit_Called?.Invoke(); }

   //public void CloseEditor()
   //{
   //    RemoveAttachedPlugView();

   //}

   int GeneralBufferSize = 256;  //default: change to 512, 1024, 2048 if stuttering/glitching occurs
   double SampleRate = 41000;

   //internal IntPtr nativeProcData = IntPtr.Zero;

   public void ResetAudioProcessor(int newBufferSize, double newSampleRate, IntPtr hostBufferPtr)
   {
      GeneralBufferSize = newBufferSize;
      SampleRate = newSampleRate;

      CreateAudioProcessor(hostBufferPtr);
   }

   internal void CreateAudioProcessor(IntPtr hostBufferPtr)
   {
      IntPtr procDataPtr = IntPtr.Zero;

      int parameterCount = GetParameterCount();

      int createAudProcResult = NativeMethods.SetupAudioProcessor(myPluginId, GeneralBufferSize, ref procDataPtr, parameterCount, SampleRate);

      LogOutput($"*Resetting to General BufferSize = {GeneralBufferSize}, samplerate = {SampleRate}");
      //LogOutput("Setup Audio Processor: " + createAudProcResult);

      ProcessData nativeProcData = Marshal.PtrToStructure<ProcessData>(procDataPtr);

      //Get number of channels for each out bus
      OutputBussesCount = nativeProcData.NumOutputs;
      InputBussesCount = nativeProcData.NumInputs;

      ManagedBusInfo[] OutputBusInfos = new ManagedBusInfo[OutputBussesCount];
      ManagedBusInfo[] InputBusInfos = new ManagedBusInfo[InputBussesCount];


      for (int i = 0; i < nativeProcData.NumOutputs; i++)
         GetBusInfo(MediaType.kAudio, BusDirection.kOutput, i, ref OutputBusInfos[i]);

      for (int i = 0; i < nativeProcData.NumInputs; i++)
         GetBusInfo(MediaType.kAudio, BusDirection.kInput, i, ref InputBusInfos[i]);


      AudioProcessor = new ManagedAudioProcessor(
         myPluginId,
         nativeProcData.NumInputs,
         nativeProcData.NumOutputs,
         OutputBusInfos,
         InputBusInfos,
         GeneralBufferSize,
         procDataPtr,
         hostBufferPtr
         );

      LogOutput("Created new audioProcessor: " + AudioProcessor.managedProcessData.NumSamples);

   }

   public int GetBusInfo(MediaType mediaType, BusDirection busdirection, int index, ref ManagedBusInfo mBusInfo)
   {
      BusInfo businfo = new();
      int businfores = NativeMethods.GetBusInfo(myPluginId, (int)mediaType, (int)busdirection, index, ref businfo);
      mBusInfo = new ManagedBusInfo(businfo);

      return businfores;
   }

   public int GetBusCount(MediaType mediaType, BusDirection busdirection)
   {
      return NativeMethods.GetBusCount(myPluginId, (int)mediaType, (int)busdirection);
   }

   public int ActivateBus(MediaType mediaType, BusDirection busdirection, int index, bool state)
   {
      int returnVal = NativeMethods.ActivateBus(myPluginId, (int)mediaType, (int)busdirection, index, state ? (byte)1 : (byte)0);

      if (returnVal == 0)
      {  //successfully activated or deactivated bus
         switch (busdirection)
         {
            case BusDirection.kOutput:
               this.AudioProcessor.managedProcessData.Outputs[index].IsActivated = state;
               break;
            case BusDirection.kInput:
               this.AudioProcessor.managedProcessData.Inputs[index].IsActivated = state;
               break;
         }
      }
      return returnVal;
   }


   public byte[] GetState()
   {

      IntPtr unmanagedPointer;
      long size;
      int result = NativeMethods.GetStateOfPlugin(myPluginId, out unmanagedPointer, out size);

      //return new byte[5];


      Debug.WriteLine($"Result of getting state of: {this.VstFilePath} = {result}");

      if (result == 0)
      {
         byte[] managedData = new byte[size];
         Marshal.Copy(unmanagedPointer, managedData, 0, (int)size);
         NativeMethods.ReleaseStateStream(unmanagedPointer);
         return managedData;
      }

      return null!;

   }

   public int SetState(byte[] stateChunk)
   {
      //return NativeMethods.SetStateOfPlugin(myPluginId, IntPtr.Zero, stateChunk.Length);


      int returnResult = -1;

      GCHandle handle = GCHandle.Alloc(stateChunk, GCHandleType.Pinned);
      try
      {
         IntPtr stateChunkPtr = handle.AddrOfPinnedObject();
         if (stateChunkPtr == IntPtr.Zero) return -1;
         returnResult = NativeMethods.SetStateOfPlugin(myPluginId, stateChunkPtr, stateChunk.Length);
      }
      finally
      {
         handle.Free();
      }

      return returnResult;


   }

   public int GetEditorSize(ref int Width, ref int Height)
   {
      return NativeMethods.GetEditorSize(myPluginId, ref Width, ref Height);
   }

}