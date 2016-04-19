using ColossalFramework.UI;
using MarkARoute.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.UI
{
    class DeleteSignPanel : UIPanel, IEventSubscriber
    {
        protected RectOffset m_UIPadding = new RectOffset(5, 5, 5, 5);

        private int titleOffset = 40;
        private TitleBar m_panelTitle;
        public UIFastList signsList = null;

        private Vector2 offset = Vector2.zero;

        public override void Awake()
        {
            this.isInteractive = true;
            this.enabled = true;
            this.width = 350;
            this.height = 300;

            base.Awake();
        }

        public override void Start()
        {
            base.Start();

            m_panelTitle = this.AddUIComponent<TitleBar>();
            m_panelTitle.title = "Existing Signs";
            m_panelTitle.m_closeActions.Add("closeAll");

            CreatePanelComponents();

            this.relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2) + width, Mathf.Floor((GetUIView().fixedHeight - height) / 2));
            this.backgroundSprite = "MenuPanel2";
        }

        private void CreatePanelComponents()
        {

            signsList = UIFastList.Create <DeleteSignRowItem>(this);
            signsList.backgroundSprite = "UnlockingPanel";
            signsList.size = new Vector2(this.width - m_UIPadding.left - m_UIPadding.right, (this.height - titleOffset - m_UIPadding.top - m_UIPadding.bottom));
            signsList.canSelect = false;
            signsList.relativePosition = new Vector2(m_UIPadding.left, titleOffset + m_UIPadding.top);
            signsList.rowHeight = 80f;
            signsList.rowsData.Clear();
            signsList.selectedIndex = -1;

            RefreshList();
        }

        public void RefreshList()
        {
            signsList.rowsData.Clear();
            foreach (SignContainer signContainer in RouteManager.Instance().m_signList)
            {
                signsList.rowsData.Add(signContainer);
            }
            signsList.DisplayAt(0);
            signsList.selectedIndex = 0;
        }

        public void onReceiveEvent(string eventName, object eventData)
        {
            switch (eventName)
            {
                case "deleteSign":
                    SignContainer container = eventData as SignContainer;
                    GameObject.Destroy(container.m_signObj);
                    RouteManager.Instance().m_signList.Remove(container);
                    RefreshList();
                    break;
                case "forceUpdateSigns":
                    RefreshList();
                    break;
                case "closeSignDeletePanel":
                case "closeAll":
                    Hide();
                    break;
                default:
                    break;
            }
        }
    }
}
