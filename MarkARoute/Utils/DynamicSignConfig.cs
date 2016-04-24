using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MarkARoute.Utils
{
    class DynamicSignConfig
    {
        private static readonly string FILE_NAME = "MarkARouteVmsStrings.xml";
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
                XmlSerializer serializer = new XmlSerializer(typeof(string[]));
                StreamReader reader = new StreamReader(FILE_NAME);

                string[] vmsMsgStrings = ((string[])serializer.Deserialize(reader));
                reader.Close();

                if (vmsMsgStrings != null)
                {
                    Instance().msgStrings = new List<string>(vmsMsgStrings);

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
            XmlSerializer serializer = new XmlSerializer(typeof(string[]));
            StreamWriter writer = new StreamWriter(FILE_NAME);

            serializer.Serialize(writer, instance.msgStrings);
            writer.Close();

            LoggerUtils.Log("Saved route shield info file.");

        }
    }
}
