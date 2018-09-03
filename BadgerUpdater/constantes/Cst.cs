using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BadgerUpdater.constantes
{
    public static class Cst
    {

        public const string RunNameTpl = "UpdateRun-{0}";


        public static readonly string[] FilesIgnored =
{
            "Resources/AryxDevLibrary.dll",
            "Resources/BadgerCommonLibrary.dll",
            "Resources/BadgerUpdater.exe",
            "Resources/Interop.IWshRuntimeLibrary.dll",
            "Resources/Ionic.Zip.Reduced.dll",
            "Resources/updateBadger.cmd",
            "Resources/logUpd.log",

        };
    }
}
