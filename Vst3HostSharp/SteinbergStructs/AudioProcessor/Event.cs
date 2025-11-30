using System;
using System.Runtime.InteropServices;

namespace Vst3HostSharp;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct Event
{
   [FieldOffset(0)]
   public int busIndex;
   [FieldOffset(4)]
   public int sampleOffset;
   [FieldOffset(8)]
   public double ppqPosition;
   [FieldOffset(16)]
   public ushort flags;
   [FieldOffset(18)]
   public ushort type;
   [FieldOffset(20)]
   public int padding; //extra padding of 4
   [FieldOffset(24)]
   public NoteOnEvent noteOn;
   [FieldOffset(24)]
   public NoteOffEvent noteOff;
   [FieldOffset(24)]
   public NoteExpressionValueEvent noteExpressionValueEvent;
   //[FieldOffset(24)]
   //public UnmanagedDataEvent data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NoteOnEvent
{
   public short channel;
   public short pitch;
   public float tuning;
   public float velocity;
   public int length;
   public int noteId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NoteOffEvent
{
   public short channel;
   public short pitch;
   public float tuning;
   public float velocity;
   public int noteId;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NoteExpressionValueEvent
{
   public uint typeId;
   public int noteId;
   public double value;
}


//[StructLayout(LayoutKind.Sequential, Pack = 1)]
//public struct UnmanagedDataEvent
//{
//   public uint size;
//   public uint type;
//   public IntPtr bytes;
//}

//enum DataTypes
//{   
//   kMidiSysEx = 0	///< for MIDI system exclusive message
//}

///** PolyPressure event specific data. Used in \ref Event (union) \ingroup vstEventGrp*/
//public class PolyPressureEvent
//{
//   Int16 Channel;		///< channel index in event bus
//	Int16 Pitch;		///< range [0, 127] = [C-2, G8] with A3=440Hz
//	float Pressure;		///< range [0.0, 1.0]
//	int NoteId;		///< event should be applied to the noteId (if not -1)
//}

///** Chord event specific data. Used in \ref Event (union) \ingroup vstEventGrp*/
//public class ChordEvent 
//{
//	Int16 Root;			///< range [0, 127] = [C-2, G8] with A3=440Hz
//	Int16 BassNote;		///< range [0, 127] = [C-2, G8] with A3=440Hz
//	Int16 Mask;			///< root is bit 0
//	UInt16 TextLen;    ///< the number of characters (TChar) between the beginning of text and the terminating
//                              ///< null character (without including the terminating null character itself)
//   //TChar^ Text;	///< UTF-16, null terminated Hosts Chord Name
//	//byte[] TextBytes;
//	string Text = "";
//}

///** Scale event specific data. Used in \ref Event (union) \ingroup vstEventGrp*/
//public class ScaleEvent 
//{
//	Int16 Root;			///< range [0, 127] = root Note/Transpose Factor
//	Int16 Mask;			///< Bit 0 =  C,  Bit 1 = C#, ... (0x5ab5 = Major Scale)
//	UInt16 TextLen;    ///< the number of characters (TChar) between the beginning of text and the terminating
//                              ///< null character (without including the terminating null character itself)
//   //TChar^ Text;	///< UTF-16, null terminated, Hosts Scale Name
//   string Text = "";	///< UTF-16, null terminated, Hosts Scale Name
//}

///** Legacy MIDI CC Out event specific data. Used in \ref Event (union) \ingroup vstEventGrp- [released: 3.6.12]
//This kind of event is reserved for generating MIDI CC as output event for kEvent Bus during the process call.*/
//public class LegacyMIDICCOutEvent 
//{
//	byte ControlNumber;///< see enum ControllerNumbers [0, 255]
//	sbyte Channel;		///< channel index in event bus [0, 15]
//	sbyte Value;			///< value of Controller [0, 127]
//	sbyte Value2;		///< [0, 127] used for pitch bend (kPitchBend) and polyPressure (kCtrlPolyPressure)
//}


///** Note Expression Text event. Used in Event (union)
//A Expression event affects one single playing note. \sa INoteExpressionController \see NoteExpressionTypeInfo */

//public class NoteExpressionTextEvent 
//{
//   uint TypeId;	///< see \ref NoteExpressionTypeID (kTextTypeID or kPhoneticTypeID)
//	int NoteId;			///< associated note identifier to apply the change
//	uint TextLen;       ///< the number of characters (TChar) between the beginning of text and the terminating
//							  ///< null character (without including the terminating null character itself)
//	//TChar^ Text;				///< UTF-16, null terminated
//	string Text = "";
//}

//enum EventFlags
//{
//   kIsLive = 1 << 0,			///< indicates that the event is played live (directly from keyboard)
//	kUserReserved1 = 1 << 14,	
//	kUserReserved2 = 1 << 15	
//}

public enum EventTypes : ushort
{
   KNoteOnEvent = 0,
   KNoteOffEvent = 1,
   KDataEvent = 2,
   KPolyPressureEvent = 3,
   KNoteExpressionValueEvent = 4,
   KNoteExpressionTextEvent = 5,
   KChordEvent = 6,
   KScaleEvent = 7,
   KLegacyMIDICCOutEvent = 65535
}

////Managed DataEvent while keeping name
//public class DataEvent : IDisposable
//{
//   public UnmanagedDataEvent unmanagedDataEvent;
//   private byte[] _dataBytes;
//   private GCHandle _handle;

//   public DataEvent(byte[] midiBytes, uint type)
//   {
//      _dataBytes = midiBytes;
//      _handle = GCHandle.Alloc(_dataBytes, GCHandleType.Pinned);

//      unmanagedDataEvent = new UnmanagedDataEvent
//      {
//         size = (uint)_dataBytes.Length,
//         type = type,
//         bytes = _handle.AddrOfPinnedObject()
//      };
//   }

//   public void Dispose()
//   {
//      if (_handle.IsAllocated)
//         _handle.Free();
//   }
//}

public static class EventFactory
{
   public static Event CreateNoteOnEvent(int pitch, float velocity, int channel = 0, int busIndex = 0, int sampleOffset = 0, int noteId = 0)
   {
      return new Event
      {
         busIndex = busIndex,
         sampleOffset = sampleOffset,
         type = 0, // kNoteOnEvent
         noteOn = new NoteOnEvent
         {
            pitch = (short)pitch,
            velocity = velocity,
            channel = (short)channel,
            noteId = noteId
         }
      };
   }

   public static Event CreateNoteOffEvent(int pitch, float velocity = 0f, int channel = 0, int busIndex = 0, int sampleOffset = 0, int noteId = 0)
   {
      return new Event
      {
         busIndex = busIndex,
         sampleOffset = sampleOffset,
         type = 1, // kNoteOffEvent
         noteOff = new NoteOffEvent
         {
            pitch = (short)pitch,
            velocity = velocity,
            channel = (short)channel,
            noteId = noteId
         }
      };
   }

   //   public static Event CreateDataEvent(DataEvent data, int busIndex = 0, int sampleOffset = 0)
   //   {
   //      return new Event
   //      {
   //         type = 2, // kDataEvent
   //         busIndex = busIndex,
   //         sampleOffset = sampleOffset,
   //         data = data.unmanagedDataEvent
   //      };
   //   }

}
