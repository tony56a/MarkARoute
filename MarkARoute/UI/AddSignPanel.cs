using ColossalFramework.UI;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MarkARoute.Tools;

namespace MarkARoute.UI
{
    class AddSignPanel : UIPanel,IEventSubscriber
    {
        protected RectOffset m_UIPadding = new RectOffset(5, 5, 5, 5);

        private TitleBar m_panelTitle;
        private UIDropDown m_routeTypeDropdown;
        private UITextField m_routeStrField;
        private UILabel m_routeLabel;

        private UITextField[] m_destinationField = new UITextField[2];
        private UILabel m_destinationLabel;

        public StaticSignPlacementTool mSignPlacementTool;

        public override void Awake()
        {
            this.isInteractive = true;
            this.enabled = true;
            this.width = 250;
            this.height = 400;

            base.Awake();
        }

        public override void Start()
        {
            base.Start();

            m_panelTitle = this.AddUIComponent<TitleBar>();
            m_panelTitle.title = "Set a name";
            m_panelTitle.m_closeActions.Add("closeAll");

            CreatePanelComponents();

            this.relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
            this.backgroundSprite = "MenuPanel2";
            this.eventKeyPress += RoadNamePanel_eventKeyPress;
        }

        private void RoadNamePanel_eventKeyPress(UIComponent component, UIKeyEventParameter eventParam)
        {
            if (eventParam.keycode == KeyCode.KeypadEnter || eventParam.keycode == KeyCode.Return)
            {
                SetRoadData();
            }
        }

        private void CreatePanelComponents()
        {
            m_routeLabel = this.AddUIComponent<UILabel>();
            m_routeLabel.textScale = 1f;
            m_routeLabel.size = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_routeLabel.textColor = new Color32(180, 180, 180, 255);
            m_routeLabel.relativePosition = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_routeLabel.textAlignment = UIHorizontalAlignment.Left;
            m_routeLabel.text = "Route Name";

            m_routeTypeDropdown = UIUtils.CreateDropDown(this, new Vector2(((this.width - m_UIPadding.left - 2 * m_UIPadding.right)), 25));
            //TODO: Replace with Random namer values
            foreach (RouteShieldInfo info in RouteShieldConfig.Instance().routeShieldDictionary.Values)
            {
                m_routeTypeDropdown.AddItem(info.textureName);
            }
            m_routeTypeDropdown.selectedIndex = 0;
            m_routeTypeDropdown.relativePosition = new Vector3(m_UIPadding.left, m_routeLabel.relativePosition.y + m_routeLabel.height + m_UIPadding.bottom);

            m_routeStrField = UIUtils.CreateTextField(this);
            m_routeStrField.relativePosition = new Vector3(m_UIPadding.left, m_routeTypeDropdown.relativePosition.y + m_routeTypeDropdown.height + m_UIPadding.bottom);
            m_routeStrField.height = 25;
            m_routeStrField.width = (this.width - m_UIPadding.left - 2 * m_UIPadding.right);
            m_routeStrField.processMarkup = false;
            m_routeStrField.textColor = Color.white;
            m_routeStrField.maxLength = 3;

            m_destinationLabel = this.AddUIComponent<UILabel>();
            m_destinationLabel.textScale = 1f;
            m_destinationLabel.size = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_destinationLabel.textColor = new Color32(180, 180, 180, 255);
            m_destinationLabel.relativePosition = new Vector3(m_UIPadding.left, m_routeStrField.relativePosition.y + m_routeStrField.height + m_UIPadding.bottom);
            m_destinationLabel.textAlignment = UIHorizontalAlignment.Left;
            m_destinationLabel.text = "Destination Name";

            m_destinationField[0] = UIUtils.CreateTextField(this);
            m_destinationField[0].relativePosition = new Vector3(m_UIPadding.left, m_destinationLabel.relativePosition.y + m_destinationLabel.height + m_UIPadding.bottom);
            m_destinationField[0].height = 25;
            m_destinationField[0].width = (this.width - m_UIPadding.left - 2 * m_UIPadding.right);
            m_destinationField[0].processMarkup = false;
            m_destinationField[0].textColor = Color.white;
            m_destinationField[0].maxLength = 12;

            m_destinationField[1] = UIUtils.CreateTextField(this);
            m_destinationField[1].relativePosition = new Vector3(m_UIPadding.left, m_destinationField[0].relativePosition.y + m_destinationField[0].height + m_UIPadding.bottom);
            m_destinationField[1].height = 25;
            m_destinationField[1].width = (this.width - m_UIPadding.left - 2 * m_UIPadding.right);
            m_destinationField[1].processMarkup = false;
            m_destinationField[1].textColor = Color.white;
            m_destinationField[1].maxLength = 12;

            UIButton nameRoadButton = UIUtils.CreateButton(this);
            nameRoadButton.text = "Set";
            nameRoadButton.size = new Vector2(60, 30);
            nameRoadButton.relativePosition = new Vector3(this.width - nameRoadButton.width - m_UIPadding.right, m_destinationField[1].relativePosition.y + m_destinationField[1].height + m_UIPadding.bottom);
            nameRoadButton.eventClicked += NameRoadButton_eventClicked;
            nameRoadButton.tooltip = "Create the label";

            this.height = nameRoadButton.relativePosition.y + nameRoadButton.height + m_UIPadding.bottom;
        }

        private void m_textField_eventKeyDown(UIComponent component, UIKeyEventParameter eventParam)
        {
            if (eventParam.keycode == KeyCode.KeypadEnter || eventParam.keycode == KeyCode.Return)
            {
                SetRoadData();
            }
        }

        private void NameRoadButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            SetRoadData();
        }

        /// <summary>
        /// Gets the colour from the panel and sets it to be rendered/saved
        /// </summary>
        private void SetRoadData()
        {
            if (String.IsNullOrEmpty(m_routeStrField.text))
            {
                mSignPlacementTool.routeStr = null;
                mSignPlacementTool.routePrefix = null;
            }
            else
            {
                mSignPlacementTool.routeStr = m_routeStrField.text;
                mSignPlacementTool.routePrefix = m_routeTypeDropdown.selectedValue;
            }
            mSignPlacementTool.destination = m_destinationField[0].text+'\n'+ m_destinationField[1].text;
            ToolsModifierControl.toolController.CurrentTool = mSignPlacementTool;
            ToolsModifierControl.SetTool<StaticSignPlacementTool>();
            EventBusManager.Instance().Publish("closeAll", null);
        }
        public void onReceiveEvent(string eventName, object eventData)
        {
            string message = eventData as string;
            switch (eventName)
            {
                case "updateroutepaneltext":
                    if (message != null)
                    {

                        string[] routeValues = message.Split('/');
                        int routeType = 0;
                        for (int i = 0; i < m_routeTypeDropdown.items.Length; i++)
                        {
                            if (m_routeTypeDropdown.items[i].ToLower() == routeValues[0].ToLower())
                            {
                                routeType = i;
                                break;
                            }
                        }
                        m_routeTypeDropdown.selectedIndex = routeType;
                        m_routeStrField.text = routeValues[1];
                    }
                    break;
                case "closeAll":
                    Hide();
                    break;
                default:
                    break;
            }
        }
    }
}
