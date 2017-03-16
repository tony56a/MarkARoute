using System.Reflection;
using Harmony;
using ColossalFramework.Math;
using MarkARoute.Managers;
using UnityEngine;
using ColossalFramework.UI;

namespace MarkARoute.Patches
{
    public static class ContainerHolder
    {
        public static SignContainer container;

    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("RenderOverlay")]
    public class BulldozerToolOverlayPatch
    {
        static Color toolColor = Color.red;

        public static bool Prefix(BulldozeTool __instance, ref RenderManager.CameraInfo cameraInfo)
        {
            if (ContainerHolder.container != null)
            {
                float size = Mathf.Max(ContainerHolder.container.m_sign.mesh.bounds.extents.x, ContainerHolder.container.m_sign.mesh.bounds.extents.z);
                ++ToolManager.instance.m_drawCallData.m_overlayCalls;
                RenderManager.instance.OverlayEffect.DrawCircle(RenderManager.instance.CurrentCameraInfo, toolColor, ContainerHolder.container.pos, size, ContainerHolder.container.pos.y - 100f, ContainerHolder.container.pos.y + 100f, true, false);
                return false;

            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("OnToolGUI")]
    public class BulldozerToolDeletePatch
    {

        public static bool Prefix()
        {
            if (ContainerHolder.container != null)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == (int)UIMouseButton.None)
                {

                    GameObject.Destroy(ContainerHolder.container.m_signObj);
                    DynamicSignContainer dynamicSign = ContainerHolder.container as DynamicSignContainer;
                    if (dynamicSign != null)
                    {
                        RouteManager.instance.m_dynamicSignList.Remove(dynamicSign);
                    }
                    else
                    {
                        RouteManager.instance.m_signList.Remove(ContainerHolder.container);
                    }
                    PropTool.DispatchPlacementEffect(ContainerHolder.container.pos, true);

                    ContainerHolder.container = null;
                }
                return false;
            }
            return true;
        }

    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("OnToolUpdate")]
    public class BulldozerToolPatch
    {

        static bool RaycastSign(Ray currentPosition, out SignContainer returnValue)
        {
            Vector3 origin = currentPosition.origin;
            Vector3 normalized = currentPosition.direction.normalized;
            Vector3 _b = currentPosition.origin + normalized * Camera.main.farClipPlane;
            Segment3 ray = new Segment3(origin, _b);

            foreach (SignContainer container in RouteManager.instance.m_signList)
            {
                if (ray.DistanceSqr(container.pos) < 30)
                {
                    returnValue = container;
                    return true;
                }
            }
            foreach (DynamicSignContainer container in RouteManager.instance.m_dynamicSignList)
            {
                if (ray.DistanceSqr(container.pos) < 30)
                {
                    returnValue = container;
                    return true;
                }
            }

            returnValue = null;
            return false;
        }



        public static void Postfix()
        {

            BulldozeTool tool = ToolsModifierControl.toolController.CurrentTool as BulldozeTool;
            if (tool != null && tool.enabled)
            {
                MethodInfo showToolInfo = typeof(DefaultTool).GetMethod("ShowToolInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                Ray currentPosition = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastSign(currentPosition, out ContainerHolder.container);
                if (ContainerHolder.container != null)
                {
                    showToolInfo.Invoke(tool, new object[] { true, "Delete this sign", ContainerHolder.container.pos });
                   
                }
                else
                {
                    ContainerHolder.container = null;
                    showToolInfo.Invoke(tool, new object[] { false, null, new Vector3() });
                }

            }

        }
    }
}
