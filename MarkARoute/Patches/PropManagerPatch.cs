using Harmony;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Patches
{
    /*[HarmonyPatch(typeof(PropManager))]
    [HarmonyPatch("CreateProp")]*/
    class PropCreatePatch
    {
        public static bool Prefix(ref PropInfo info, ref Vector3 position, ref float angle, ref bool single)
        {
            if( !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ) )
            {
                return true;
            }
            else
            {
                string str = String.Format("Invoking prop at:{0}, angle:{1}, info:{2}", position, angle, info.GetLocalizedTitle());
                LoggerUtils.Log(str);
                PropInstance newProp = new PropInstance();
                newProp.m_flags = ((ushort)PropInstance.Flags.Created | 32768 | 16384 );
                PropInfo prefab = PrefabCollection<PropInfo>.GetPrefab((uint)info.m_prefabDataIndex);
                newProp.Info = prefab;
                newProp.Single = single;
                newProp.Blocked = false;
                newProp.Position = position;
                newProp.Angle = angle;
                AltPropManager.instance.SetProp(position, newProp);
                PropTool.DispatchPlacementEffect(position, true);
                PropManager.instance.UpdateProp(0);
                return false;
            }
        }
    }

    /*[HarmonyPatch(typeof(PropManager))]
    [HarmonyPatch("EndRenderingImpl")]*/
    class PropRenderingPatch
    {
        public static void Postfix(ref RenderManager.CameraInfo cameraInfo)
        {
            ItemClass.Availability availability = ToolManager.instance.m_properties.m_mode;
            FastList<RenderGroup> fastList = RenderManager.instance.m_renderedGroups;
            int num1 = 1 << LayerMask.NameToLayer("Props") | 1 << RenderManager.instance.lightSystem.m_lightLayer;
            for (int index1 = 0; index1 < fastList.m_size; ++index1)
            {
                RenderGroup renderGroup = fastList.m_buffer[index1];
                if ((renderGroup.m_instanceMask & num1) != 0)
                {
                    int num2 = renderGroup.m_x * 270 / 45;
                    int num3 = renderGroup.m_z * 270 / 45;
                    int num4 = (renderGroup.m_x + 1) * 270 / 45 - 1;
                    int num5 = (renderGroup.m_z + 1) * 270 / 45 - 1;
                    for (int index2 = num3; index2 <= num5; ++index2)
                    {
                        for (int index3 = num2; index3 <= num4; ++index3)
                        {
                            int gridKey = index2 * 270 + index3;
                            if (AltPropManager.instance.Props.ContainsKey(gridKey)){
                                List<PropInstance> list = AltPropManager.instance.Props[gridKey];
                                foreach (PropInstance instance in list)
                                {
                                    instance.RenderInstance(cameraInfo, 0, renderGroup.m_instanceMask);
                                }
                            }
                           

                        }
                    }
                }
            }
         
        }
    }
}
