using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WaveCompagnonPlayer.utils
{
    public static class SoundInfo
    {
        [DllImport("winmm.dll")]
        private static extern uint mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);

        public static int GetSoundLength(string fileName)
        {
            StringBuilder lengthBuf = new StringBuilder(32);

            var rInt = mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
            Console.WriteLine(rInt);
            rInt = mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
            Console.WriteLine(rInt);
            rInt = mciSendString("close wave", null, 0, IntPtr.Zero);
            Console.WriteLine(rInt);
            int length = 0;
            int.TryParse(lengthBuf.ToString(), out length);

            return length;
        }


    }
}
