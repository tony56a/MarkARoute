using ColossalFramework.UI;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.UI
{
    class DeleteSignRowItem : UIPanel, IUIFastListRow
    {
        private UIPanel background;
        private UILabel label;
        private SignContainer container;

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width;
            height = 80;

            background = AddUIComponent<UIPanel>();
            background.width = width;
            background.height = 80;
            background.relativePosition = Vector2.zero;
            background.zOrder = 0;

            label = this.AddUIComponent<UILabel>();
            label.textScale = 1f;
            label.size = new Vector2(width, height);
            label.textColor = new Color32(180, 180, 180, 255);
            label.relativePosition = new Vector2(0, height * 0.25f);
            label.textAlignment = UIHorizontalAlignment.Left;
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {

            base.OnMouseDown(p);
            EventBusManager.Instance().Publish("deleteSign", container);
        }
    

        public void Display(object data, bool isRowOdd)
        {
            if (data != null)
            {
                SignContainer signContainer = data as SignContainer;

                if (signContainer != null && background != null)
                {
                    container = signContainer;
                    label.text = ( String.IsNullOrEmpty(signContainer.m_routePrefix) ? "" : signContainer.m_routePrefix ) +
                                 ( String.IsNullOrEmpty(signContainer.m_route) ? "" : signContainer.m_route ) +
                                 '\n' +
                                 ( String.IsNullOrEmpty(signContainer.m_destination) ? "" : signContainer.m_destination );

                    if (isRowOdd)
                    {
                        background.backgroundSprite = "UnlockingItemBackground";
                        background.color = new Color32(0, 0, 0, 128);
                    }
                    else
                    {
                        background.backgroundSprite = null;
                    }
                }
            }

        }

        public void Select(bool isRowOdd)
        {
            if (background != null)
            {
                /*background.backgroundSprite = "ListItemHighlight";
                background.color = new Color32(255, 255, 255, 255);*/
            }
        }

        public void Deselect(bool isRowOdd)
        {
            if (background != null)
            {
                if (isRowOdd)
                {
                    background.backgroundSprite = "UnlockingItemBackground";
                    background.color = new Color32(0, 0, 0, 128);
                }
                else
                {
                    background.backgroundSprite = null;
                }
            }
        }

    }
}
