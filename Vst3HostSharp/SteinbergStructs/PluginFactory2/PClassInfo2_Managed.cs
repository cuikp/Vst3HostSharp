using static Vst3HostSharp.HelperMethods;

namespace Vst3HostSharp;

public class ManagedPClassInfo // : IDisposable
{
    public Guid Cid;
    public int Cardinality;
    public String Category = "";
    public String Name = "";
    public uint ClassFlags;
    public String SubCategories = "";
    public String Vendor = "";
    public String Version = "";
    public String SdkVersion = "";


    public ManagedPClassInfo(PClassInfoW pinfW)
    {
      
        unsafe
        {
            Cid = pinfW.cid;
            Cardinality = pinfW.cardinality;
            Category = StringFromByte(pinfW.category, 32);
            Name = StringFromChar(pinfW.name, 64);
            ClassFlags = pinfW.classFlags;
            SubCategories = StringFromByte(pinfW.subCategories, 128);
            Vendor = StringFromChar(pinfW.vendor, 64);
            Version = StringFromChar(pinfW.version, 64);
            SdkVersion = StringFromChar(pinfW.sdkVersion, 64);

        }
    }
   
    public ManagedPClassInfo(PClassInfo2 pinf2)
    {
        unsafe
        {
            Cid = pinf2.cid;
            Cardinality = pinf2.cardinality;
            Category = StringFromByte(pinf2.category, 32);
            Name = StringFromByte(pinf2.name, 64);
            ClassFlags = pinf2.classFlags;
            SubCategories = StringFromByte(pinf2.subCategories, 128);
            Vendor = StringFromByte(pinf2.vendor, 64);
            Version = StringFromByte(pinf2.version, 64);
            SdkVersion = StringFromByte(pinf2.sdkVersion, 64);

        }

    }

   public ManagedPClassInfo(PClassInfo pinf)
   {
      unsafe
      {
         Cid = pinf.cid;
         Cardinality = pinf.cardinality;
         Category = StringFromByte(pinf.category, 32);
         Name = StringFromByte(pinf.name, 64);
      }

   }

   //public void Dispose()
   //{
   //   ////  what goes here???????
      
   //}
  
   //~ManagedPClassInfo()
   //{
   //   Dispose();
   //}


}


