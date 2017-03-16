using ColossalFramework.UI;
using MarkARoute.Managers;
using MarkARoute.UI.Utils;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.UI
{
    class AddOverrideSignPanel : UIPanel, IEventSubscriber
    {

        protected RectOffset m_UIPadding = new RectOffset(5, 5, 5, 5);
        private TitleBar m_panelTitle;

        private UILabel m_propTypeLabel;
        protected UIDropDown m_propTypeDropDown;

        private float yCursor;
        private float bottomTextureYCursor;


        protected List<TextureSelectOption> mTextureSelectOptions = new List<TextureSelectOption>();
        private UIButton confirmBtn;

        public ushort netSegmentId;

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
            m_panelTitle.title = "Configure Sign";
            m_panelTitle.m_closeActions.Add("closeAll");

            CreatePanelComponents();

            this.relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
            this.backgroundSprite = "MenuPanel2";
            this.eventKeyPress += AddOverrideSignPanel_eventKeyPress;
        }

        private void CreatePanelComponents()
        {
            yCursor = m_panelTitle.height + m_UIPadding.top;

            m_propTypeLabel = this.AddUIComponent<UILabel>();
            m_propTypeLabel.textScale = 1f;
            m_propTypeLabel.size = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_propTypeLabel.textColor = new Color32(180, 180, 180, 255);
            m_propTypeLabel.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_propTypeLabel.textAlignment = UIHorizontalAlignment.Left;
            m_propTypeLabel.text = "Sign prop type";

            yCursor += m_propTypeLabel.height + m_UIPadding.bottom;

            m_propTypeDropDown = UIUtils.CreateDropDown(this, new Vector2(((this.width - m_UIPadding.left - 2 * m_UIPadding.right)), 25));

            // Add No sign and vanilla sign options
            m_propTypeDropDown.AddItem(RouteManager.NONE);
            m_propTypeDropDown.AddItem(RouteManager.VANILLA);

            var keys = PropUtils.m_signPropDict.Keys;
            foreach (String signPropName in PropUtils.m_signPropDict.Keys.Where(key => SignPropConfig.overrideSignValues.ContainsKey(key)))
            {
                m_propTypeDropDown.AddItem(signPropName);
            }
            m_propTypeDropDown.autoListWidth = true;
            m_propTypeDropDown.selectedIndex = 0;
            m_propTypeDropDown.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_propTypeDropDown.eventSelectedIndexChanged += propTypeDropDown_eventSelectedIndexChanged;

            yCursor += m_propTypeDropDown.height + m_UIPadding.bottom;

            float tempYCursor = yCursor;

            for (int i = 0; i < 4; i++)
            {
                TextureSelectOption option = TextureSelectOption.CreateOptions(String.Format("#{0} texture", i + 1), this, ref tempYCursor);
                option.isHidden = true;
                mTextureSelectOptions.Add(option);
            }
            bottomTextureYCursor = yCursor;

            confirmBtn = UIUtils.CreateButton(this);
            confirmBtn.text = "Set";
            confirmBtn.size = new Vector2(60, 30);
            confirmBtn.relativePosition = new Vector3(this.width - confirmBtn.width - m_UIPadding.right, yCursor);
            confirmBtn.eventClicked += ConfirmBtn_eventClicked;
            confirmBtn.tooltip = "Create the label";

            yCursor += confirmBtn.height + m_UIPadding.bottom;

            this.height = yCursor;
        }

        public void SetRoadData()
        {
            List<string> textureReplaceStrings = new List<string>();
            foreach (TextureSelectOption option in mTextureSelectOptions)
            {
                textureReplaceStrings.Add(option.m_textureDropdown.selectedValue);
            }
            RouteManager.instance.SetOverrideSign(netSegmentId, m_propTypeDropDown.selectedValue, textureReplaceStrings);
            EventBusManager.Instance().Publish("closeAll", null);
        }

        private void propTypeDropDown_eventSelectedIndexChanged(UIComponent component, int value)
        {

            foreach (TextureSelectOption option in mTextureSelectOptions)
            {
                option.isHidden = true;

            }
            if (TextureReplaceConfig.texturePropInfoDict.ContainsKey(m_propTypeDropDown.selectedValue))
            {
                TextureReplaceConfig.TextureSignPropInfo info = TextureReplaceConfig.texturePropInfoDict[m_propTypeDropDown.selectedValue];
                for (int i = 0; i < info.numTextures; i++)
                {
                    mTextureSelectOptions[i].isHidden = false;
                    mTextureSelectOptions[i].textureSelectLabel.text = String.Format("{0} texture", info.drawAreaDescriptors[i]);

                    mTextureSelectOptions[i].m_textureDropdown.items = null;
                    mTextureSelectOptions[i].m_textureDropdown.AddItem(RouteManager.NONE);

                    foreach (string key in SpriteUtils.mTextureStore[m_propTypeDropDown.selectedValue].mTextureRefs[(i + 1).ToString()].Keys)
                    {
                        mTextureSelectOptions[i].m_textureDropdown.AddItem(key);
                    }
                    mTextureSelectOptions[i].m_textureDropdown.selectedIndex = 0;

                }

                UIComponent bottomComponent = mTextureSelectOptions[info.numTextures - 1].m_textureDropdown;
                bottomTextureYCursor = bottomComponent.relativePosition.y + bottomComponent.height + m_UIPadding.bottom;

            }
            else
            {
                bottomTextureYCursor = m_propTypeDropDown.relativePosition.y + m_propTypeDropDown.height + m_UIPadding.bottom;
            }
            confirmBtn.relativePosition = new Vector3(this.width - confirmBtn.width - m_UIPadding.right, bottomTextureYCursor);

            this.height = bottomTextureYCursor + confirmBtn.height + m_UIPadding.bottom;
        }

        private void ConfirmBtn_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            SetRoadData();
        }

        private void AddOverrideSignPanel_eventKeyPress(UIComponent component, UIKeyEventParameter eventParam)
        {
            if (eventParam.keycode == KeyCode.KeypadEnter || eventParam.keycode == KeyCode.Return)
            {
                SetRoadData();
            }
        }

        public void onReceiveEvent(string eventName, object eventData)
        {
            string message = eventData as string;
            switch (eventName)
            {
                case "closeAll":
                    Hide();
                    break;
                default:
                    break;
            }
        }
    }
}
