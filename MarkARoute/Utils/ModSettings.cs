using ColossalFramework.HTTP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MarkARoute.Utils
{
    class ModSettings
    {
        private static readonly string FILE_NAME = "MarkARouteSettings.json";
        private static readonly string ELEMENT_NAME = "MarkARouteVmsItems";

        public Hashtable settings = new Hashtable { { "loadMotorwaySigns", true } };

        private static ModSettings instance = null;

        public static ModSettings Instance()
        {
            if (instance == null)
            {
                instance = new ModSettings();
            }

            return instance;
        }

        /// <summary>
        /// Load all options from the disk.
        /// </summary>
        public static void LoadSettings()
        {
            if (File.Exists(FILE_NAME))
            {
                StreamReader reader = new StreamReader(FILE_NAME);
                Hashtable loadedSettings = JSON.JsonDecode(reader.ReadToEnd()) as Hashtable;
                reader.Close();
                Instance().settings = loadedSettings ?? new Hashtable { { "loadMotorwaySigns", true } };

            }
            else
            {
                Instance().settings = new Hashtable { { "loadMotorwaySigns", true } };
                LoggerUtils.LogWarning("Could not load the settings file!");
            }
        }

        /// <summary>
        /// Save all options from the disk.
        /// </summary>
        public static void SaveSettings()
        {
            StreamWriter writer = new StreamWriter(FILE_NAME);
            writer.WriteLine(JSON.JsonEncode(Instance().settings));
            writer.Close();

            LoggerUtils.Log("Saved setting file.");

        }
    }
}
