using MarkARoute.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkARoute.Tools
{
    class StaticSignPlacementTool : SignPlacementTool
    {
        protected override void Awake()
        {
            base.BaseAwake();
            for (uint i = 0; i < PrefabCollection<PropInfo>.LoadedCount(); ++i)
            {
                if (PrefabCollection<PropInfo>.GetLoaded(i).name.ToLower().Contains("hwysign"))
                {
                    base.m_propInfo = PrefabCollection<PropInfo>.GetLoaded(i);
                }
            }
        }

        protected override void HandleSignPlaced()
        {
            RouteManager.Instance().SetSign(this.m_cachedPosition, this.m_cachedAngle, routePrefix, routeStr, destination);
            RenderingManager.instance.ForceUpdate();
            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
            ToolsModifierControl.SetTool<DefaultTool>();
        }
    }
}
