using ESerial.SerialLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESerial.App.WPF.Settings
{
    internal class SettingsProvider : ISettingsProvider
    {
        private readonly string SETTINGS_DIRNAME;
        private const string SETTINGS_FILENAME = "Settings.ini";

        public SettingsProvider()
        {
            SETTINGS_DIRNAME = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}ESerial";

            if (!Directory.Exists(SETTINGS_DIRNAME))
            {
                Directory.CreateDirectory(SETTINGS_DIRNAME);
            }
        }
    }
}
