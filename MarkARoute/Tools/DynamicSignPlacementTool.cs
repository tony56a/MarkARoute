using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkARoute.Tools
{
    class DynamicSignPlacementTool : SignPlacementTool
    {
        public ushort segmentId;

        protected override void Awake()
        {
            base.BaseAwake();
        }

        public void SetPropInfo()
        {
            RenderingManager.instance.ForceUpdate(false);
            RenderingManager.instance.m_signPropDict.TryGetValue("electronic_sign_gantry", out this.m_propInfo);
        }

        protected override void HandleSignPlaced()
        {
            RouteManager.instance.SetDynamicSign(this.m_cachedPosition, this.m_cachedAngle, routePrefix, routeStr, segmentId);
            RenderingManager.instance.ForceUpdate(false);
            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
            ToolsModifierControl.SetTool<DefaultTool>();
        }
    }
}
