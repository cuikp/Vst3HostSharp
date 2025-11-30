using System.Text;

namespace Vst3HostSharp;

internal class HelperMethods
{
   internal static unsafe string StringFromByte(byte* bb, int len)
    {
        if (bb == null)
            throw new ArgumentNullException("The input byte pointer is null.");

        unsafe
        {
            byte* stringPtr = bb;

            int stringLength = len;
            for (int l = 0; l < len; l++)
                if (stringPtr[l] == 0) 
                    { stringLength = l; break; }
            try { return Encoding.UTF8.GetString(stringPtr, stringLength); }
            catch (DecoderFallbackException ex) { throw new ArgumentNullException("Possible invalid UTF8 string.:" + ex.Message); }
        }
    }

   internal static unsafe string StringFromChar(char* charArray, int length)
   {
      return new string(charArray, 0, length).TrimEnd('\0');
   }

   //internal static string GetStringFromPointer(IntPtr pointer)
   //{
   //   string resultString = Marshal.PtrToStringUTF8(pointer)!;
   //   string[] results = resultString.Split(new char[] { ';' });
   //   NativeMethods.FreeString(pointer);

   //   return string.Join("\n", results);

   //}
 
}

