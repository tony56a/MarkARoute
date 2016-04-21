using ColossalFramework.UI;
using MarkARoute.Managers;
using MarkARoute.Tools;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.UI
{
    class ModPanel : UIPanel
    {
        private static readonly int WIDTH = 170;
        private static readonly int PADDING = 10;

        private RoadSelectorTool mRoadSelectTool;
        private StaticSignPlacementTool mSignPlacementTool;
        private DynamicSignPlacementTool mDynamicSignPlacementTool;
        private SignDeletionTool mSignDeletionTool;

        private UIButton markRouteBtn;
        private UIButton addSignBtn;
        private UIButton deleteSignBtn;
        private UIButton addDynamicSignBtn;
        private UIButton deleteDynamicSignBtn;

        private GameObject m_namingPanelObject;
        private GameObject m_usedRoutesPanelObject;
        private GameObject m_addSignPanelObject;
        private GameObject m_deleteSignPanelObject;

        private AddSignPanel m_addSignPanel;
        private RouteNamePanel m_namingPanel;
        private UsedRoutesPanel m_usedRoutesPanel;
        private DeleteSignPanel m_deleteSignPanel;

        public ModPanel()
        {
            UIView uiView = UIView.GetAView();
            int yCursor = PADDING;
            this.backgroundSprite = "GenericPanel";
            color = new Color32(75, 75, 135, 255);
            width = WIDTH;
        
            markRouteBtn = getButton(yCursor, "Mark Road Segment", markRouteBtn_eventClick);
            yCursor += (30 + PADDING);
            addSignBtn = getButton(yCursor, "Add a Sign", addSignBtn_eventClick);
            yCursor += (30 + PADDING);
            deleteSignBtn = getButton(yCursor, "Delete a Sign", deleteSignBtn_eventClick);
            yCursor += (30 + PADDING);
            addDynamicSignBtn = getButton(yCursor, "Add a dynamic Sign", addDynamicSignBtn_eventClick);
            yCursor += (30 + PADDING);
            deleteDynamicSignBtn = getButton(yCursor, "Delete a dynamic Sign", deleteDynamicSignBtn_eventClick);

            this.height = addDynamicSignBtn.relativePosition.y + addDynamicSignBtn.height + PADDING * 2;

            m_namingPanelObject = new GameObject("RouteNamePanel");
            m_namingPanel = m_namingPanelObject.AddComponent<RouteNamePanel>();
            m_namingPanel.transform.parent = uiView.transform;
            m_namingPanel.Hide();

            m_usedRoutesPanelObject = new GameObject("UsedRoutesPanel");
            m_usedRoutesPanel = m_usedRoutesPanelObject.AddComponent<UsedRoutesPanel>();
            m_usedRoutesPanel.transform.parent = uiView.transform;
            m_usedRoutesPanel.Hide();

            m_addSignPanelObject = new GameObject("AddSignsPanel");
            m_addSignPanel = m_addSignPanelObject.AddComponent<AddSignPanel>();
            m_addSignPanel.transform.parent = uiView.transform;
            m_addSignPanel.Hide();

            m_deleteSignPanelObject = new GameObject("DeleteSignPanel");
            m_deleteSignPanel = m_deleteSignPanelObject.AddComponent<DeleteSignPanel>();
            m_deleteSignPanel.transform.parent = uiView.transform;
            m_deleteSignPanel.Hide();

            EventBusManager.Instance().Subscribe("forceupdateroutes", m_usedRoutesPanel);
            EventBusManager.Instance().Subscribe("closeUsedNamePanel", m_usedRoutesPanel);
            EventBusManager.Instance().Subscribe("closeAll", m_usedRoutesPanel);
            EventBusManager.Instance().Subscribe("closeAll", m_namingPanel);
            EventBusManager.Instance().Subscribe("closeAll", m_addSignPanel);
            EventBusManager.Instance().Subscribe("closeAll", m_deleteSignPanel);
            EventBusManager.Instance().Subscribe("deleteSign", m_deleteSignPanel);
            EventBusManager.Instance().Subscribe("forceUpdateSigns", m_deleteSignPanel);
            EventBusManager.Instance().Subscribe("updateroutepaneltext", m_addSignPanel);
            EventBusManager.Instance().Subscribe("updateroutepaneltext", m_namingPanel);

            mDynamicSignPlacementTool = ToolsModifierControl.toolController.gameObject.AddComponent<DynamicSignPlacementTool>();
            mSignDeletionTool = ToolsModifierControl.toolController.gameObject.AddComponent<SignDeletionTool>();

            mSignPlacementTool = ToolsModifierControl.toolController.gameObject.AddComponent<StaticSignPlacementTool>();
            m_addSignPanel.mSignPlacementTool = mSignPlacementTool;

            mRoadSelectTool = ToolsModifierControl.toolController.gameObject.AddComponent<RoadSelectorTool>();
            mRoadSelectTool.m_namingPanel = m_namingPanel;
            mRoadSelectTool.m_usedRoutesPanel = m_usedRoutesPanel;
            mRoadSelectTool.m_dynamicSignPlacementTool = mDynamicSignPlacementTool;

            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
            ToolsModifierControl.SetTool<DefaultTool>();


        }

        public override void Start()
        {
            relativePosition = new Vector3(85f, 100f);
        }

        private UIButton getButton(int y, String text, MouseEventHandler handler)
        {
            UIButton button = AddUIComponent(typeof(UIButton)) as UIButton;

            button.text = text;
            button.width = 150;
            button.height = 30;
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenuFocused";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(7, 132, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);
            button.eventClick += handler;
            button.relativePosition = new Vector3(PADDING , y);
            return button;
        }

        private void deleteDynamicSignBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            mSignDeletionTool.isDynamic = true;
            if (ToolsModifierControl.toolController.CurrentTool != mSignDeletionTool)
            {
                ToolsModifierControl.toolController.CurrentTool = mSignDeletionTool;
                ToolsModifierControl.SetTool<SignDeletionTool>();
            }
            else
            {
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            EventBusManager.Instance().Publish("closeAll", null);
        }

        private void addDynamicSignBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            mRoadSelectTool.isDynamic = true;
            if (ToolsModifierControl.toolController.CurrentTool != mRoadSelectTool)
            {
                ToolsModifierControl.toolController.CurrentTool = mRoadSelectTool;
                ToolsModifierControl.SetTool<RoadSelectorTool>();
            }
            else
            {
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            EventBusManager.Instance().Publish("closeAll", null);
        }

        private void markRouteBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            mRoadSelectTool.isDynamic = false;
            if (ToolsModifierControl.toolController.CurrentTool != mRoadSelectTool)
            {
                ToolsModifierControl.toolController.CurrentTool = mRoadSelectTool;
                ToolsModifierControl.SetTool<RoadSelectorTool>();
            }
            else
            {
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            EventBusManager.Instance().Publish("closeAll", null);
        }

        private void addSignBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_addSignPanel.isVisible)
            {
                m_addSignPanel.isVisible = false;
                m_addSignPanel.Hide();
                m_usedRoutesPanel.Hide();

            }
            else
            {
                m_addSignPanel.isVisible = true;
                m_addSignPanel.Show();
                m_usedRoutesPanel.RefreshList();
                m_usedRoutesPanel.Show();
            }

        }

        private void deleteSignBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            mSignDeletionTool.isDynamic = false;
            if (ToolsModifierControl.toolController.CurrentTool != mSignDeletionTool)
            {
                ToolsModifierControl.toolController.CurrentTool = mSignDeletionTool;
                ToolsModifierControl.SetTool<SignDeletionTool>();
            }
            else
            {
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            EventBusManager.Instance().Publish("closeAll", null);
        }

    }
}
