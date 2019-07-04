using ColossalFramework.HTTP;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Utils
{
    [Serializable]
    class ModSettings
    {
        [NonSerialized]
        private static ModSettings instance = null;
        [NonSerialized]
        private static readonly string FILE_NAME = "MarkARouteSetting.json";

        #region settings
        // Some nonsense with Unity built-in types pervent serialization, use floats instead
        public float btnPositionX = 180f;
        public float btnPositionY = 60f;
        public float btnPositionZ = 0f;

        #endregion

        #region settingSetters
        public void setBtnPosition(Vector3 newPos)
        {
            this.btnPositionX = newPos.x;
            this.btnPositionY = newPos.y;
        }
        #endregion

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
            ModSettings newInstance = new ModSettings();
            if (File.Exists(FILE_NAME))
            {
                StreamReader reader = new StreamReader(FILE_NAME);
                newInstance = JsonMapper.ToObject<ModSettings>(reader.ReadToEnd());
                newInstance = newInstance ?? new ModSettings();
                reader.Close();
            }
            else
            {
                LoggerUtils.LogWarning("Could not load the settings file!");
            }

            instance = newInstance;

        }

        /// <summary>
        /// Save all options from the disk.
        /// </summary>
        public static void SaveSettings()
        {
            StreamWriter writer = new StreamWriter(FILE_NAME);
            writer.WriteLine(JsonMapper.ToJson(instance));
            writer.Close();

            LoggerUtils.Log("Saved setting file.");

        }
    }

}
