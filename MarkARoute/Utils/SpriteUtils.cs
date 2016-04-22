using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Utils
{
    public class SpriteUtils
    {
        public static Dictionary<string, Material> m_textureStore = new Dictionary<string, Material>();
        public static List<Material> m_screenTextureStore = new List<Material>();
        /// <summary>
        /// Load an image file as a material for use when rendering route markers
        /// </summary>
        /// <param name="texturePath"> The path of the texture</param>
        /// <param name="textureName"> The name of the texture</param>
        /// <returns></returns>
        public static bool AddTexture(string fullPath, string textureName, bool isScreenTexture=false)
        {
            Shader shader = Shader.Find("UI/Default UI Shader");
            string modPath = FileUtils.GetModPath();
            //string fullPath = modPath + "/" + texturePath;

            if (!shader || !File.Exists(fullPath))
            {
                return false;
            }

            Texture2D texture = new Texture2D(2, 2);
            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            byte[] imageData = new byte[fileStream.Length];

            fileStream.Read(imageData, 0, (int)fileStream.Length);
            texture.LoadImage(imageData);
            FixTransparency(texture);

            Material material = new Material(shader);
            material.mainTexture = texture;
            if( !isScreenTexture)
            {
                m_textureStore[textureName] = material;
            }
            else
            {
                m_screenTextureStore.Add(material);
            }
            return true;
        }

        //Borrowed from Road Namer, which borrowed from Traffic++, which was copied from below
        //=========================================================================
        // Methods created by petrucio -> http://answers.unity3d.com/questions/238922/png-transparency-has-white-borderhalo.html
        //
        // Copy the values of adjacent pixels to transparent pixels color info, to
        // remove the white border artifact when importing transparent .PNGs.
        public static void FixTransparency(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            int w = texture.width;
            int h = texture.height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    Color32 pixel = pixels[idx];
                    if (pixel.a == 0)
                    {
                        bool done = false;
                        if (!done && x > 0) done = TryAdjacent(ref pixel, pixels[idx - 1]);        // Left   pixel
                        if (!done && x < w - 1) done = TryAdjacent(ref pixel, pixels[idx + 1]);        // Right  pixel
                        if (!done && y > 0) done = TryAdjacent(ref pixel, pixels[idx - w]);        // Top    pixel
                        if (!done && y < h - 1) done = TryAdjacent(ref pixel, pixels[idx + w]);        // Bottom pixel
                        pixels[idx] = pixel;
                    }
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
        }

        private static bool TryAdjacent(ref Color32 pixel, Color32 adjacent)
        {
            if (adjacent.a == 0) return false;

            pixel.r = adjacent.r;
            pixel.g = adjacent.g;
            pixel.b = adjacent.b;
            return true;
        }
        //=========================================================================
    }
}
