
namespace Vst3HostSharp;

public class ManagedBusInfo
{
   public int MediaType;
   public int Direction;
   public int ChannelCount;
   public string? Name;
   public int BusType;
   public uint Flags;

   public ManagedBusInfo(BusInfo businfo)
    {
        unsafe
        {
         MediaType = businfo.mediaType;
         Direction = businfo.direction;
         ChannelCount = businfo.channelCount;
         Name = new string(businfo.name);
         BusType = businfo.busType;
         Flags = businfo.flags;
        }
    }

}

