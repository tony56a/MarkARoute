using MarkARoute.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MarkARoute.Utils
{
    public class TextureRefs
    {
        public Dictionary<string, Dictionary<string, Texture2D>> mTextureRefs = new Dictionary<string, Dictionary<string, Texture2D>> {
        { "1", new Dictionary<string, Texture2D>() },
        { "2", new Dictionary<string, Texture2D>() },
        { "3", new Dictionary<string, Texture2D>() },
        { "4", new Dictionary<string, Texture2D>() } };

    }

    public class SpriteUtils
    {
        private static Regex textureFileRegex = new Regex("(\\d+)_(.*)");

        public static Dictionary<string, Material> mSpriteStore = new Dictionary<string, Material>();
        public static Dictionary<string, TextureRefs> mTextureStore = new Dictionary<string, TextureRefs>();

        /// <summary>
        /// Load an image file as a material for use when rendering route markers
        /// </summary>
        /// <param name="texturePath"> The path of the texture</param>
        /// <param name="textureName"> The name of the texture</param>
        /// <returns></returns>
        public static bool AddSprite(string fullPath, string textureName)
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
            mSpriteStore[textureName] = material;
            return true;
        }

        public static bool AddTexture(string fullPath, string propName, string textureName)
        {
            if (!File.Exists(fullPath))
            {
                return false;
            }
            Texture2D texture = new Texture2D(2, 2);
            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            byte[] imageData = new byte[fileStream.Length];

            fileStream.Read(imageData, 0, (int)fileStream.Length);
            texture.LoadImage(imageData);
            FixTransparency(texture);
            texture = FlipTexture(texture);

            string drawArea;
            string drawTexture;

            Match match = textureFileRegex.Match(textureName);
            if( match.Success)
            {
                drawArea = match.Groups[1].Value;
                drawTexture = match.Groups[2].Value;
                if (!mTextureStore.ContainsKey(propName)){
                    mTextureStore[propName] = new TextureRefs();
                }
                TextureRefs refs = mTextureStore[propName];
                if (!refs.mTextureRefs.ContainsKey(drawArea))
                {
                    refs.mTextureRefs[drawArea] = new Dictionary<string, Texture2D>();
                }
                refs.mTextureRefs[drawArea][drawTexture] = texture;

            }
            
            return true;
        }


        // Texture rotation methods taken from here: http://stackoverflow.com/questions/35950660/unity-180-rotation-for-a-texture2d-or-maybe-flip-both
        public static Texture2D FlipTexture( Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            System.Array.Reverse(pixels, 0, pixels.Length);
            texture.SetPixels32(pixels);

            Texture2D flipped = new Texture2D(texture.width, texture.height);
            int xN = texture.width;
            int yN = texture.height;
            
            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    flipped.SetPixel(xN - i - 1, j, texture.GetPixel(i, j));
                }
            }

            flipped.Apply();
            return flipped;            
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
