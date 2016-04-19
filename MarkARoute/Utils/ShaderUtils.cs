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
        // Example Shaders from Unity :http://docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html,
        // http://wiki.unity3d.com/index.php?title=3DText

        public static readonly string textShaderText =
            "Shader \"GUI/3D Text Shader - Cull Back\" {" +
            "     Properties {" +
            "         _MainTex(\"Font Texture\", 2D) = \"white\" { }" +
            "         _Color(\"Text Color\", Color) = (1,1,1,1)" +
            "}" +
            " SubShader {" +
            "     Tags { \"Queue\" = \"Transparent\" \"IgnoreProjector\" = \"True\" \"RenderType\" = \"Transparent\" }" +
            "     Lighting Off Cull Back ZWrite Off Fog { Mode Off }" +
            "     Blend SrcAlpha OneMinusSrcAlpha" +
            "     Pass {" +
            "         Color[_Color]" +
            "         SetTexture[_MainTex] {" +
            "             combine primary, texture *primary" +
            "        }" +
            "     }" +
            " }" +
            "}";

        public static Shader textShader = new Material(textShaderText).shader;

        public static Dictionary<string, Shader> m_shaderStore = new Dictionary<string, Shader>();

        public static bool AddShader(string fullPath, string shaderName)
        {
            string modPath = FileUtils.GetModPath();
            //string fullPath = modPath + "/" + texturePath;

            if (!File.Exists(fullPath))
            {
                return false;
            }

            string shaderText = File.ReadAllText(fullPath);
            Material material = new Material(shaderText);
            m_shaderStore[shaderName] = material.shader;
            return true;
        }
    }
}
