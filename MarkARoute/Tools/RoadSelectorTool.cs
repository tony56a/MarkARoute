using ColossalFramework;
using MarkARoute.Managers;
using MarkARoute.UI;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Tools
{
    class RoadSelectorTool: DefaultTool
    {
        public RouteNamePanel m_namingPanel = null;
        public UsedRoutesPanel m_usedRoutesPanel = null;
        public bool isDynamic = false;
        public DynamicSignPlacementTool m_dynamicSignPlacementTool = null;

        protected override void Awake()
        {
            LoggerUtils.Log("Tool awake");

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
                RaycastOutput raycastOutput;

                if (RaycastRoad(out raycastOutput))
                {
                    ushort netSegmentId = raycastOutput.m_netSegment;
                    ushort nodeId = raycastOutput.m_netNode;
                    if (netSegmentId != 0)
                    {
                        NetManager netManager = NetManager.instance;
                        NetSegment netSegment = netManager.m_segments.m_buffer[(int)netSegmentId];

                        if (netSegment.m_flags.IsFlagSet(NetSegment.Flags.Created))
                        {
                            if (Event.current.type == EventType.MouseDown /*&& Event.current.button == (int)UIMouseButton.Left*/)
                            {
                                //unset tool
                                ShowToolInfo(false, null, new Vector3());

                                if (m_namingPanel != null)
                                {
#if DEBUG
                                    NetNode startNode = netManager.m_nodes.m_buffer[netSegment.m_startNode]; //Not used yet, but there just incase. This isn't final
                                    Vector3 rotation = netSegment.GetDirection(netSegment.m_startNode);
                                    LoggerUtils.LogToConsole(rotation.ToString());
#endif
                                    if(!isDynamic)
                                    {
                                        m_namingPanel.m_initialRouteStr = RouteManager.instance.GetRouteStr(netSegmentId);
                                        m_namingPanel.m_initialRoutePrefix = RouteManager.instance.GetRouteType(netSegmentId);
                                        m_namingPanel.UpdateTextField();

                                        m_namingPanel.m_netSegmentId = netSegmentId;
                                        m_namingPanel.m_netSegmentName = netSegment.Info.name.Replace(" ", "");
                                        m_namingPanel.Show();
                                        m_usedRoutesPanel.RefreshList();
                                        m_usedRoutesPanel.Show();
                                    }
                                    else
                                    {
                                        m_dynamicSignPlacementTool.segmentId = netSegmentId;
                                        m_dynamicSignPlacementTool.routeStr = RouteManager.instance.GetRouteStr(netSegmentId);
                                        m_dynamicSignPlacementTool.routePrefix = RouteManager.instance.GetRouteType(netSegmentId);
                                        ToolsModifierControl.toolController.CurrentTool = m_dynamicSignPlacementTool;
                                        ToolsModifierControl.SetTool<DynamicSignPlacementTool>();
                                        EventBusManager.Instance().Publish("closeAll", null);
                                    }
                                  
                                }
                            }
                            else
                            {
                                if (!isDynamic)
                                {
                                    ShowToolInfo(true, "Mark this route", netSegment.m_bounds.center);
                                }
                                else
                                {
                                    ShowToolInfo(true, "Add a traffic sign here", netSegment.m_bounds.center);
                                }
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

        bool RaycastRoad(out RaycastOutput raycastOutput)
        {
            RaycastInput raycastInput = new RaycastInput(Camera.main.ScreenPointToRay(Input.mousePosition), Camera.main.farClipPlane);
            raycastInput.m_netService.m_service = ItemClass.Service.Road;
            raycastInput.m_netService.m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels;
            raycastInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
            raycastInput.m_ignoreNodeFlags = NetNode.Flags.None;
            raycastInput.m_ignoreTerrain = true;

            return RayCast(raycastInput, out raycastOutput);
        }
    }
}

