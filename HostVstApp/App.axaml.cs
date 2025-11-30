using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HostVstApp.ViewModels;
using HostVstApp.Views;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace HostVstApp;

public partial class App : Application
{
   public override void Initialize()
   {
      AvaloniaXamlLoader.Load(this);

   }

   public override void OnFrameworkInitializationCompleted()
   {

      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
         desktop.MainWindow = new MainWindow
         {
            DataContext = new MainViewModel()
         };

         ((MainWindow)desktop.MainWindow).SetDpi();
      }
     
            
      //for 3rd party dlls/dylibs
      AddVariablePaths();

      base.OnFrameworkInitializationCompleted();

      
   }

   internal static void AddVariablePaths()
   {
      string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
      string libsPath = Path.Combine(currentDirectory, "libs");
      //string windowsPathSoundlibs = Path.Combine(currentDirectory, "libs", "Windows");
      string windowsPathSoundlibs = Path.Combine(currentDirectory, "libs", "Win");
      string macPathSoundlibs = Path.Combine(currentDirectory, "libs", "MacOS");

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
         AddPathVariable("PATH", windowsPathSoundlibs);
         AddPathVariable("PATH", libsPath);
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
         AddPathVariable("DYLD_LIBRARY_PATH", macPathSoundlibs);
         AddPathVariable("DYLD_LIBRARY_PATH", libsPath);
      }

   }

   private static void AddPathVariable(string variableName, string path)
   {
      var currentPath = Environment.GetEnvironmentVariable(variableName) ?? string.Empty;

      if (!currentPath.Split(Path.PathSeparator).Contains(path))
      {
         var newPath = $"{path}{Path.PathSeparator}{currentPath}";
         Environment.SetEnvironmentVariable(variableName, newPath);
      }
   
   }
}
