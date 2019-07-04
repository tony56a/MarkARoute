using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Tools
{
    class StaticSignPlacementTool : SignPlacementTool
    {
        public string signType;

        public bool useTextureReplace;
        public List<string> textureReplaceStrings;
        public Color color;

        protected override void Awake()
        {
            base.BaseAwake();
        }

        public void SetPropInfo(string signType)
        {
            this.signType = signType;
            RenderingManager.instance.ForceUpdate(false);
            base.m_propInfo = PropUtils.m_signPropDict[signType];
        }

        protected override void HandleSignPlaced()
        {
            if (useTextureReplace)
            {
                RouteManager.instance.SetSign(this.m_cachedPosition, this.m_cachedAngle, this.signType, textureReplaceStrings);
            }else
            {
                RouteManager.instance.SetSign(this.m_cachedPosition, this.m_cachedAngle, routePrefix, routeStr, destination, this.signType, this.color);
            }
            RenderingManager.instance.ForceUpdate(false);
            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
            ToolsModifierControl.SetTool<DefaultTool>();
        }
    }
}
