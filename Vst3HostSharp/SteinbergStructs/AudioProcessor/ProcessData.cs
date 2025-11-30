using System;
using System.Runtime.InteropServices;
using ParamID = System.UInt32;
using ParamValue = double;

namespace Vst3HostSharp;

[StructLayout(LayoutKind.Explicit)]
internal struct AudioBusBuffers
{
   [FieldOffset(0)]
   internal int NumChannels;
   //[FieldOffset(4)]
   [FieldOffset(8)]
   internal ulong SilenceFlags;
   //[FieldOffset(12)]
   [FieldOffset(16)]
   internal IntPtr ChannelBuffers32;
   //[FieldOffset(12)]
   [FieldOffset(16)]
   internal IntPtr ChannelBuffers64;
}


[StructLayout(LayoutKind.Sequential)]
public struct ProcessData
{
   public ProcessData() { }

   public int ProcessMode;        ///< xrocessing mode - value of \ref ProcessMode
	public int SymbolicSampleSize;   ///< sample size - value of \ref SymbolicSampleSizes
	public int NumSamples;
   public int NumInputs;       ///< audio input busses
	public int NumOutputs;            ///< audio output busses

   internal IntPtr Inputs;  ///< AudioBusBuffers (input)
	internal IntPtr Outputs;  ///< AudioBusBuffers (output)

   public IntPtr InputParameterChanges;   ///< incoming parameter changes for this block
	public IntPtr OutputParameterChanges;  ///< outgoing parameter changes for this block (optional)
	public IntPtr InputEvents;           ///< incoming events for this block (optional)
	public IntPtr OutputEvents;          ///< outgoing events for this block (optional)
	public IntPtr DataProcessContext;			//optional

}

