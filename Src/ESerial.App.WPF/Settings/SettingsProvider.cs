using ESerial.SerialLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace ESerial.App.WPF.Settings
{
    internal class SettingsProvider : ISettingsProvider
    {
        private readonly string SETTINGS_DIRNAME;
        private readonly string SETTINGS_FILENAME;

        private Dictionary<string, string> SETTINGS;

        public SettingsProvider(string settingsFilename = "Settings.ini")
        {
            SETTINGS_FILENAME = settingsFilename;
            SETTINGS_DIRNAME = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}ESerial";

            if (!Directory.Exists(SETTINGS_DIRNAME))
            {
                Directory.CreateDirectory(SETTINGS_DIRNAME);
            }

            SETTINGS = new Dictionary<string, string>();

            ReadSettingsFile();
        }

        public void SetString(string key, string value)
        {
            SETTINGS[key] = value;
        }

        public string GetString(string key)
        {
            if (SETTINGS.ContainsKey(key))
            {
                return SETTINGS[key];
            }
            else
            {
                return "";
            }
        }

        public void SaveSettings()
        {
            try
            {
                using (StreamWriter file = new StreamWriter(Path.Combine(SETTINGS_DIRNAME, SETTINGS_FILENAME)))
                {
                    foreach (KeyValuePair<string, string> setting in SETTINGS)
                    {
                        file.WriteLine($"{setting.Key} = {setting.Value}");
                    }
                }
            }
            catch (Exception) { }
        }

        private void ReadSettingsFile()
        {
            if (File.Exists(Path.Combine(SETTINGS_DIRNAME, SETTINGS_FILENAME)))
            {
                try
                {
                    using (StreamReader file = new StreamReader(Path.Combine(SETTINGS_DIRNAME, SETTINGS_FILENAME)))
                    {
                        string? line;
                        while ((line = file.ReadLine()) != null)
                        {
                            line = line.Trim();

                            string[] parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                SETTINGS[parts[0].Trim()] = parts[1].Trim();
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
