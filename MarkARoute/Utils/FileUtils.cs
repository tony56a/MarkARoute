using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Utils
{
    class FileUtils
    {

        private static readonly string MODNAME = "MarkARoute";
        private const ulong m_workshopId = 649674529ul;
        private static string m_savedModPath = null;

        public static string GetModPath()
        {
            if (m_savedModPath == null)
            {
                PluginManager pluginManager = PluginManager.instance;

                foreach (PluginManager.PluginInfo pluginInfo in pluginManager.GetPluginsInfo())
                {
                    if (pluginInfo.name == MODNAME || pluginInfo.publishedFileID.AsUInt64 == m_workshopId)
                    {
                        m_savedModPath = pluginInfo.modPath;
                    }
                }
            }

            return m_savedModPath;
        }

        public static AssetBundle GetAssetBundle( string path)
        {
            try
            {
                string absUri = "file:///" + GetModPath().Replace("\\", "/") + "/" + path;
                WWW www = new WWW(absUri);
                AssetBundle bundle = www.assetBundle;

                LoggerUtils.Log(path + " bundle loading " + ((bundle == null) ? "failed " + www.error : "succeeded"));
                String[] allAssets = bundle.GetAllAssetNames();
                foreach (String asset in allAssets)
                {
                    LoggerUtils.Log("asset is: " +asset);
                }
                return bundle;
            }
            catch (Exception e)
            {
                Debug.Log("Exception trying to load bundle file!" + e.ToString());
                return null;
            }
        }
    }
}
