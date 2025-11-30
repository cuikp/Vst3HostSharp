using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.IO;

namespace HostVstApp.ValueConverters;


public class FileNameConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      return Path.GetFileName(value?.ToString()!);
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) { throw new NotImplementedException(); }
}


