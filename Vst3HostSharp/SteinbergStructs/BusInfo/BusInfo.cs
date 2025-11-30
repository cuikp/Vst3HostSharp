using System.Runtime.InteropServices;

namespace Vst3HostSharp;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 16)]
public unsafe partial struct BusInfo
{
   public int mediaType;
   public int direction;
   public int channelCount;
   public fixed char name[128]; // 128 UTF-16 characters
   public int busType;
   public uint flags;

}


public enum BusFlag
{
   kDefaultActive = 1 << 0,
   kIsControlVoltage = 1 << 1
};

public enum MediaType
{
   kAudio = 0,    ///< audio
   kEvent,        ///< events
   kNumMediaTypes
};

public enum BusDirection
{
   kInput = 0,    ///< input bus
   kOutput        ///< output bus
};

public enum BusType
{
   kMain = 0,     ///< main bus
   kAux        ///< auxiliary bus (sidechain)
};

