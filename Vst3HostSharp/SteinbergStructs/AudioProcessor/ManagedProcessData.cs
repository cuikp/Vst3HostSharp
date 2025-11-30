using System.Runtime.InteropServices;

namespace Vst3HostSharp;

public enum SymbolicSampleSizes
{
   Sample32,		///< 32-bit precision
	Sample64		///< 64-bit precision
};


public class ManagedProcessData
{
   public int ProcessMode = 0;
   public int SymbolicSampleSize = 0;
   public int NumSamples = 0;
   public int NumInputs = 0;
   public int NumOutputs = 0;
   public int[] OutputChannelCounts = [];
   public int[] InputChannelCounts = [];

   public ManagedAudioBusBuffers[] Inputs = null!;
   public ManagedAudioBusBuffers[] Outputs = null!;

 
   public ManagedProcessData(ProcessData procData)
   {
      unsafe
      {
         ProcessMode = procData.ProcessMode;
         SymbolicSampleSize = procData.SymbolicSampleSize;
         NumSamples = procData.NumSamples;
         NumInputs = procData.NumInputs;
         NumOutputs = procData.NumOutputs;

         Inputs = new ManagedAudioBusBuffers[NumInputs];
         Outputs = new ManagedAudioBusBuffers[NumOutputs];
                  
         IntPtr currPtrIn = procData.Inputs;

         for (int i = 0; i < NumInputs; i++)
         {  
            AudioBusBuffers inputBuffer = Marshal.PtrToStructure<AudioBusBuffers>(currPtrIn);
            int numChannelsIn = inputBuffer.NumChannels;
            IntPtr channelBuffersArrayPtr = inputBuffer.ChannelBuffers32;

            Inputs[i] = new ManagedAudioBusBuffers(numChannelsIn, NumSamples, true)
            {
               ChannelBuffers32 = new float[numChannelsIn][],
               //first audio buffer is main so only first should be activated
               IsActivated = (i == 0),
            };

            for (int chno = 0; chno < numChannelsIn; chno++)
            {
               IntPtr channelPtrIn = Marshal.ReadIntPtr(channelBuffersArrayPtr, chno * sizeof(IntPtr)); // Offset to the correct channel pointer
               Inputs[i].ChannelBufferPointers.Add(channelPtrIn);
               Inputs[i].ChannelBuffers32![chno] = new float[NumSamples];
            }

            currPtrIn = IntPtr.Add(currPtrIn, sizeof(AudioBusBuffers));
         }

         IntPtr currPtrOut = procData.Outputs;

         for (int i = 0; i < NumOutputs; i++)
         {
            AudioBusBuffers outputBuffer = Marshal.PtrToStructure<AudioBusBuffers>(currPtrOut);

            int numChannelsOut = outputBuffer.NumChannels;
            IntPtr channelBuffersArrayPtr = outputBuffer.ChannelBuffers32;

            Outputs[i] = new ManagedAudioBusBuffers(numChannelsOut, NumSamples, true)
            {
               ChannelBuffers32 = new float[numChannelsOut][],
               //first audio buffer is main so only first should be activated
               IsActivated = (i == 0),
            };

            for (int chno = 0; chno < numChannelsOut; chno++)
            {
               IntPtr channelPtrOut = Marshal.ReadIntPtr(channelBuffersArrayPtr, chno * sizeof(IntPtr));
               Outputs[i].ChannelBufferPointers.Add(channelPtrOut);
               Outputs[i].ChannelBuffers32[chno] = new float[NumSamples];
            }

            currPtrOut = IntPtr.Add(currPtrOut, Marshal.SizeOf<AudioBusBuffers>());
         }
      }

   }

   private bool disposed = false; // To detect redundant calls

   public void Dispose()
   {
      Dispose(true);
      GC.SuppressFinalize(this);
   }

   protected virtual void Dispose(bool disposing)
   {
      if (disposed)
         return;
      
      if (disposing)
      {
         // Dispose managed state (managed objects).
         if (Inputs != null)
         {
            foreach (var input in Inputs)
               input.Dispose();
         }

         if (Outputs != null)
         {
            foreach (var output in Outputs)
               output.Dispose();
         }
      }

      disposed = true;

   }
    

}


