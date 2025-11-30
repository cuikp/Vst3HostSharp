using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostVstApp;


public class RendererContext : NativeControlHost
{
   public RendererContext()
   {
   }

   public IntPtr Handle { get; private set; }

   protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
   {
      var handle = base.CreateNativeControlCore(parent);
      Handle = handle.Handle;
      return handle;
   }

   private void InitializeComponent()
   {
      AvaloniaXamlLoader.Load(this);
   }
}
