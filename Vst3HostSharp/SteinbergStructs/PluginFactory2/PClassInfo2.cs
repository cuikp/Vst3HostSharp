using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Vst3HostSharp;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 16)]
public unsafe partial struct PClassInfoW
{
   public Guid cid;
   public int cardinality;
   public fixed byte category[32];
   public fixed char name[64];
   public uint classFlags;
   public fixed byte subCategories[128];
   public fixed char vendor[64];
   public fixed char version[64];
   public fixed char sdkVersion[64];
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 16)]
public unsafe partial struct PClassInfo2
{
    public Guid cid;
    public int cardinality;
    public fixed byte category[32];
    public fixed byte name[64];
    public uint classFlags;
    public fixed byte subCategories[128];
    public fixed byte vendor[64];
    public fixed byte version[64];
    public fixed byte sdkVersion[64];
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 16)]
public unsafe partial struct PClassInfo
{
    public Guid cid;
    public int cardinality;
    public fixed byte category[32];
    public fixed byte name[64];
}

