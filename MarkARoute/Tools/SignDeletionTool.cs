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
                ushort segmentId;
                SignContainer container;
                Ray currentPosition = Camera.main.ScreenPointToRay(Input.mousePosition);

                if( isDynamic)
                {
                    if (RaycastDynamicSign(currentPosition, out segmentId))
                    {
                        if (segmentId != 0)
                        {
                            if (Event.current.type == EventType.MouseDown /*&& Event.current.button == (int)UIMouseButton.Left*/)
                            {
                                //unset tool
                                ShowToolInfo(false, null, new Vector3());
                                GameObject.Destroy(RouteManager.Instance().m_dynamicSignDict[segmentId].m_signObj);
                                RouteManager.Instance().m_dynamicSignDict.Remove(segmentId);
                            }
                            else
                            {
                                ShowToolInfo(true, "Delete this sign", RouteManager.Instance().m_dynamicSignDict[segmentId].pos);
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
                                RouteManager.Instance().m_signList.Remove(container);
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

        bool RaycastDynamicSign(Ray currentPosition, out ushort containerKey)
        {
            Vector3 origin = currentPosition.origin;
            Vector3 normalized = currentPosition.direction.normalized;
            Vector3 _b = currentPosition.origin + normalized * Camera.main.farClipPlane;
            Segment3 ray = new Segment3(origin, _b);

            foreach (DynamicSignContainer container in RouteManager.Instance().m_dynamicSignDict.Values)
            {
                if (ray.DistanceSqr(container.pos) < 800)
                {
                    containerKey = container.m_segment;
                    return true;
                }
            }

            containerKey = 0;
            return false;
        }

        bool RaycastStaticSign(Ray currentPosition, out SignContainer returnValue)
        {
            Vector3 origin = currentPosition.origin;
            Vector3 normalized = currentPosition.direction.normalized;
            Vector3 _b = currentPosition.origin + normalized * Camera.main.farClipPlane;
            Segment3 ray = new Segment3(origin, _b);

            foreach (SignContainer container in RouteManager.Instance().m_signList)
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
