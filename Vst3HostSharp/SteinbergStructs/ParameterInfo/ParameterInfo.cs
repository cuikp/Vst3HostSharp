using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Vst3HostSharp;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 16)]
//[StructLayout(LayoutKind.Sequential)]
public unsafe partial struct ParameterInfo
{
   internal UInt32 id;          // unique identifier of this parameter (named tag too)
   internal fixed char title[128];		// parameter title (e.g. "Volume")
   internal fixed char shortTitle[128];		// parameter title (e.g. "Volume")
   internal fixed char units[128];		// parameter title (e.g. "Volume")
	internal int stepCount;     //< number of discrete steps (0: continuous, 1: toggle, discrete value otherwise corresponding to max - min, for example: 127 for a min = 0 and a max = 127) 
   internal double defaultNormalizedValue;	// default normalized value [0,1] (in case of discrete value: defaultNormalizedValue = defDiscreteValue / stepCount)
	internal int unitId;       // id of unit this parameter belongs to (see \ref vst3Units)
   internal int flags;			// ParameterFlags (see below)

   
}


