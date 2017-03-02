using ColossalFramework;
using ColossalFramework.Math;
using MarkARoute.Managers;
using MarkARoute.Utils;
using UnityEngine;

namespace MarkARoute.Tools
{
    class SignDeletionTool : DefaultTool
    {
        public bool isDynamic = false;
        private Randomizer m_randomizer;
        SignContainer container;
        Color toolColor = new Color(1f, 1f, 3f / 16f, 0.75f);

        protected override void Awake()
        {
            LoggerUtils.Log("Sign deletion Tool awake");

            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventBusManager.Instance().Publish("closeAll", null);

        }

        protected override void OnToolUpdate()
        {
            if (m_toolController != null && !m_toolController.IsInsideUI && Cursor.visible)
            {
                Ray currentPosition = Camera.main.ScreenPointToRay(Input.mousePosition);

                if( isDynamic)
                {
                    if (RaycastDynamicSign(currentPosition, out container))
                    {
                        if (container != null)
                        {

                            if (Event.current.type == EventType.MouseDown /*&& Event.current.button == (int)UIMouseButton.Left*/)
                            {
                                //unset tool
                                ShowToolInfo(false, null, new Vector3());
                                GameObject.Destroy(container.m_signObj);
                                PropTool.DispatchPlacementEffect(container.pos, true);
                                RouteManager.instance.m_dynamicSignList.Remove(container as DynamicSignContainer);
                                container = null;
                            }
                            else
                            {
                                ShowToolInfo(true, "Delete this sign", container.pos);
                            }

                        }
                    }
                }

                else
                {
                    if (RaycastStaticSign(currentPosition, out container))
                    {
                        if (container != null)
                        {
                            if (Event.current.type == EventType.MouseDown /*&& Event.current.button == (int)UIMouseButton.Left*/)
                            {
                                //unset tool
                                ShowToolInfo(false, null, new Vector3());
                                GameObject.Destroy(container.m_signObj);
                                PropTool.DispatchPlacementEffect(container.pos, true);
                                RouteManager.instance.m_signList.Remove(container);
                                container = null;
                            }
                            else
                            {
                                ShowToolInfo(true, "Delete this sign", container.pos);
                            }

                        }
                    }
                }
              
               
            }
            else
            {
                ShowToolInfo(false, null, new Vector3());
            }
        }

        bool RaycastDynamicSign(Ray currentPosition, out SignContainer returnValue)
        {
            Vector3 origin = currentPosition.origin;
            Vector3 normalized = currentPosition.direction.normalized;
            Vector3 _b = currentPosition.origin + normalized * Camera.main.farClipPlane;
            Segment3 ray = new Segment3(origin, _b);

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

        bool RaycastStaticSign(Ray currentPosition, out SignContainer returnValue)
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

            returnValue = null;
            return false;
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            Randomizer r1 = this.m_randomizer;
            Randomizer r2 = new Randomizer((int)Singleton<PropManager>.instance.m_props.NextFreeItem(ref r1));

            if (container != null && (!this.m_toolController.IsInsideUI && Cursor.visible))
            {
                RenderOverlay(cameraInfo, container);
            }
            base.RenderOverlay(cameraInfo);
        }

        private void RenderOverlay(RenderManager.CameraInfo cameraInfo, SignContainer container )
        {
            float size = Mathf.Max(container.m_sign.mesh.bounds.extents.x, container.m_sign.mesh.bounds.extents.z);
            ++Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls;
            Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, toolColor, container.pos, size, container.pos.y - 100f, container.pos.y + 100f, false, true);
        }

    }
}
