using ColossalFramework.UI;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MarkARoute.Tools;
using MarkARoute.UI.Utils;

namespace MarkARoute.UI
{

    abstract class AddSignPanel : UIPanel,IEventSubscriber
    {
        protected const string OVERLAY = "Overlay";
        protected const string TEXTURE_REPLACE = "Texture replace";

        public abstract List<String> supportedModes
        {
            get;
        }

        private TitleBar m_panelTitle;
        protected UIDropDown m_routeTypeDropdown;
        protected UITextField m_routeStrField;
        private UILabel m_routeLabel;

        protected UITextField[] m_destinationField = new UITextField[2];
        private UIPanel m_colourSelectorPinPanel;
        private UILabel m_signColorLabel;
        private UIColorField m_colourSelector;
        private UILabel m_destinationLabel;

        private UILabel m_propTypeLabel;
        protected UIDropDown m_propTypeDropDown;

        private UILabel m_propRenderingTypeLabel;
        protected UIDropDown m_propRenderingTypeDropdown;
        private UIButton nameRoadButton;

        private float yCursor;
        private float bottomYCursor;
        private float bottomTextureYCursor;

        protected List<TextureSelectOption> mTextureSelectOptions = new List<TextureSelectOption>();

        public StaticSignPlacementTool mSignPlacementTool;

        public abstract void populatePropTypes();

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

        private void CreatePanelComponents()
        {
            yCursor = m_panelTitle.height + UIUtils.UIPadding.top;

            m_propRenderingTypeLabel = this.AddUIComponent<UILabel>();
            m_propRenderingTypeLabel.textScale = 1f;
            m_propRenderingTypeLabel.size = new Vector3(UIUtils.UIPadding.left, m_panelTitle.height + UIUtils.UIPadding.bottom);
            m_propRenderingTypeLabel.textColor = new Color32(180, 180, 180, 255);
            m_propRenderingTypeLabel.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_propRenderingTypeLabel.textAlignment = UIHorizontalAlignment.Left;
            m_propRenderingTypeLabel.text = "Sign Texture method";

            yCursor += m_propRenderingTypeLabel.height + UIUtils.UIPadding.bottom;

            m_propRenderingTypeDropdown = UIUtils.CreateDropDown(this, new Vector2(((this.width - UIUtils.UIPadding.left - 2 * UIUtils.UIPadding.right)), 25));
            foreach (String replacementType in supportedModes)
            {
                m_propRenderingTypeDropdown.AddItem(replacementType);
            }
            m_propRenderingTypeDropdown.selectedIndex = 0;
            m_propRenderingTypeDropdown.autoListWidth = true;
            m_propRenderingTypeDropdown.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_propRenderingTypeDropdown.eventSelectedIndexChanged += M_propTextTypeDropdown_eventSelectedIndexChanged;

            yCursor += m_propRenderingTypeDropdown.height + UIUtils.UIPadding.bottom;

            m_propTypeLabel = this.AddUIComponent<UILabel>();
            m_propTypeLabel.textScale = 1f;
            m_propTypeLabel.size = new Vector3(UIUtils.UIPadding.left, m_panelTitle.height + UIUtils.UIPadding.bottom);
            m_propTypeLabel.textColor = new Color32(180, 180, 180, 255);
            m_propTypeLabel.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_propTypeLabel.textAlignment = UIHorizontalAlignment.Left;
            m_propTypeLabel.text = "Sign prop type";

            yCursor += m_propRenderingTypeDropdown.height + UIUtils.UIPadding.bottom;

            m_propTypeDropDown = UIUtils.CreateDropDown(this, new Vector2(((this.width - UIUtils.UIPadding.left - 2 * UIUtils.UIPadding.right)), 25));
            populatePropTypes();
            m_propTypeDropDown.autoListWidth = true;
            m_propTypeDropDown.selectedIndex = 0;
            m_propTypeDropDown.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_propTypeDropDown.eventSelectedIndexChanged += M_propTypeDropDown_eventSelectedIndexChanged;

            yCursor += m_propTypeDropDown.height + UIUtils.UIPadding.bottom;

            float tempYCursor = yCursor;

            for (int i = 0; i < 4; i++)
            {
                TextureSelectOption option = TextureSelectOption.CreateOptions(String.Format("#{0} texture", i + 1), this, ref tempYCursor);
                option.isHidden = true;
                mTextureSelectOptions.Add(option);
            }

            bottomTextureYCursor = tempYCursor;

            m_routeLabel = this.AddUIComponent<UILabel>();
            m_routeLabel.textScale = 1f;
            m_routeLabel.size = new Vector3(UIUtils.UIPadding.left, m_panelTitle.height + UIUtils.UIPadding.bottom);
            m_routeLabel.textColor = new Color32(180, 180, 180, 255);
            m_routeLabel.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_routeLabel.textAlignment = UIHorizontalAlignment.Left;
            m_routeLabel.text = "Route Name";

            yCursor += m_routeLabel.height + UIUtils.UIPadding.bottom;

            m_routeTypeDropdown = UIUtils.CreateDropDown(this, new Vector2(((this.width - UIUtils.UIPadding.left - 2 * UIUtils.UIPadding.right)), 25));
            foreach (RouteShieldInfo info in RouteShieldConfig.Instance().routeShieldDictionary.Values)
            {
                m_routeTypeDropdown.AddItem(info.textureName);
            }
            m_routeTypeDropdown.selectedIndex = 0;
            m_routeTypeDropdown.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);

            yCursor += m_routeTypeDropdown.height + UIUtils.UIPadding.bottom;

            m_routeStrField = UIUtils.CreateTextField(this);
            m_routeStrField.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_routeStrField.height = 25;
            m_routeStrField.width = (this.width - UIUtils.UIPadding.left - 2 * UIUtils.UIPadding.right);
            m_routeStrField.processMarkup = false;
            m_routeStrField.textColor = Color.white;
            m_routeStrField.maxLength = 3;

            yCursor += m_routeStrField.height + UIUtils.UIPadding.bottom;

            m_destinationLabel = this.AddUIComponent<UILabel>();
            m_destinationLabel.textScale = 1f;
            m_destinationLabel.size = new Vector3(UIUtils.UIPadding.left, m_panelTitle.height + UIUtils.UIPadding.bottom);
            m_destinationLabel.textColor = new Color32(180, 180, 180, 255);
            m_destinationLabel.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_destinationLabel.textAlignment = UIHorizontalAlignment.Left;
            m_destinationLabel.text = "Destination Name";

            yCursor += m_destinationLabel.height + UIUtils.UIPadding.bottom;

            m_destinationField[0] = UIUtils.CreateTextField(this);
            m_destinationField[0].relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_destinationField[0].height = 25;
            m_destinationField[0].width = (this.width - UIUtils.UIPadding.left - 2 * UIUtils.UIPadding.right);
            m_destinationField[0].processMarkup = false;
            m_destinationField[0].textColor = Color.white;

            yCursor += m_destinationField[0].height + UIUtils.UIPadding.bottom;

            m_destinationField[1] = UIUtils.CreateTextField(this);
            m_destinationField[1].relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_destinationField[1].height = 25;
            m_destinationField[1].width = (this.width - UIUtils.UIPadding.left - 2 * UIUtils.UIPadding.right);
            m_destinationField[1].processMarkup = false;
            m_destinationField[1].textColor = Color.white;

            yCursor += m_destinationField[1].height + UIUtils.UIPadding.bottom;

            m_signColorLabel = this.AddUIComponent<UILabel>();
            m_signColorLabel.textScale = 1f;
            m_signColorLabel.size = new Vector3(UIUtils.UIPadding.left, m_panelTitle.height + UIUtils.UIPadding.bottom);
            m_signColorLabel.textColor = new Color32(180, 180, 180, 255);
            m_signColorLabel.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);
            m_signColorLabel.textAlignment = UIHorizontalAlignment.Left;
            m_signColorLabel.text = "Text Color";

            yCursor += m_destinationLabel.height + UIUtils.UIPadding.bottom;

            m_colourSelectorPinPanel = this.AddUIComponent<UIPanel>();
            m_colourSelectorPinPanel.relativePosition = new Vector3(UIUtils.UIPadding.left, yCursor);

            m_colourSelector = UIUtils.CreateColorField(m_colourSelectorPinPanel);
            m_colourSelector.pickerPosition = UIColorField.ColorPickerPosition.LeftBelow;
            m_colourSelector.eventColorChanged += ColourSelector_eventColorChanged;
            m_colourSelector.eventColorPickerClose += ColourSelector_eventColorPickerClose;
            m_colourSelector.tooltip = "Set the text colour";
            m_colourSelector.relativePosition = new Vector3(0, 0);

            bottomYCursor = yCursor;

            nameRoadButton = UIUtils.CreateButton(this);
            nameRoadButton.text = "Set";
            nameRoadButton.size = new Vector2(60, 30);
            nameRoadButton.relativePosition = new Vector3(this.width - nameRoadButton.width - UIUtils.UIPadding.right, yCursor);
            nameRoadButton.eventClicked += NameRoadButton_eventClicked;
            nameRoadButton.tooltip = "Create the label";

            yCursor += nameRoadButton.height + UIUtils.UIPadding.bottom;

            this.height = yCursor;
            SetAddSignType(m_propRenderingTypeDropdown.selectedValue);

        }


        private void ColourSelector_eventColorChanged(UIComponent component, Color32 color)
        {
            foreach (UITextField textField in m_destinationField)
            {
                textField.textColor = color;
            }
        }

        private void ColourSelector_eventColorPickerClose(UIColorField dropdown, UIColorPicker popup, ref bool overridden)
        {
            foreach( UITextField textField in m_destinationField)
            {
                textField.textColor = popup.color;
            }
        }

        public abstract void SetRoadData();

        private void M_propTextTypeDropdown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            SetAddSignType(m_propRenderingTypeDropdown.selectedValue);
        }

        private void SetAddSignType(String value)
        {
            switch (value)
            {
                case TEXTURE_REPLACE:
                    m_routeLabel.Hide();
                    m_routeTypeDropdown.Hide();
                    m_routeStrField.Hide();
                    m_destinationLabel.Hide();
                    m_destinationField[0].Hide();
                    m_destinationField[1].Hide();
                    m_signColorLabel.Hide();
                    m_colourSelectorPinPanel.Hide();
                    m_colourSelector.Hide();

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
                        bottomTextureYCursor = bottomComponent.relativePosition.y + bottomComponent.height + UIUtils.UIPadding.bottom;

                    }
                    else
                    {
                        bottomTextureYCursor = m_routeTypeDropdown.relativePosition.y + m_routeTypeDropdown.height + UIUtils.UIPadding.bottom;
                    }
                    nameRoadButton.relativePosition = new Vector3(this.width - nameRoadButton.width - UIUtils.UIPadding.right, bottomTextureYCursor);

                    this.height = bottomTextureYCursor + nameRoadButton.height + UIUtils.UIPadding.bottom;
                    break;
                case OVERLAY:
                    m_routeLabel.Show();
                    m_routeTypeDropdown.Show();
                    m_routeStrField.Show();
                    m_destinationLabel.Show();
                    m_destinationField[0].Show();
                    m_destinationField[1].Show();
                    m_signColorLabel.Show();
                    m_colourSelectorPinPanel.Show();
                    m_colourSelector.Show();
                    foreach (TextureSelectOption option in mTextureSelectOptions)
                    {
                        option.isHidden = true;
                    }

                    nameRoadButton.relativePosition = new Vector3(this.width - nameRoadButton.width - UIUtils.UIPadding.right, bottomYCursor);
                    this.height = bottomYCursor + nameRoadButton.height + UIUtils.UIPadding.bottom;
                    break;
            }
        }

        private void M_propTypeDropDown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            m_propRenderingTypeDropdown.selectedIndex = 0;
            m_propRenderingTypeDropdown.items = null;
            if (supportedModes.Contains(OVERLAY))
            {
                m_propRenderingTypeDropdown.AddItem(OVERLAY);
            }

            if (TextureReplaceConfig.texturePropInfoDict.ContainsKey(m_propTypeDropDown.selectedValue))
            {
                m_propRenderingTypeDropdown.AddItem(TEXTURE_REPLACE);
            }
            else
            {
                m_propRenderingTypeDropdown.AddItem(RouteManager.NONE);
            }
           
            if(!supportedModes.Contains(OVERLAY))
            {
                SetAddSignType(TEXTURE_REPLACE);
            }
            
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

        private void RoadNamePanel_eventKeyPress(UIComponent component, UIKeyEventParameter eventParam)
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
