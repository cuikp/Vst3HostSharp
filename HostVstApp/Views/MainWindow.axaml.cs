using Avalonia.Controls;
using HostVstApp.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using Vst3HostSharp;

namespace HostVstApp.Views;

public partial class MainWindow : Window
{
   public MainWindow()
   {
      InitializeComponent();

      Closing += MainWindow_Closing;

        Loaded += MainWindow_Loaded;
      
   }

    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
      DataContext = (MainViewModel)DataContext!;
   }

   private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
   {
      
   }

   public void SetDpi()
   {
      var screens = Screens.ScreenFromVisual(this);
      double dpiScalingFactor = screens?.Scaling ?? 1.0;

      if (this.DataContext != null)
      {
         ((MainViewModel)DataContext).dpiScalingFactor = dpiScalingFactor;
      }

   }
}
