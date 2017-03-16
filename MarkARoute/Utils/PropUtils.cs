using MarkARoute.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarkARoute.Utils
{
    class PropUtils : Object
    {
        public static Dictionary<string, PropInfo> m_signPropDict;

        public static bool LoadPropMeshes()
        {
            m_signPropDict = new Dictionary<string, PropInfo>();
            List<string> meshKeys = new List<string>(SignPropConfig.signPropInfoDict.Keys);
            List<PrefabInfo> m_allPropInfos = Resources.FindObjectsOfTypeAll<PrefabInfo>().Where(prefabInfo =>
                                                        prefabInfo.GetType().Equals(typeof(PropInfo))).ToList();

            // Bit of a placeholder hack, since we don't support multiple type of VMS models as of yet
            meshKeys.Add("electronic_sign_gantry");

            for (int i = 0; i < meshKeys.Count; ++i)
            {
                foreach (PrefabInfo prefab in m_allPropInfos)
                {
                    if (prefab.name.ToLower().Contains(meshKeys[i]))
                    {
                        m_signPropDict[meshKeys[i]] = prefab as PropInfo;
                    }
                }
            }

            return meshKeys.Count == 0;
        }
        
        public static List<NetLaneProps.Prop> findHighwaySignProp()
        {
            List<NetLaneProps.Prop> retVal = new List<NetLaneProps.Prop>();
            NetCollection[] propCollections = FindObjectsOfType<NetCollection>();
            foreach (NetCollection collection in propCollections)
            {
                foreach (NetInfo prefab in collection.m_prefabs.Where(prefab => prefab.m_lanes != null))
                {
                    foreach (NetInfo.Lane lane in prefab.m_lanes.Where(lane => lane.m_laneProps != null))
                    {
                        foreach (NetLaneProps.Prop prop in lane.m_laneProps.m_props.Where(prop => prop != null))
                        {
                            if (prop.m_prop != null && prop.m_prop.name != null && prop.m_prop.name.ToLower().Contains("motorway overroad signs"))
                            {
                                retVal.Add(prop);
                            }
                        }
                    }
                }
            }
            return retVal;
        }

        public static Material ReplaceTexture(string signPropType, List<string>textureOverrides )
        {

            Material material = m_signPropDict[signPropType].m_material;

            Texture2D texture = material.mainTexture as Texture2D;
            material = Instantiate(m_signPropDict[signPropType].m_material) as Material;
            Texture2D texCopy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            texCopy.SetPixels(texture.GetPixels());

            TextureReplaceConfig.TextureSignPropInfo signPropTextureInfo = TextureReplaceConfig.texturePropInfoDict[signPropType];
            for (int i = 0; i < signPropTextureInfo.numTextures; i++)
            {
                string textureString = textureOverrides[i];
                if (textureString != null && textureString != RouteManager.NONE && SpriteUtils.mTextureStore[signPropType].mTextureRefs[(i + 1).ToString()].ContainsKey(textureString))
                {
                    Texture2D otherTexture = SpriteUtils.mTextureStore[signPropType].mTextureRefs[(i + 1).ToString()][textureString];
                    for (int j = 0; j < otherTexture.width; j++)
                    {
                        for (int k = 0; k < otherTexture.height; k++)
                        {
                            texCopy.SetPixel((int)signPropTextureInfo.drawAreas[i].x + j, (int)signPropTextureInfo.drawAreas[i].yMax - k, otherTexture.GetPixel(j, k));
                        }
                    }
                }
            }
            texCopy.Apply();
            material.mainTexture = texCopy;
            return material;
        }
    }
}
