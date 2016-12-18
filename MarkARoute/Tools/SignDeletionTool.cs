using ColossalFramework.Math;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Tools
{
    class SignDeletionTool : DefaultTool
    {
        public bool isDynamic = false;

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
                DynamicSignContainer dynamicContainer;
                SignContainer container;
                Ray currentPosition = Camera.main.ScreenPointToRay(Input.mousePosition);

                if( isDynamic)
                {
                    if (RaycastDynamicSign(currentPosition, out dynamicContainer))
                    {
                        if (dynamicContainer != null)
                        {
                            if (Event.current.type == EventType.MouseDown /*&& Event.current.button == (int)UIMouseButton.Left*/)
                            {
                                //unset tool
                                ShowToolInfo(false, null, new Vector3());
                                GameObject.Destroy(dynamicContainer.m_signObj);
                                RouteManager.instance.m_dynamicSignList.Remove(dynamicContainer);
                            }
                            else
                            {
                                ShowToolInfo(true, "Delete this sign", dynamicContainer.pos);
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
                                RouteManager.instance.m_signList.Remove(container);
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

        bool RaycastDynamicSign(Ray currentPosition, out DynamicSignContainer returnValue)
        {
            Vector3 origin = currentPosition.origin;
            Vector3 normalized = currentPosition.direction.normalized;
            Vector3 _b = currentPosition.origin + normalized * Camera.main.farClipPlane;
            Segment3 ray = new Segment3(origin, _b);

            foreach (DynamicSignContainer container in RouteManager.instance.m_dynamicSignList)
            {
                if (ray.DistanceSqr(container.pos) < 500)
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
                if (ray.DistanceSqr(container.pos) < 800)
                {
                    returnValue = container;
                    return true;
                }
            }

            returnValue = null;
            return false;
        }
    }
}
