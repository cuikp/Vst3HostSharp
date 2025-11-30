using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HostVstApp.ViewModels;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vst3HostSharp;


namespace HostVstApp.Views;

public partial class MainView : UserControl
{
   internal double dpiScalingFactor = 1;

   MainViewModel myMVM = null!;

   public MainView()
   {
      InitializeComponent();

      this.Loaded += MainView_Loaded;
      this.Unloaded += MainView_Unloaded;

   }

   private void MainView_Unloaded(object? sender, RoutedEventArgs e)
   {

      myMVM.MainSoundSource.StopPlay();
      myMVM.MainSoundSource.CloseSoundSource();

      myMVM.ScannedVsts.Clear();

      List<Vst3Plugin> toDisposePlugins = [.. myMVM.MainSoundSource.ActiveVsts.Where(svst => svst != null)];

      for (int vst3no = myMVM.MainSoundSource.ActiveVsts.Count - 1; vst3no >= 0; vst3no--)
      {
         Vst3Plugin vstplugin = myMVM.MainSoundSource.ActiveVsts.ElementAt(vst3no);
        
         if (myMVM.openWindows.FirstOrDefault(ow => ow.Tag == vstplugin) is Window vstwin)
         {
            myMVM.openWindows.Remove(vstwin);
            vstwin.Tag = "ForceClose";
            vstwin.Close();
         }

         myMVM.MainSoundSource.ActiveVsts.Remove<Vst3Plugin>(vstplugin);
         myMVM.ActiveVstList.Remove(vstplugin);

         vstplugin.Dispose();

      }

   }

   private async void MainView_Loaded(object? sender, RoutedEventArgs e)
   {
      myMVM = (MainViewModel)DataContext!;

      try { myMVM.MainSoundSource = new SoundSource(testFile); }
      catch (Exception ex) { myMVM.OutputToFile("___Error making sound source: \n" + ex.Message); }

      SoundPlayDP.DataContext = myMVM.MainSoundSource;


      Vst3HostSharp.Logging.SetOutputLogPath(myMVM.GetOutputDir);  //enable logging from vst3HostSharp

      if (Design.IsDesignMode) return;

      if (Settings.Default.VstScanFolder == "")
      {
         bool selected = await SelectVstFolder();
         if (!selected)
            Environment.Exit(0);

         myMVM.UpdateVst3ScanFolder();

      }

      Vst3sLV.IsEnabled = false;
      PlayTestFileBut.IsEnabled = false;

      myMVM.TitleText = "Scanning folder for vst3s . . .";
      await Task.Run(() =>
      {
         myMVM.ScanVsts();
      });
      Vst3sLV.IsEnabled = true;
      PlayTestFileBut.IsEnabled = true;

      myMVM.TitleText = "HostVst3App";

   }

   private async Task<bool> SelectVstFolder()
   {
      var topLevel = TopLevel.GetTopLevel(this);
      FolderPickerOpenOptions fpoo = new()
      {
         Title = "Select vst3 folder",
         AllowMultiple = false
      };

      MainWindow? mwin = this.Parent as MainWindow;
      fpoo.SuggestedStartLocation = await mwin!.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads);
      IReadOnlyList<IStorageFolder>? folders = await topLevel!.StorageProvider.OpenFolderPickerAsync(fpoo);

      if (folders == null || folders.Count == 0)
         return false;
      else
      {
         string? f = folders[0].TryGetLocalPath();
         if (f != null)
         {
            Settings.Default.VstScanFolder = f;
            Settings.Default.Save();
            return true;
         }
      }

      myMVM.UpdateVst3ScanFolder();

      return true;
   }

   readonly string testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "bass solo_95bpm_8bars.mp3");

   private void AddVstButton_Click(object? sender, RoutedEventArgs e)
   {
      if (sender is not Button b || b.DataContext is not Vst3PluginInfo vstinfo) return;

      myMVM.OutputToFile($"\nWill try to create vst3plugin: {vstinfo.Vst3FilePath}");

      Vst3Plugin vst3Plugin = new(vstinfo.InnerVst3FilePath) { PluginType = vstinfo.PluginType };

      vstinfo.myVst3Plugin = vst3Plugin;

      myMVM.OutputToFile($"Loaded: {Path.GetFileName(vst3Plugin.VstFilePath)}");

      vst3Plugin.InitializePlugin(256, 44100, IntPtr.Zero);  

      myMVM.VstInfoText = vst3Plugin.PluginLoadResults;

      myMVM.MainSoundSource.ActiveVsts.Add(vst3Plugin);
      myMVM.ActiveVstList.Add(vst3Plugin);

      if (OperatingSystem.IsWindows())
         myMVM.UpdateErrorDisplay();

   }

   private void ShowVstEditorBut_Click(object? sender, RoutedEventArgs e)
   {
      if (sender is not Button b || b.DataContext is not Vst3Plugin vstplugin) return;

      if (vstplugin.IsEditorViewCreated)
         ((Window)vstplugin.PlugViewParent!).IsVisible = true;  //temporary   
      else
      {  //show and activate the window

         RendererContext rcont = new() { Width = 100, Height = 100, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
         Window vstWin = new()
         {
            Content = rcont,
            Tag = vstplugin,
            ShowInTaskbar = false,
            MinWidth = 100,
            MinHeight = 100,
            Title = "Avalonia window",
            CanResize = false
         };

         myMVM.openWindows.Add(vstWin);

         vstplugin.PlugViewParent = vstWin;

         vstWin.Closing += VstWin_Closing;

         vstWin.Show(); // Have to show first to get handle

         //bool SizeChangedAfterAttachment = false;

         int platformtype = OperatingSystem.IsWindows() ? 0 : 1;

         int getWidth = 100;
         int getHeight = 100;

         //SelectedVst3.myVst3Plugin.GetEditorSize(ref getWidth, ref getHeight);
         //Debug.WriteLine("GetwidthB4 = " + getWidth + " :: getheightB4" + getHeight);

         bool attached = vstplugin.ShowEditor(rcont.Handle, platformtype);

         if (attached)
         {
            Debug.WriteLine("App: Editor successfully shown...");

            vstplugin.GetEditorSize(ref getWidth, ref getHeight);

            double resizeFac = OperatingSystem.IsWindows() ? myMVM.dpiScalingFactor : 1;

            rcont.Width = getWidth / resizeFac;
            rcont.Height = getHeight / resizeFac;

            vstWin.Width = rcont.Width;
            vstWin.Height = rcont.Height;

            //SelectedVst3.PluginSize_Changed += SelectedVst3_PluginSize_Changed;

            if (OperatingSystem.IsWindows())
               myMVM.UpdateErrorDisplay();

            vstplugin.IsEditorViewCreated = true;

         }
         else
            vstWin.Close();
      }

   }

   private void VstWin_Closing(object? sender, WindowClosingEventArgs e)
   {
      if (sender is Window vstwin)
      {
         if (vstwin.Tag is string s && s == "ForceClose")
            return;
         else
         {
            vstwin.IsVisible = false;
            e.Cancel = true;
         }
      }
   }


   public async void BeginPlayBut_Click(object? sender, RoutedEventArgs e)
   {
      try
      {
         SoundPlayDP.DataContext = myMVM.MainSoundSource;
         switch (myMVM.MainSoundSource.playingBack)
         {
            case true:
               myMVM.MainSoundSource.StopPlay();
               PlayTestFileBut.Content = "Play test file";
               break;

            case false:
               myMVM.MainSoundSource.BeginPlay();
               PlayTestFileBut.Content = "Stop test file";
               break;
         }

      }
      catch (Exception ex) { await MessageBox.Show("Error: \n" + ex.Message); }


   }

   private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
   {
      if (myMVM == null) return;
      ListBox? lb = sender as ListBox;
      if (lb!.SelectedItem == null) return;
      myMVM.SelectedVst3 = (lb.SelectedItem as Vst3PluginInfo)!;

      StructsAndEnums.VstType thisType = myMVM.SelectedVst3.PluginType;

   }

   private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
   {
      if (sender is not TextBox thistb) return;
      thistb.SelectionStart = thistb.Text!.Length;
      thistb.SelectionEnd = thistb.SelectionStart;


   }

   private void DisposeVstButton_Click(object? sender, RoutedEventArgs e)
   {
      if (sender is Button b && b.DataContext is Vst3Plugin vstplugin)
      {
         if (myMVM.MainSoundSource.ActiveVsts.ToList().Contains(vstplugin))
         {
            myMVM.MainSoundSource.ActiveVsts.Remove<Vst3Plugin>(vstplugin);
            myMVM.ActiveVstList.Remove(vstplugin);
            vstplugin.Dispose();
            if (myMVM.openWindows.FirstOrDefault(ow => ow.Tag == vstplugin) is Window vstwin)
            {
               myMVM.openWindows.Remove(vstwin);
               vstwin.Tag = "ForceClose";
               vstwin.Close();
            }
            if (OperatingSystem.IsWindows())
               myMVM.UpdateErrorDisplay();

         }
      }

   }

   private async void SelectVst3FolderButton_Click(object? sender, RoutedEventArgs e)
   {
      bool selected = await SelectVstFolder();
      myMVM.UpdateVst3ScanFolder();


      if (selected)
      {
         myMVM.TitleText = "Scanning folder for vst3s . . .";
         Vst3sLV.IsEnabled = false;
         await Task.Run(() =>
         {
            myMVM.ScanVsts();
         });
         myMVM.TitleText = "HostVst3App";
         Vst3sLV.IsEnabled = true;
      }

   }

   private void ActiveVstsLB_SelectionChanged(object? sender, SelectionChangedEventArgs e)
   {
      myMVM.VstParamText = "";

      if (sender is ListBox lbox && lbox.SelectedItem is Vst3Plugin vstplugin)
      {

         int parameterCount = vstplugin.GetParameterCount();
         string dispText = $"Parameter count: {parameterCount}\n";

         for (int i = 0; i < parameterCount; i++)
         {
            dispText += "\n";
            ManagedParameterInfo mpinfo = vstplugin.GetParameterInfo(i);
            dispText += (i + $" Id={mpinfo.Id}  {mpinfo.Title} :: Default= {mpinfo.DefaultNormalizedValue} :: {mpinfo.Flags}");
         }

         myMVM.VstParamText = dispText;
      }

   }
}
