using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils;
using BadgerCommonLibrary.constants;

namespace WaveCompagnonPlayer.dto
{
    public class AppArgsDto
    {
        public EnumWaveCompModeTraitement WaveCompModeTraitements { get; set; }
        public EnumSonWindows SoundToPlay { get; set; }
        public string SoundDevice { get; set; }
        public int SoundVolume { get; set; }

        public string GetDeviceTpl { get; set; }
        public bool IsDebugMode { get; internal set; }
    }
}
