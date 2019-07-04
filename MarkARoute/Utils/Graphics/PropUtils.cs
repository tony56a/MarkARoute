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
            if( !m_signPropDict.ContainsKey(signPropType))
            {
                // This should be for "none" cases
                return null;
            }
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

        public static bool GetNearLines(ushort segmentId, float maxDistance,float searchSize, ref HashSet<string> linesFound)
        {
            NetManager nm = NetManager.instance;
            Vector3 pos = nm.m_segments.m_buffer[segmentId].m_middlePosition;
            float extendedMaxDistance = maxDistance * 1.3f;
            int num = Mathf.Max((int)((pos.x - extendedMaxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((pos.z - extendedMaxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((pos.x + extendedMaxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((pos.z + extendedMaxDistance) / 64f + 135f), 269);
            bool noneFound = true;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = nm.m_segmentGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        NetSegment segment = nm.m_segments.m_buffer[num6];
                        float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[(int)num6].m_position);
                        if (num8 < maxDistance * maxDistance && (linesFound.Count < searchSize))
                        {
                            linesFound.Add(NetManager.instance.GetSegmentName(num6));
                        }

                            num6 = nm.m_nodes.m_buffer[num6].m_nextGridNode;
                        if (++num7 >= 32768)
                        {
                            LoggerUtils.Log("Out Of Bounds for list search");
                            break;
                        }
                    }
                }
            }
            return noneFound;
        }

    }
}
