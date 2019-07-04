
using ColossalFramework.UI;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MarkARoute.Utils
{
    public static class StringUtils
    {
        public static Color ExtractColourFromTags(string text)
        {
            Color defaultColor = Color.white;
            Regex colourExtraction = new Regex("(?:<color)(#[0-9a-fA-F]+?)(>.*)");
            string extractedTag = colourExtraction.Replace(text, "$1");

            if (extractedTag != null && extractedTag != text && extractedTag != "")
            {
                defaultColor = UIMarkupStyle.ParseColor(extractedTag, defaultColor);
            }

            return defaultColor;
        }

        public static string RemoveTags(string text)
        {
            Regex tagRemover = new Regex("(<\\/?color.*?>)");

            return tagRemover.Replace(text, "");
        }
    }
}
