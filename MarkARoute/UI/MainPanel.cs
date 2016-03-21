using ColossalFramework.UI;
using MarkARoute.Managers;
using MarkARoute.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.UI
{
    public class MainPanel : UICustomControl
    {
        RoadSelectorTool mRoadSelectTool;
        UIButton markRouteBtn;

        private GameObject m_namingPanelObject;
        private GameObject m_usedRoutesPanelObject;
        private RouteNamePanel m_namingPanel;
        private UsedRoutesPanel m_usedRoutesPanel;

        public MainPanel()
        {
            UIView uiView = UIView.GetAView();
            markRouteBtn = (UIButton)uiView.AddUIComponent(typeof(UIButton));

            markRouteBtn.text = "Mark a Route";
            markRouteBtn.width = 150;
            markRouteBtn.height = 30;
            markRouteBtn.normalBgSprite = "ButtonMenu";
            markRouteBtn.disabledBgSprite = "ButtonMenuDisabled";
            markRouteBtn.hoveredBgSprite = "ButtonMenuHovered";
            markRouteBtn.focusedBgSprite = "ButtonMenuFocused";
            markRouteBtn.pressedBgSprite = "ButtonMenuPressed";
            markRouteBtn.textColor = new Color32(255, 255, 255, 255);
            markRouteBtn.disabledTextColor = new Color32(7, 7, 7, 255);
            markRouteBtn.hoveredTextColor = new Color32(7, 132, 255, 255);
            markRouteBtn.focusedTextColor = new Color32(255, 255, 255, 255);
            markRouteBtn.pressedTextColor = new Color32(30, 30, 44, 255);
            markRouteBtn.eventClick += markRouteBtn_eventClick;
            markRouteBtn.relativePosition = new Vector3(150f, 60f);

            m_namingPanelObject = new GameObject("RouteNamePanel");
            m_namingPanel = m_namingPanelObject.AddComponent<RouteNamePanel>();
            m_namingPanel.transform.parent = uiView.transform;
            m_namingPanel.Hide();

            m_usedRoutesPanelObject = new GameObject("UsedRoutesPanel");
            m_usedRoutesPanel = m_usedRoutesPanelObject.AddComponent<UsedRoutesPanel>();
            m_usedRoutesPanel.transform.parent = uiView.transform;
            m_usedRoutesPanel.Hide();

            EventBusManager.Instance().Subscribe("forceupdateroadnames", m_usedRoutesPanel);
            EventBusManager.Instance().Subscribe("closeUsedNamePanel", m_usedRoutesPanel);
            EventBusManager.Instance().Subscribe("closeAll", m_usedRoutesPanel);
            EventBusManager.Instance().Subscribe("closeAll", m_namingPanel);
            EventBusManager.Instance().Subscribe("updateroutepaneltext", m_namingPanel);
        }

        private void markRouteBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {

            if (mRoadSelectTool == null)
            {
                if (!ToolsModifierControl.toolController.gameObject.GetComponent<RoadSelectorTool>())
                {
                    ToolsModifierControl.toolController.gameObject.AddComponent<RoadSelectorTool>();
                }
                mRoadSelectTool = ToolsModifierControl.toolController.gameObject.GetComponent<RoadSelectorTool>();
                mRoadSelectTool.m_namingPanel = m_namingPanel;
                mRoadSelectTool.m_usedRoutesPanel = m_usedRoutesPanel;
                ToolsModifierControl.toolController.CurrentTool = mRoadSelectTool;
                ToolsModifierControl.SetTool<RoadSelectorTool>();
            }
            else
            {
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
                UnityEngine.Object.Destroy(mRoadSelectTool);
                mRoadSelectTool = null;
            }
        }

        private void EnableTool()
        {
            if (mRoadSelectTool == null)
            {

            }
        }
    }
}
