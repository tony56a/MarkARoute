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
            for (uint i = 0; i < PrefabCollection<PropInfo>.LoadedCount(); ++i)
            {
                if (PrefabCollection<PropInfo>.GetLoaded(i).name.ToLower().Contains("electronic_sign_gantry"))
                {
                    base.m_propInfo = PrefabCollection<PropInfo>.GetLoaded(i);
                }
            }
        }

        protected override void HandleSignPlaced()
        {
            RouteManager.Instance().SetDynamicSign(this.m_cachedPosition, this.m_cachedAngle, routePrefix, routeStr, segmentId);
            RenderingManager.instance.ForceUpdate();
            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
            ToolsModifierControl.SetTool<DefaultTool>();
        }
    }
}
