using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkARoute.Utils
{
    class FileUtils
    {
        private static readonly string MODNAME = "MarkARoute";
        private const ulong m_workshopId = 558960454ul;
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
    }
}
