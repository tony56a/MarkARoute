using ColossalFramework.Math;
using ColossalFramework.UI;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MarkARoute.UI;

namespace MarkARoute.Tools
{
    class SignOverrideSelectorTool : DefaultTool
    {
        List<NetLaneProps.Prop> highwayProps = PropUtils.findHighwaySignProp();
        ushort netSegmentId = 0;
        bool isValid = false;
        public AddOverrideSignPanel m_overrideSignPanel;

        protected override void OnToolUpdate()
        {
            if (m_toolController != null && !m_toolController.IsInsideUI && Cursor.visible)
            {
                netSegmentId = 0;
                isValid = false;
                RaycastOutput raycastOutput;
                if (RaycastRoad(out raycastOutput))
                {
                    netSegmentId = raycastOutput.m_netSegment;
                    ushort nodeId = raycastOutput.m_netNode;
                    if (netSegmentId != 0)
                    {
                        NetManager netManager = NetManager.instance;
                        NetSegment netSegment = netManager.m_segments.m_buffer[(int)netSegmentId];
                        NetLane lanes = netManager.m_lanes.m_buffer[netSegment.m_lanes];
                        NetNode.Flags startFlags = netManager.m_nodes.m_buffer[ (netSegment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None ? netSegment.m_endNode : netSegment.m_startNode].m_flags;
                        NetNode.Flags endFlags = netManager.m_nodes.m_buffer[(netSegment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None ? netSegment.m_startNode : netSegment.m_endNode].m_flags;

                        foreach (NetInfo.Lane lane in netSegment.Info.m_lanes)
                        {
                            foreach (NetLaneProps.Prop prop in lane.m_laneProps.m_props.Where(prop => prop != null))
                            {
                                if (prop.m_prop != null && prop.m_prop.name != null && prop.m_prop.name.ToLower().Contains("motorway overroad signs"))
                                {
                                    isValid |= prop.CheckFlags((NetLane.Flags)lanes.m_flags, startFlags, endFlags);
                                }
                            }
                        }

                        if (isValid)
                        {
                            if (Event.current.type == EventType.MouseDown && Event.current.button == (int)UIMouseButton.None )
                            {
                                ShowToolInfo(false, null, new Vector3());
                                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                                ToolsModifierControl.SetTool<DefaultTool>();
                                m_overrideSignPanel.netSegmentId = netSegmentId;
                                m_overrideSignPanel.Show();
                            }
                            
                        }
                       
                    }    
                }
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
        
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if( isValid )
            {
                RenderOverlay(cameraInfo, netSegmentId, Color.green);
            }
            base.RenderOverlay(cameraInfo);
        }

        private void RenderOverlay(RenderManager.CameraInfo cameraInfo, ushort netSegmentId, Color toolColor)
        {
            NetSegment segment = NetManager.instance.m_segments.m_buffer[netSegmentId];

            NetInfo info = segment.Info;
            if (info == null || (segment.m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None && !info.m_overlayVisible)
                return;
            Bezier3 bezier;
            bezier.a = NetManager.instance.m_nodes.m_buffer[(int)segment.m_startNode].m_position;
            bezier.d = NetManager.instance.m_nodes.m_buffer[(int)segment.m_endNode].m_position;
            NetSegment.CalculateMiddlePoints(bezier.a, segment.m_startDirection, bezier.d, segment.m_endDirection, false, false, out bezier.b, out bezier.c);
            bool flag1 = false;
            bool flag2 = false;
            ++ToolManager.instance.m_drawCallData.m_overlayCalls;
            RenderManager.instance.OverlayEffect.DrawBezier(cameraInfo, toolColor, bezier, info.m_halfWidth * 2f, !flag1 ? -100000f : info.m_halfWidth, !flag2 ? -100000f : info.m_halfWidth, -1f, 1280f, false, false);
        }
    }
}
