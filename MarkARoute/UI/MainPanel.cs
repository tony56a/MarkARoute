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
        UIButton markRouteBtn;
        ModPanel modPanel;

        private bool m_isUiShowing;

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
            markRouteBtn.relativePosition = new Vector3(180f, 60f);

            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
            ToolsModifierControl.SetTool<DefaultTool>();
        }

        private void markRouteBtn_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
            ToolsModifierControl.SetTool<DefaultTool>();

            if (m_isUiShowing)
            {
                hideUI();
            }
            else
            {
                showUI();
            }
        }

        private void showUI()
        {
            UIView uiView = UIView.GetAView();
            if (modPanel != null)
            {
                modPanel.Show();
            }
            else
            {
                modPanel = uiView.AddUIComponent(typeof(ModPanel)) as ModPanel;
            }

            m_isUiShowing = true;
        }

        private void hideUI()
        {
            if(modPanel != null)
            {
                modPanel.Hide();
            }
            m_isUiShowing = false;
            EventBusManager.Instance().Publish("closeAll", null);
        }
    }
}
