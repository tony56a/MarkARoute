using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace MarkARoute.Utils
{

    public class RouteShieldConfig
    {
        private static Dictionary<string, RouteShieldInfo> fallbackDict = new Dictionary<string, RouteShieldInfo>()
        {
            { "BC",new RouteShieldInfo(-0.25f, 0f, 0.5f, new Color(0,0,0.58137f), "BC") },
            { "ON",new RouteShieldInfo(-0.65f, 0f, 0.5f, Color.black, "ON") },
            { "I",new RouteShieldInfo(-0.25f, 0f, 0.6f, Color.white, "I") },
            { "US",new RouteShieldInfo(0f, 0f, 0.7f, Color.black, "US") },
            { "AUS",new RouteShieldInfo(-0.25f, 0f, 0.5f, Color.white, "AUS") },
            { "route",new RouteShieldInfo(0f, -0.5f, 0.3f, Color.white, "route") },
            { "DE",new RouteShieldInfo(0f, 0f, 0.5f, Color.white, "DE") },
            { "CN",new RouteShieldInfo(-0.2f, 0f, 0.6f, Color.white, "CN") },
            { "NL",new RouteShieldInfo(0f, 0f, 0.7f, Color.white, "NL") },
            { "TO",new RouteShieldInfo(0f, 0f, 0.5f, Color.black, "TO") },


        };

        public Dictionary<string, RouteShieldInfo> routeShieldDictionary;

        public RouteShieldInfo GetRouteShieldInfo(string key)
        {
            if (routeShieldDictionary.ContainsKey(key))
            {
                return routeShieldDictionary[key];
            }
            else
            {
                return null;
            }
        }

        private static RouteShieldConfig instance = null;
        public static RouteShieldConfig Instance()
        {
            if (instance == null)
            {
                instance = new RouteShieldConfig();
            }

            return instance;
        }

        public static void SetInstance(RouteShieldConfig manager)
        {
            if (manager != null)
            {
                instance = manager;
            }
            else
            {
                LoggerUtils.LogError("Tried to set RouteShieldInstance instance to a null variable!");
            }
        }

        /// <summary>
        /// Load all options from the disk.
        /// </summary>
        public static void LoadRouteShieldInfo()
        {
            if (File.Exists("RouteShieldOptions.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RouteShieldObject[]),new XmlRootAttribute() { ElementName = "RouteInfoItems" });
                StreamReader reader = new StreamReader("RouteShieldOptions.xml");
                Dictionary<string, RouteShieldInfo> routeShieldDict = ((RouteShieldObject[])serializer.Deserialize(reader)).ToDictionary(i => i.key, i => i.value);
                reader.Close();

                if (routeShieldDict != null)
                {
                    Instance().routeShieldDictionary = routeShieldDict;

                    LoggerUtils.Log("Loaded route shield info file.");
                }
                else
                {
                    Instance().routeShieldDictionary = fallbackDict;
                    LoggerUtils.LogError("Created route shield info is invalid!");
                }
            }
            else
            {
                Instance().routeShieldDictionary = fallbackDict;
                LoggerUtils.LogWarning("Could not load the route shield info file!");
            }
        }

        /// <summary>
        /// Save all options from the disk.
        /// </summary>
        public static void SaveRouteShieldInfo()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RouteShieldObject[]), new XmlRootAttribute() { ElementName = "RouteInfoItems" });
            StreamWriter writer = new StreamWriter("RouteShieldOptions.xml");

            serializer.Serialize(writer, instance.routeShieldDictionary.Select(kv => new RouteShieldObject() { key = kv.Key, value = kv.Value }).ToArray());
            writer.Close();

            LoggerUtils.Log("Saved route shield info file.");

        }
    }
}

[Serializable]
public class RouteShieldObject
{
    //TODO: replace this( and serialization entirely ) with JSON, since dictionaries are supported by default.
    [XmlElement("RouteShieldKey")]
    public string key;
    [XmlElement("RouteShieldInfo")]
    public RouteShieldInfo value;
}

[Serializable]
public class RouteShieldInfo
{
    [XmlElement("UpOffset",IsNullable = false)]
    public float upOffset = 0f;

    [XmlElement("LeftOffset",IsNullable = false)]
    public float leftOffset = 0f;

    [XmlElement("TextColor",IsNullable = false)]
    public Color textColor = Color.black;

    [XmlElement("TextScale",IsNullable = false)]
    public float textScale = 0.5f;

    [XmlElement("TextureName",IsNullable = false)]
    public String textureName = "";

    public RouteShieldInfo()
    {
        // Default Constructor without parameter
    }

    public RouteShieldInfo(string textureName)
    {
        this.textureName = textureName;
    }

    public RouteShieldInfo(float upOffset, float leftOffset, float textScale, Color textColor, string textureName)
    {
        this.upOffset = upOffset;
        this.leftOffset = leftOffset;
        this.textColor = textColor;
        this.textScale = textScale;
        this.textureName = textureName;
    }
}

