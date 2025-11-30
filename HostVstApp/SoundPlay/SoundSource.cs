using HostVstApp.ViewModels;
using libsndfile.NET8;
using PortAudioSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Vst3HostSharp;

namespace HostVstApp;

public partial class SoundSource
{
 
   readonly int FramesPerBuffer =256;
   readonly int finalFrameSize = 0;
   readonly int totalFrames = 0;
   readonly SndFile audioFile;
   readonly Stream stream;
   public float Volume { get; set; } = 1;
   internal bool playingBack = false;
   float cursor = 0;

   internal readonly ConcurrentBag<Vst3Plugin> ActiveVsts = []; 

   internal void CloseSoundSource()
   {
      stream.Dispose();
      PortAudio.Terminate();
   }

   public string SoundFileName { get; set; }
   public string SoundFileNameOnly => System.IO.Path.GetFileName(SoundFileName);

   public SoundSource(string fileName)
   {
      this.SoundFileName = fileName;
      PortAudio.LoadNativeLibrary();

      PortAudio.Initialize();

      // Try setting up an output device
      StreamParameters streamParams = new ()
      {   //set default parameters
         device = PortAudio.DefaultOutputDevice,
         channelCount = 2,
         sampleFormat = SampleFormat.Float32,
         suggestedLatency = PortAudio.GetDeviceInfo(PortAudio.DefaultOutputDevice).defaultLowOutputLatency,
         hostApiSpecificStreamInfo = IntPtr.Zero
      };
            
      audioFile = SndFile.OpenRead(fileName);

      //Debug.WriteLine("device=" + oParams.device + "\n" + audioFile.Info.samplerate);
      //Debug.WriteLine("deviceName=" + PortAudio.GetDeviceInfo(PortAudio.DefaultOutputDevice).name + "\nsamprate= " + audioFile.Info.samplerate);

      stream = new PortAudioSharp.Stream(null, streamParams, audioFile.Format.SampleRate, (uint)FramesPerBuffer,
          StreamFlags.NoFlag, PlayCallback, this);

      // Set how much data needs to be chunked out when playback happening
      finalFrameSize = audioFile.Format.Channels * FramesPerBuffer;
      totalFrames = audioFile.Format.Channels * (int)audioFile.Frames;

   }

   public void BeginPlay()
   {
      playingBack = true;
      stream.Start();

   }

   public void StopPlay()
   {
      playingBack = false;
      if (stream.IsActive)
         stream.Stop();
   }

   private readonly ReaderWriterLockSlim rwLock = new();

   private static float[] outputFloats = null!;
   private static float[] processingFloats = null!;

   private StreamCallbackResult PlayCallback(IntPtr input, IntPtr outputPtr, UInt32 frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr dataPtr)
   {// NOTE: make sure there are no malloc in this block, as it can cause issues.

      SoundSource data = Stream.GetUserData<SoundSource>(dataPtr);

      long numRead = 0;
      unsafe
      {
         // Reset to zero
         float* bufferStart = (float*)outputPtr;
         for (uint i = 0; i < data.finalFrameSize; i++)
            *bufferStart++ = 0;

         // If we are reading data, then play it back
         if (data.playingBack)
         {
            outputFloats = new float[data.finalFrameSize];

            rwLock.EnterReadLock();
            try
            {
               processingFloats = new float[data.finalFrameSize];
               
               numRead = audioFile.ReadItems(processingFloats, data.finalFrameSize);
                  for (int floatidx = 0; floatidx < numRead; floatidx++)
                     outputFloats[floatidx] += processingFloats[floatidx] * Volume; 

               ProcessVsts();
            }
            
            finally { rwLock.ExitReadLock(); }

            bufferStart = (float*)outputPtr;  // reset bufferStart location
            for (int i = 0; i < data.finalFrameSize; i++)
               *bufferStart++ = outputFloats[i];

         }
      }


      // Increment the counter
      data.cursor += numRead;

      // Did we hit the end?
      if (data.playingBack && (numRead < frameCount))
      {  // Stop playback, and reset to the beginning
         Debug.WriteLine("reached end");
         data.cursor = 0;
         audioFile.Seek(0, SfSeek.Begin);

      }

      // Continue on
      return StreamCallbackResult.Continue;
   }

}
