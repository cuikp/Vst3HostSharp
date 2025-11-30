using System.Runtime.InteropServices;

namespace Vst3HostSharp;

public class StructsAndEnums
{
   [StructLayout(LayoutKind.Sequential)]
   internal struct InitializedComponent
   {
      internal IntPtr compPtr;
      internal IntPtr edPtr;
      internal IntPtr plugViewPtr;
      internal IntPtr compHandlerPtr;
      internal IntPtr hostAppPtr;
      internal VstType vstType;
      internal int BusCountIn;
      internal int BusCountOut;
      internal int editorWidth;
      internal int editorHeight;
      [MarshalAs(UnmanagedType.I1)]
      internal bool HasEditor;
      [MarshalAs(UnmanagedType.I1)]
      internal bool HasPlugView;

   }


   public enum VstType
   {
      VstUnknown,
      VstEffect,
      VstInstrument,
      VstFailedLoad,
      VstCrashedLoad
   }

   [StructLayout(LayoutKind.Sequential)]
   internal struct ViewRect
   {
      internal int left;
      internal int top;
      internal int right;
      internal int bottom;
      internal readonly int getWidth() { return right - left; }
      internal readonly int getHeight() { return bottom - top; }
   }
}
