namespace Vst3HostSharp;

public class ManagedAudioBusBuffers : IDisposable
{
   public int BusType { get; set; } = 0;
   public bool IsActivated { get; set; } = false;
   public string RoutingID { get; set; } = Guid.NewGuid().ToString();
   public string Name { get; set; } = "";

   public int NumChannels { get; private set; }
   public int NumSamples { get; private set; }
   public UInt64 SilenceFlags { get; private set; }

   public float[][] ChannelBuffers32 = null!;   
   public double[][] ChannelBuffers64 = null!;

   public List<IntPtr> ChannelBufferPointers = [];

   public ManagedAudioBusBuffers(int numChannels, int numSamples, bool is32Bit) 
	{
      //Allocate memory for each channel
      int sampleSize = is32Bit ? sizeof(float) : sizeof(double);
      
      NumChannels = numChannels;
      NumSamples = numSamples;
      SilenceFlags = 0; // audioBusBuffers.SilenceFlags;

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
         // Free any other managed objects here.
      }

      for (int i = 0; i < ChannelBufferPointers.Count; i++)
         ChannelBufferPointers[i] = IntPtr.Zero;

      //// Free any unmanaged objects here.
      //foreach (var ptr in ChannelBufferPointers)
      //{
      //   if (ptr != IntPtr.Zero)
      //   {
      //      Marshal.FreeHGlobal(ptr);
      //   }
      //}

      disposed = true;
   }

   

}



