using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BadgerPluginExtender.dto
{
    public class PluginInfo
    {

        public string Name { get; private set; }

        public Version Version { get; private set; }

        public PluginInfo(string pluginName, Version pluginVersion)
        {
            Name = pluginName;

            Version = pluginVersion;
        }

    }
}
