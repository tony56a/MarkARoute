using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Utils
{
    class ShaderUtils
    {

        public static Dictionary<string, Shader> m_shaderStore = new Dictionary<string, Shader>();

        public static bool AddShader(string fullPath, string shaderName)
        {
            AssetBundle bundle = FileUtils.GetAssetBundle(fullPath);

            if (bundle == null)
            {
                return false;
            }

            Shader shader = bundle.LoadAsset(shaderName + ".shader") as Shader;

            LoggerUtils.Log("Shader is loaded?" + (shader != null));
            if( shader != null)
            {
                m_shaderStore[shaderName] = shader;
            }
            bundle.Unload(false);
            return true;
        }
    }
}
