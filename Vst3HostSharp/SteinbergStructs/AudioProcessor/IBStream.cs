using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ParamID = System.UInt32;
using ParamValue = double;
using static Vst3HostSharp.HelperMethods;
using System.Reflection.Metadata;

namespace Vst3HostSharp;


//public class IBStream
//{
//	public int Read(IntPtr buffer, int numBytes, int32* numBytesRead = nullptr)
//	{
//		byte[] readBytes = 

//		return 0;
//	}

//	public int Write(IntPtr buffer, int numBytes, int32* numBytesWritten = nullptr)
//	{

//      return 0;
//   }

//	public int Seek(Int64 pos, int mode, int64* result = nullptr)
//	{

//      return 0;
//   }

//	public int Tell(int64* pos)
//	{

//		return 0;
//	}
//}

