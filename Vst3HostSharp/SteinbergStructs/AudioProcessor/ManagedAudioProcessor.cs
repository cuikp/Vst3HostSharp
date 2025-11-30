using System.Runtime.InteropServices;
using static Vst3HostSharp.HelperMethods;

namespace Vst3HostSharp;

public class ManagedAudioProcessor : IDisposable
{

   public ManagedProcessData managedProcessData;
   readonly int mainPluginId;
   List<GCHandle>[] _pinnedChannelBuffers;

   readonly IntPtr HostBufferPtr;

   public ManagedAudioProcessor(
      int pluginid,
      int numInputs,
      int numOutputs,
      ManagedBusInfo[] outBusInfos,
      ManagedBusInfo[] inBusInfos,
      int numSamples,
      IntPtr procData,
      IntPtr hostBufferPtr
      )
   {
      unsafe
      {
         mainPluginId = pluginid;

         ProcessData pdata = Marshal.PtrToStructure<ProcessData>(procData);

         this.HostBufferPtr = hostBufferPtr;

         managedProcessData = new ManagedProcessData(pdata)
         {
            NumInputs = numInputs,
            NumOutputs = numOutputs,
            OutputChannelCounts = [.. outBusInfos.Select(obi=> obi.ChannelCount)],
            InputChannelCounts = [.. inBusInfos.Select(ibi => ibi.ChannelCount)],
            NumSamples = numSamples
         };

         _pinnedChannelBuffers = new List<GCHandle>[numInputs + numOutputs];

         // PIN INPUTS (so host writes directly into them)
         for (int busno = 0; busno < numInputs; busno++)
         {
            int chansIn = managedProcessData.InputChannelCounts[busno];
            _pinnedChannelBuffers[busno] = new List<GCHandle>(chansIn);
            var input = managedProcessData.Inputs[busno];
            input.BusType = inBusInfos[busno].BusType;
            input.Name = inBusInfos[busno].Name ?? "Unknown";
            input.ChannelBuffers32 = new float[chansIn][];
            input.ChannelBufferPointers = [];

            for (int ch = 0; ch < chansIn; ch++)
            {
               var buf = new float[numSamples];
               input.ChannelBuffers32[ch] = buf;
               var pin = GCHandle.Alloc(buf, GCHandleType.Pinned);
               _pinnedChannelBuffers[busno].Add(pin);
               input.ChannelBufferPointers.Add(pin.AddrOfPinnedObject());
            }
         }

         PatchNativeInputPointers(procData, managedProcessData);


         // PIN OUTPUTS (so plugin writes directly into them)
         for (int busno = 0; busno < numOutputs; busno++)
         {
            int slot = numInputs + busno;
            int chans = managedProcessData.OutputChannelCounts[busno];
            _pinnedChannelBuffers[slot] = new List<GCHandle>(chans);
            var output = managedProcessData.Outputs[busno];
            output.BusType = outBusInfos[busno].BusType;
            output.Name = outBusInfos[busno].Name ?? "Unknown";

            output.ChannelBuffers32 = new float[chans][];
            output.ChannelBufferPointers = [];

            for (int ch = 0; ch < chans; ch++)
            {
               var buf = new float[numSamples];
               output.ChannelBuffers32[ch] = buf;
               var pin = GCHandle.Alloc(buf, GCHandleType.Pinned);
               _pinnedChannelBuffers[slot].Add(pin);
               output.ChannelBufferPointers.Add(pin.AddrOfPinnedObject());
            }
         }

         PatchNativeOutputPointers(procData, managedProcessData);

      }
   }

   unsafe void PatchNativeOutputPointers(IntPtr procDataPtr, ManagedProcessData mpd)
   {
      byte* basePtr = (byte*)procDataPtr.ToPointer();
      int offOutputs = Marshal.OffsetOf<ProcessData>("Outputs").ToInt32();

      IntPtr outsArrayPtr = new(*(IntPtr*)(basePtr + offOutputs));

      int busSize = Marshal.SizeOf<AudioBusBuffers>();
      int offChanArr = Marshal.OffsetOf<AudioBusBuffers>("ChannelBuffers32").ToInt32();

      for (int b = 0; b < mpd.NumOutputs; b++)
      {
         byte* busPtr = (byte*)outsArrayPtr.ToPointer() + b * busSize;
         IntPtr chanArrPtr = new(*(IntPtr*)(busPtr + offChanArr));
         for (int ch = 0; ch < mpd.Outputs[b].NumChannels; ch++)
         {
            Marshal.WriteIntPtr(chanArrPtr, ch * IntPtr.Size, mpd.Outputs[b].ChannelBufferPointers[ch]);
         }
      }
   }

   unsafe void PatchNativeInputPointers(IntPtr procDataPtr, ManagedProcessData mpd)
   {
      byte* basePtr = (byte*)procDataPtr.ToPointer();
      int offInputs = Marshal.OffsetOf<ProcessData>("Inputs").ToInt32();
      IntPtr insArray = new(*(IntPtr*)(basePtr + offInputs));

      int busSize = Marshal.SizeOf<AudioBusBuffers>();
      int offChan = Marshal.OffsetOf<AudioBusBuffers>("ChannelBuffers32").ToInt32();

      for (int b = 0; b < mpd.NumInputs; b++)
      {
         byte* busPtr = (byte*)insArray.ToPointer() + b * busSize;
         IntPtr chanArrPtr = new(*(IntPtr*)(busPtr + offChan));
         // overwrite each channel pointer
         for (int ch = 0; ch < mpd.Inputs[b].NumChannels; ch++)
         {
            Marshal.WriteIntPtr(chanArrPtr, ch * IntPtr.Size, mpd.Inputs[b].ChannelBufferPointers[ch]);
         }
      }
   }


   public unsafe void PerformProcessData()
   {
      NativeMethods.Process(mainPluginId);

   }

   public int SetProcessing(bool state)
   {
      return NativeMethods.SetProcessing(mainPluginId, state ? 1 : 0);
   }

   public int ClearEvents()
   {
      return NativeMethods.ClearEvents(mainPluginId);
   }

   public int AddEvent(ref Event newEvent)
   {
      int returnInt = NativeMethods.AddEvent(mainPluginId, ref newEvent);

      //LogOutput( (returnInt == 0 ? "Succeeded in adding" : "Failed to add") + " event: " + returnInt + " :: " + newEvent.type.ToString() + " :: " + (newEvent.type == 0 ? newEvent.noteOn.pitch : (newEvent.type == 1 ? newEvent.noteOff.pitch : -1)));

      return returnInt;

   }

   public int AddParameterData(UInt32 ParamID, double value)
   {
      int returnInt = NativeMethods.AddParameterData(mainPluginId, ParamID, value);

      //LogOutput( (returnInt == 0 ? "Succeeded in adding" : "Failed to add") + " Parameter data at value: " + value);

      return returnInt;

   }

   public int GetOutputParameterChangesCount() { return NativeMethods.GetOutputParameterChangesCount(mainPluginId); }

   public int GetParameterData(int index, out UInt32 ParamID, out double value)
   {
      return NativeMethods.GetParameterData(mainPluginId, index, out ParamID, out value);
   }

   public Event GetEvent(int index)
   {
      Event ev = new();
      int result = NativeMethods.GetEvent(mainPluginId, index, ref ev);
      return ev;

   }

   public void Dispose()
   {

      if (_pinnedChannelBuffers != null)
      {
         foreach (var pinList in _pinnedChannelBuffers)
         {
            if (pinList == null) continue;
            foreach (var pin in pinList)
               if (pin.IsAllocated)
                  pin.Free();
         }
      }

      _pinnedChannelBuffers = null!;

      managedProcessData.Dispose();

      managedProcessData = null!;


      //processData = IntPtr.Zero;


   }

}

