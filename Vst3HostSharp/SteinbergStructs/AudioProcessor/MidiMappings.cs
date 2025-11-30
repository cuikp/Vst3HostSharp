using System;
using System.Runtime.InteropServices;

namespace Vst3HostSharp;

public class MidiMappings
{
   public readonly struct MidiControllerInfo
   {
      public string ControllerName { get; }
      public double DefaultValue { get; }

      internal MidiControllerInfo(string name, double defaultValue)
      {
         ControllerName = name;
         DefaultValue = defaultValue;
      }
   }

   public static readonly MidiControllerInfo[] MidiControllers =
   [
       new("Bank Select MSB",       0.0),
       new("Modulation Wheel",      0.0),
       new("Breath Controller",     0.0),
       new("",                      0.0),
       new("Foot Controller",       0.0),
       new ("Portamento Time",      0.0),
       new("Data Entry MSB",        0.0),
       new( "Volume",               1.0),
       new( "Balance",              0.5),
       new( "",                     0.0),
       new("Pan",                   0.5),
       new("Expression",            1.0),
       new ("Effect 1 Depth",       0.0),
       new("Effect 2 Depth",        0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("General Purpose Controller 1", 0.0),
       new("General Purpose Controller 2", 0.0),
       new("General Purpose Controller 3", 0.0),
       new("General Purpose Controller 4", 0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("Bank Select LSB",       0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("Data Entry LSB",        0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("",                      0.0),
       new("Sustain Pedal (Damper)", 0.0),
       new("Portamento On/Off",     0.0),
       new("Sostenuto On/Off",      0.0),
       new("Soft Pedal On/Off",     0.0),
       new("Legato Footswitch",     0.0),
       new("Hold 2",                0.0),
       new("Sound Controller 1 (Variation)", 0.0),  // 70
      
       new("Sound Controller 2 (Filter Cutoff)", 0.0), // 71
       new("Sound Controller 3 (Release Time)", 0.0),  // 72
       new("Sound Controller 4 (Attack Time)", 0.0),   // 73
       new("Sound Controller 5 (Filter Resonance)", 1.0), //74
       new("Sound Controller 6 (Decay Time)", 0.5),       //75

       new("Sound Controller 7 (Vibrato Rate)", 0.0),
       new("Sound Controller 8 (Vibrato Depth)", 0.0),
       new("Sound Controller 9 (Vibrato Delay)", 0.0),
       new("Sound Controller 10",  0.0),
       new("General Purpose Controller 5", 0.0),
       new("General Purpose Controller 6", 0.0),
       new("General Purpose Controller 7", 0.0),
       new("General Purpose Controller 8", 0.0),
       new("Portamento Control",   0.0),
       new("",                     0.0),
       new("",                     0.0),
       new("",                     0.0),
       new("",                     0.0),
       new("",                     0.0),
       new("",                     0.0),
       new("Effect 1 Depth (Reverb)", 0.0),
       new("Effect 2 Depth (Tremolo)", 0.0),
       new("Effect 3 Depth (Chorus)", 0.0),
       new("Effect 4 Depth (Detune)", 0.0),
       new("Effect 5 Depth (Phaser)", 0.0),
       new("Data Increment",      0.0),
       new("Data Decrement",      0.0),
       new("NRPN LSB",            0.0),
       new("NRPN MSB",            0.0),
       new("RPN LSB",             0.0),
       new("RPN MSB",             0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("",                    0.0),
       new("All Sound Off",       0.0),
       new("Reset All Controllers", 0.0),
       new("Local Control On/Off", 0.0),
       new("All Notes Off",       0.0),
       new("Omni Mode Off",       0.0),
       new("Omni Mode On",        0.0),
       new("Mono Mode On",        0.0),
       new("Poly Mode On",        1.0),
       new("Channel Pressure (Aftertouch)", 0.0),  //128
       new("Pitch Bend",          0.5),            //129
       new("Program Change",      0.0),            //130
       new("Polyphonic Key Pressure", 0.0),        //131
       new("MIDI Time Code (Quarter Frame)", 0.0)  //132
   ];


  

}
