using System;
using Vst3HostSharp;

namespace HostVstApp;

public partial class SoundSource
{
   private void ProcessVsts()
   {
      //lock (ActiveVsts)
      //{
      foreach (Vst3Plugin vst in ActiveVsts)
      {
         if (vst.IsActive)
         {

            var vstProcData = vst.AudioProcessor.managedProcessData;
            if (vst.PluginType == StructsAndEnums.VstType.VstEffect)
            {
               if (vstProcData.SymbolicSampleSize == (int)SymbolicSampleSizes.Sample32)
               {
                  for (int busNo = 0; busNo < vst.AudioProcessor.managedProcessData.Inputs!.Length; busNo++)
                  {
                     int numChannels = vstProcData.Inputs[busNo].NumChannels;

                     for (int chno = 0; chno < numChannels; chno++)
                     {// Debug.WriteLine("channelNoB4 = " + chno + " ::: numsamples = " + vst.AudioProcessor.managedProcessData.NumSamples);

                        for (int j = 0; j < vst.AudioProcessor.managedProcessData.NumSamples; j++)
                           if (numChannels == 1)
                           { //MONO VST3
                              float left = outputFloats[j * 2];       // Left channel is at even indices
                              float right = outputFloats[j * 2 + 1]; // Right channel is at odd indices
                              vstProcData.Inputs[busNo].ChannelBuffers32![chno][j] = (left + right) / 2f;
                           }
                           else
                           {  //Stereo in
                              vstProcData.Inputs[busNo].ChannelBuffers32![chno][j] = outputFloats[j * numChannels + chno];
                           }
                     }
                  }
               }
               else
               {
                  for (int busNo = 0; busNo < vst.AudioProcessor.managedProcessData.Inputs!.Length; busNo++)
                  {
                     for (int i = 0; i < vstProcData.Inputs[busNo].NumChannels; i++)
                     {
                        for (int j = 0; j < vst.AudioProcessor.managedProcessData.NumSamples; j++)
                           vstProcData.Inputs[busNo].ChannelBuffers64![i][j] = (double)outputFloats[i * vstProcData.Inputs[busNo].NumChannels + j];
                     }
                  }
               }


            }

            //process audio
            vst.AudioProcessor.PerformProcessData();

            //replace outputFloats with effect output, whereas vstInstrument adds
            if (vst.PluginType == StructsAndEnums.VstType.VstEffect)
               Array.Clear(outputFloats, 0, outputFloats.Length);

            //copy processed data back
            if (vstProcData.SymbolicSampleSize == (int)SymbolicSampleSizes.Sample32)
            {

               for (int busNo = 0; busNo < vst.AudioProcessor.managedProcessData.Outputs!.Length; busNo++)
               {
                  //Debug.WriteLine("numchannels this Output (" + busNo + ") = " + vstProcData.Outputs[busNo].OutputChannelCounts);

                  int numChannels = vstProcData.Outputs[busNo].NumChannels;

                  for (int chno = 0; chno < vstProcData.Outputs[busNo].NumChannels; chno++)
                  {  //Debug.WriteLine("channelNoAf = " + chno + " ::: numsamples = " + vst.AudioProcessor.managedProcessData.NumSamples);

                     for (int j = 0; j < vst.AudioProcessor.managedProcessData.NumSamples; j++)
                     {
                        if (numChannels == 1)
                        { // MONO OUTPUT FROM VST3 to stereo: duplicate half the mono output to both stereo channels
                           float splitFloat = vstProcData.Outputs[busNo].ChannelBuffers32![chno][j] / 2f;
                           outputFloats[j * 2] += splitFloat;     // Left channel
                           outputFloats[j * 2 + 1] += splitFloat; // Right channel
                        }
                        else if (numChannels == 2)
                        { // Stereo output  
                           outputFloats[j * numChannels + chno] += vstProcData.Outputs[busNo].ChannelBuffers32![chno][j];
                        }

                     }

                  }

               }
            }
            else
            {
               for (int busNo = 0; busNo < vst.AudioProcessor.managedProcessData.Outputs!.Length; busNo++)
                  for (int i = 0; i < vstProcData.Outputs[busNo].NumChannels; i++)
                  {
                     for (int j = 0; j < vst.AudioProcessor.managedProcessData.NumSamples; j++)
                        outputFloats[j * vstProcData.Inputs[busNo].NumChannels + i] = (float)vstProcData.Outputs[busNo].ChannelBuffers64![i][j];
                  }
            }
         }
      }

      //}

   }



}
