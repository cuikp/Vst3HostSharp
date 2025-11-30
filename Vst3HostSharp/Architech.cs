using System.IO;
using System.Collections.Generic;

namespace Vst3HostSharp;

internal static class ArchitectureDetector
{
   public enum WinMachineType : ushort
   {
      Unknown = 0,
      I386 = 0x014c,   // x86
      AMD64 = 0x8664,   // x64
      ARM64 = 0xAA64    // ARM64
   }

   public static WinMachineType GetWinPeMachine(string path)
   {
      using var fs = File.OpenRead(path);
      using var br = new BinaryReader(fs);

      // check for "MZ"
      if (br.ReadUInt16() != 0x5A4D)
         return WinMachineType.Unknown;

      // e_lfanew at 0x3C
      fs.Seek(0x3C, SeekOrigin.Begin);
      var peOffset = br.ReadUInt32();

      // check for "PE\0\0"
      fs.Seek(peOffset, SeekOrigin.Begin);
      if (br.ReadUInt32() != 0x00004550)
         return WinMachineType.Unknown;

      // read Machine field
      var machine = (WinMachineType)br.ReadUInt16();
      return machine;
   }


   public static class MachO
   {
      const uint FAT_MAGIC = 0xCAFEBABE;
      const uint FAT_CIGAM = 0xBEBAFECA;
      const uint MH_MAGIC_64 = 0xfeedfacf;
      const uint MH_CIGAM_64 = 0xcffaedfe;
      const uint MH_MAGIC = 0xfeedface;
      const uint MH_CIGAM = 0xcefaedfe;

      public enum MacCpuType : uint
      {
         X86 = 7,
         ARM = 12,
         X86_64 = 0x01000007,
         ARM64 = 0x0100000C
      }

      public static IEnumerable<MacCpuType> GetMacArchitectures(string file)
      {
         using var fs = File.OpenRead(file);
         using var br = new BinaryReader(fs);

         uint magic = br.ReadUInt32();
         if (magic is FAT_MAGIC or FAT_CIGAM)
         {
            bool swap = magic == FAT_CIGAM;
            uint nFatArch = swap ? Swap(br.ReadUInt32()) : br.ReadUInt32();

            for (int i = 0; i < nFatArch; i++)
            {
               uint cputype = swap ? Swap(br.ReadUInt32()) : br.ReadUInt32();
               // skip rest of fat_arch (cpusubtype, offset, size, align)
               fs.Seek(16, SeekOrigin.Current);
               yield return (MacCpuType)cputype;
            }
         }
         else if (magic is MH_MAGIC_64 or MH_CIGAM_64 or MH_MAGIC or MH_CIGAM)
         {
            // thin binary: cputype is next
            bool isBe = (magic is MH_CIGAM_64 or MH_CIGAM);
            uint cputype = isBe ? Swap(br.ReadUInt32()) : br.ReadUInt32();
            yield return (MacCpuType)cputype;
         }
         // else unknown
      }

      static uint Swap(uint x) =>
          (x >> 24) |
          ((x >> 8) & 0x0000FF00) |
          ((x << 8) & 0x00FF0000) |
          (x << 24);
   }


}
