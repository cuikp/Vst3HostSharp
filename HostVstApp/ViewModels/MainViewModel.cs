using Avalonia.Controls;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Vst3HostSharp;

namespace HostVstApp.ViewModels;

public class MainViewModel : ViewModelBase, INotifyPropertyChanged
{

   private const int FR_PRIVATE = 0x10;
   
   public new event PropertyChangedEventHandler? PropertyChanged;
   private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

   private string _InfoText = "-----";
   public string InfoText { get => _InfoText; set { _InfoText = value; } }

   private string _VstInfoText = "";
   public string VstInfoText
   {
      get => _VstInfoText; 
      set { if (_VstInfoText != value) { _VstInfoText = value; NotifyPropertyChanged(nameof(VstInfoText)); } }
   }

   private string _VstParamText = "";
   public string VstParamText
   {
      get => _VstParamText; 
      set { if (_VstParamText != value) { _VstParamText = value; NotifyPropertyChanged(nameof(VstParamText)); } }
   }

   private string _ErrorText = "";
   public string ErrorText
   {
      get { return _ErrorText; }
      set { if (_ErrorText != value) { _ErrorText = value; NotifyPropertyChanged(nameof(ErrorText)); } }
   }
      
   public void UpdateVst3ScanFolder() { NotifyPropertyChanged(nameof(Vst3ScanFolder)); }

   private string _TitleText = "HostVst3App";
   public string TitleText { get => _TitleText; set { _TitleText = value; NotifyPropertyChanged(nameof(TitleText)); } }

   public ObservableCollection<Vst3Plugin> ActiveVstList { get; set; } = [];

   private Vst3PluginInfo _SelectedVst3 = null!;
   public Vst3PluginInfo SelectedVst3
   {
      get => _SelectedVst3;
      set { _SelectedVst3 = value; }
   }

   public ObservableCollection<Vst3PluginInfo> ScannedVsts { get; set; } = [];
   public string Vst3ScanFolder => Settings.Default.VstScanFolder;

   public MainViewModel()
   {
      //Create new log files
      File.Create(OutputLogFile).Close();
      File.Create(OutputVst3LogFile).Close();

   }


   public SoundSource MainSoundSource { get; set; } = null!;

   public void ScanVsts()
   {
      ScannedVsts.Clear();
      GetVsts(Settings.Default.VstScanFolder);
      
      List<Vst3PluginInfo> toSort = [.. ScannedVsts];
      toSort.Sort((sv1, sv2) => sv1.PluginType.CompareTo(sv2.PluginType));
      ScannedVsts.Clear();
      ScannedVsts.AddRange(toSort);

   }

   private void GetVsts(string currDir)
   {
      try
      {
         if (OperatingSystem.IsWindows())
         {
            foreach (string f in Directory.GetFiles(currDir, "*.vst3"))
               ScannedVsts.Add(new Vst3PluginInfo(f));

            foreach (string d in Directory.GetDirectories(currDir))
               GetVsts(d);
            return;
         }

         if (OperatingSystem.IsMacOS())
         {
            if (currDir.EndsWith(".vst3"))
               try
               {
                  //LogOutput("Now scanning vst: " + currDir);
                  ScannedVsts.Add(new Vst3PluginInfo(currDir));
               }
               //catch (Exception ex) { /*OutputToFile($"Error creating vst3plugin info: {currDir}\n{ex.Message}");*/ }
               catch { }
            else
            {
               foreach (string d in Directory.GetDirectories(currDir))
                  GetVsts(d);
            }
            return;
         }
      }
      catch { }

   }

   public void ShowVstInfo()
   {

      int classCount = SelectedVst3.ClassCount;
      string dispText = "Class count: " + classCount.ToString() + "\n\n";

      if (classCount <= 0)
         dispText += "Error messages:\n";

      //Debug.WriteLine(SelectedVst3.PluginLoadResults);

      for (int i = 0; i < classCount; i++)
         dispText += string.Join("\n", SelectedVst3.GetInfoList()) + "\n\n";

      VstInfoText = dispText;

   }

   internal void OutputToFile(string outputLine)
   {
      File.AppendAllText(OutputLogFile, outputLine + "\n");
   }


   internal string GetOutputDir => outputDir;
   internal string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HostVstApp.Desktop");
   string OutputLogFile => Path.Combine(outputDir, "output.log");
   string OutputVst3LogFile => Path.Combine(outputDir, "outputVst3.log");

   internal void UpdateErrorDisplay()
   {
      try
      {
         string outputShowFile = OutputLogFile.Replace("output", "outputShow");
         File.Copy(OutputLogFile, outputShowFile, true);
         ErrorText = File.ReadAllText(outputShowFile, Encoding.UTF8);
      }
      catch { Debug.WriteLine("could not access output log"); }
   }


   internal double dpiScalingFactor = 1;

   internal List<Window> openWindows = [];


   private void SelectedVst3_PluginSize_Changed(Vst3Plugin sender, double newWidth, double newHeight)
   {
      Window pwin = (Window)sender.PlugViewParent!;

      Debug.WriteLine("Window changed invoked");

      double resizeFac = OperatingSystem.IsWindows() ? dpiScalingFactor : 1;  //temporary: must calculate dpix/dpiy
      pwin.Width = newWidth / resizeFac;
      pwin.Height = newHeight / resizeFac;

   }

   private string StateChunkString => Convert.ToBase64String(StateChunk);
   //private byte[] GetByteChunkFromString => Convert.FromBase64String(StateChunkString);
   private byte[] StateChunk = [];

   public void GetState()
   {
      StateChunk = SelectedVst3!.myVst3Plugin.GetState();
      //Debug.WriteLine("Received byte stream: " + StateChunkString);

   }

   public void SetState()
   {
      SelectedVst3!.myVst3Plugin.SetState(StateChunk);

   }


   internal void PlayNote(short note)
   {
      Event newevent = new()
      {
         type = (ushort)EventTypes.KNoteOnEvent,
         busIndex = 0,
         ppqPosition = 0,
         sampleOffset = 0,

         noteOn = new NoteOnEvent()
         {
            channel = 0,
            pitch = note,
            velocity = 1,
            noteId = -1
         }
      };

      SelectedVst3.myVst3Plugin.AudioProcessor.AddEvent(ref newevent);


   }

   internal void StopNote(short note)
   {
      Event newevent = new()
      {
         type = (ushort)EventTypes.KNoteOffEvent,
         busIndex = 0,
         ppqPosition = 0,
         sampleOffset = 0,

         noteOff = new NoteOffEvent()
         {
            channel = 0,
            pitch = note,
            velocity = 0,
            noteId = -1
         }
      };

      SelectedVst3.myVst3Plugin.AudioProcessor.AddEvent(ref newevent);


   }



}
