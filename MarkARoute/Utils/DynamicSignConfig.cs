using ColossalFramework.HTTP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MarkARoute.Utils
{
    class DynamicSignConfig
    {
        private static readonly string FILE_NAME = "MarkARouteVmsStrings.json";
        private static readonly string ELEMENT_NAME = "MarkARouteVmsItems";

        private static readonly List<string> fallbackMsgStrings = new List<string>{ "Eat your vegetables",
                                    "Don't cut back on roads,\nor you'll regret it",
                                    "Have you tried flurm?",
                                    "Through Traffic:\nUse inner lanes only",
                                    "Watch for sudden \nlane changes",
                                    "If exit ramp is backed up\nUse next exit"};

        public List<String> msgStrings;

        private static DynamicSignConfig instance = null;

        public static DynamicSignConfig Instance()
        {
            if (instance == null)
            {
                instance = new DynamicSignConfig();
            }

            return instance;
        }

        /// <summary>
        /// Load all options from the disk.
        /// </summary>
        public static void LoadVmsMsgList()
        {
            if (File.Exists(FILE_NAME))
            {
                StreamReader reader = new StreamReader(FILE_NAME);

                ArrayList vmsMsgStrings = JSON.JsonDecode(reader.ReadToEnd()) as ArrayList;
                
                reader.Close();

                if (vmsMsgStrings != null)
                {
                    Instance().msgStrings = vmsMsgStrings.Cast<string>().ToList();

                    LoggerUtils.Log("Loaded route VMS message file.");
                }
                else
                {
                    Instance().msgStrings = fallbackMsgStrings;
                    LoggerUtils.LogError("Created route VMS message list is invalid!");
                }
            }
            else
            {
                Instance().msgStrings = fallbackMsgStrings;
                LoggerUtils.LogWarning("Could not load the VMS message list file!");
            }
        }

        /// <summary>
        /// Save all options from the disk.
        /// </summary>
        public static void SaveVmsMsgList()
        {
            StreamWriter writer = new StreamWriter(FILE_NAME);
            ArrayList strs = new ArrayList(Instance().msgStrings);
            writer.WriteLine(JSON.JsonEncode(strs));
            writer.Close();

            LoggerUtils.Log("Saved route shield info file.");

        }
    }
}
