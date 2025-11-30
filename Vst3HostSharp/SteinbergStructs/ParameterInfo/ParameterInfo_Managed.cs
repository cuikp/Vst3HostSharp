using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Vst3HostSharp.HelperMethods;

namespace Vst3HostSharp;

public class ManagedParameterInfo
{
   public UInt32 Id;
   public string Title;
   public string ShortTitle;
   public string Units;
   public int StepCount;
   public double DefaultNormalizedValue;
   public int UnitId;
   public ParameterFlags Flags;

   public ManagedParameterInfo(ParameterInfo paraminfo)
   {
      unsafe
      {
         Id = paraminfo.id;
         Title = new string(paraminfo.title);
         ShortTitle = new string(paraminfo.shortTitle);
         Units = new string(paraminfo.units);
         StepCount = paraminfo.stepCount;
         DefaultNormalizedValue = paraminfo.defaultNormalizedValue;
         UnitId = paraminfo.unitId;
         Flags = (ParameterFlags)paraminfo.flags;
      }

   }

   public enum ParameterFlags
   {
      NoFlags = 0,		///< no flags wanted
		CanAutomate = 1 << 0,	///< parameter can be automated
		IsReadOnly = 1 << 1,	///< parameter cannot be changed from outside the plug-in - (implies that kCanAutomate is NOT set)
		IsWrapAround = 1 << 2,	///< attempts to set the parameter value out of the limits will result in a wrap around 
		IsList = 1 << 3,	///< parameter should be displayed as list in generic editor or automation editing 
		IsHidden = 1 << 4,  ///< parameter should be NOT displayed and cannot be changed from outside the plug-in - implies that kCanAutomate is NOT set and kIsReadOnly is set) [SDK 3.7.0]
      IsProgramChange = 1 << 15,   ///< parameter is a program change (unitId gives info about associated unit 
      IsBypass = 1 << 16,  ///< special bypass parameter (only one allowed): plug-in can handle bypass < (highly recommended to export a bypass parameter for effect plug-in)
      Unknown = 65545
   };

}

