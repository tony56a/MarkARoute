using Harmony;
using MarkARoute.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Patches
{
    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch("ReleaseSegment")]
    class NetManagerPatch
    {
        /// <summary>
        /// Postfix, delete route markers/ sign overrides if road segment is deleted
        /// </summary>
        /// <param name="__instance"></param>
        public static void Postfix(NetManager __instance, ref ushort segment)
        {
            if( __instance.m_segments.m_buffer[segment].m_flags == NetSegment.Flags.None)
            {
                if (RouteManager.instance.m_routeDict.ContainsKey(segment))
                {
                    RouteManager.instance.DelRoadRoute(segment);
                }
                if (RouteManager.instance.m_overrideSignDict.ContainsKey(segment))
                {
                    RouteManager.instance.m_overrideSignDict.Remove(segment);
                }
            }

          
        }
    }

    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch("TerrainUpdated")]
    class NetManagerSimulationPatch
    {
        public static void Postfix()
        {
            foreach (SignContainer container in RouteManager.instance.m_signList)
            {
                if (container.m_signObj)
                {
                    Vector3 containerPos = container.pos;
                    float currHeight = container.terrainY;
                    float terrainHeight = TerrainManager.instance.SampleDetailHeight(containerPos);
                    float diffHeight = terrainHeight - currHeight;
                    if ( Math.Abs(diffHeight) > 0.001f)
                    {
                        container.terrainY = terrainHeight;
                        containerPos.y += diffHeight;
                        container.pos.y = containerPos.y;
                        container.m_signObj.transform.position = containerPos;
                    }
       
                }
            }

           
        }
    }
}
