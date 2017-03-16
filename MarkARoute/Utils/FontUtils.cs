using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Utils
{
    public class FontUtils
    {
        // List of fonts that we want to load for the mod
        public static readonly Dictionary<string,string> m_desiredFonts = new Dictionary<string, string> { { "Highway Gothic", "Fonts/highwaygoth" }, { "Electronic Highway Sign", "Fonts/ehsmb" }, { "Transport","Fonts/transportmedium" } };

        public static Dictionary<string, Font> m_fontStore = new Dictionary<string, Font>();

        public static bool AddFonts()
        {
            foreach ( KeyValuePair<string,string> pair in m_desiredFonts)
            {
                AssetBundle bundle = FileUtils.GetAssetBundle(pair.Value);

                if (bundle == null)
                {
                    return false;
                }

                Font[] font = bundle.LoadAllAssets<Font>();

                LoggerUtils.Log("Font is loaded?" + (font.Length > 0));
                if (font != null)
                {
                    m_fontStore[pair.Key] = font[0];
                }
                bundle.Unload(false);

                if( SystemInfo.operatingSystem.Contains("Mac") || SystemInfo.operatingSystem.Contains("OS X")){
                    m_fontStore[pair.Key] = DistrictManager.instance.m_properties.m_areaNameFont.baseFont;
                }
            }
            
            return true;
        }
    }
}
