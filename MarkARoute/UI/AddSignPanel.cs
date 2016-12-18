using ColossalFramework.UI;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MarkARoute.Tools;

namespace MarkARoute.UI
{
    class TextureSelectOption
    {
        public UILabel textureSelectLabel;
        public UIDropDown m_textureDropdown;

        public bool isHidden
        {
            get
            {
                return this.m_textureDropdown.isVisible;
            }
            set
            {
                this.textureSelectLabel.isVisible = !value;
                this.m_textureDropdown.isVisible = !value;
            
            }
        }

        public static TextureSelectOption CreateOptions(string labelStr, UIComponent parent, ref float yPos)
        {
            TextureSelectOption retVal = new TextureSelectOption();

            retVal.textureSelectLabel = parent.AddUIComponent<UILabel>();
            retVal.textureSelectLabel.text = labelStr;
            retVal.textureSelectLabel.autoSize = true;
            retVal.textureSelectLabel.size = new Vector2(parent.width - UIUtils.UIPadding.left - UIUtils.UIPadding.right, 25);
            retVal.textureSelectLabel.padding = UIUtils.UIPadding;
            retVal.textureSelectLabel.relativePosition = new Vector2(UIUtils.UIPadding.left, yPos);
            retVal.textureSelectLabel.textAlignment = UIHorizontalAlignment.Left;
            retVal.textureSelectLabel.verticalAlignment = UIVerticalAlignment.Middle;

            yPos += (retVal.textureSelectLabel.height + UIUtils.UIPadding.top);

            retVal.m_textureDropdown = UIUtils.CreateDropDown(parent, new Vector2(((parent.width - UIUtils.UIPadding.left - 2 * UIUtils.UIPadding.right)), 25));
            retVal.m_textureDropdown.selectedIndex = 0;
            retVal.m_textureDropdown.autoSize = true;
            retVal.m_textureDropdown.relativePosition = new Vector3(UIUtils.UIPadding.left, yPos);

            yPos += (retVal.m_textureDropdown.height + UIUtils.UIPadding.top);
            return retVal;
        }
    }

    class AddSignPanel : UIPanel,IEventSubscriber
    {
        private const string OVERLAY = "Overlay";
        private const string TEXTURE_REPLACE = "Texture replace";

        protected RectOffset m_UIPadding = new RectOffset(5, 5, 5, 5);

        private TitleBar m_panelTitle;
        private UIDropDown m_routeTypeDropdown;
        private UITextField m_routeStrField;
        private UILabel m_routeLabel;

        private UITextField[] m_destinationField = new UITextField[2];
        private UILabel m_destinationLabel;

        private UILabel m_propTypeLabel;
        private UIDropDown m_propTypeDropDown;

        private UILabel m_propRenderingTypeLabe;
        private UIDropDown m_propRenderingTypeDropdonw;
        private UIButton nameRoadButton;

        private float yCursor;
        private float bottomYCursor;
        private float bottomTextureYCursor;

        private List<TextureSelectOption> mTextureSelectOptions = new List<TextureSelectOption>();

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

        private void CreatePanelComponents()
        {
            yCursor = m_panelTitle.height + m_UIPadding.top;

            m_propRenderingTypeLabe = this.AddUIComponent<UILabel>();
            m_propRenderingTypeLabe.textScale = 1f;
            m_propRenderingTypeLabe.size = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_propRenderingTypeLabe.textColor = new Color32(180, 180, 180, 255);
            m_propRenderingTypeLabe.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_propRenderingTypeLabe.textAlignment = UIHorizontalAlignment.Left;
            m_propRenderingTypeLabe.text = "Sign Texture method";

            yCursor += m_propRenderingTypeLabe.height + m_UIPadding.bottom;

            m_propRenderingTypeDropdonw = UIUtils.CreateDropDown(this, new Vector2(((this.width - m_UIPadding.left - 2 * m_UIPadding.right)), 25));
            foreach (String replacementType in new List<String> { OVERLAY,TEXTURE_REPLACE } )
            {
                m_propRenderingTypeDropdonw.AddItem(replacementType);
            }
            m_propRenderingTypeDropdonw.selectedIndex = 0;
            m_propRenderingTypeDropdonw.autoListWidth = true;
            m_propRenderingTypeDropdonw.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_propRenderingTypeDropdonw.eventSelectedIndexChanged += M_propTextTypeDropdown_eventSelectedIndexChanged;

            yCursor += m_propRenderingTypeDropdonw.height + m_UIPadding.bottom;

            m_propTypeLabel = this.AddUIComponent<UILabel>();
            m_propTypeLabel.textScale = 1f;
            m_propTypeLabel.size = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_propTypeLabel.textColor = new Color32(180, 180, 180, 255);
            m_propTypeLabel.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_propTypeLabel.textAlignment = UIHorizontalAlignment.Left;
            m_propTypeLabel.text = "Sign prop type";

            yCursor += m_propRenderingTypeDropdonw.height + m_UIPadding.bottom;
            
            m_propTypeDropDown = UIUtils.CreateDropDown(this, new Vector2(((this.width - m_UIPadding.left - 2 * m_UIPadding.right)), 25));
            //TODO: Replace with Random namer values
            var keys = RenderingManager.instance.m_signPropDict.Keys;
            foreach (String signPropName in RenderingManager.instance.m_signPropDict.Keys.Where(key => SignPropConfig.signPropInfoDict.ContainsKey(key)))
            {
                m_propTypeDropDown.AddItem(signPropName);
            }
            m_propTypeDropDown.autoListWidth = true;
            m_propTypeDropDown.selectedIndex = 0;
            m_propTypeDropDown.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_propTypeDropDown.eventSelectedIndexChanged += M_propTypeDropDown_eventSelectedIndexChanged;

            yCursor += m_propTypeDropDown.height + m_UIPadding.bottom;

            float tempYCursor = yCursor;

            for(int i = 0; i < 4; i++)
            {
                TextureSelectOption option = TextureSelectOption.CreateOptions(String.Format("#{0} texture", i + 1), this, ref tempYCursor);
                option.isHidden = true;
                mTextureSelectOptions.Add(option);
            }

            bottomTextureYCursor = tempYCursor;

            m_routeLabel = this.AddUIComponent<UILabel>();
            m_routeLabel.textScale = 1f;
            m_routeLabel.size = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_routeLabel.textColor = new Color32(180, 180, 180, 255);
            m_routeLabel.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_routeLabel.textAlignment = UIHorizontalAlignment.Left;
            m_routeLabel.text = "Route Name";

            yCursor += m_routeLabel.height + m_UIPadding.bottom;

            m_routeTypeDropdown = UIUtils.CreateDropDown(this, new Vector2(((this.width - m_UIPadding.left - 2 * m_UIPadding.right)), 25));
            foreach (RouteShieldInfo info in RouteShieldConfig.Instance().routeShieldDictionary.Values)
            {
                m_routeTypeDropdown.AddItem(info.textureName);
            }
            m_routeTypeDropdown.selectedIndex = 0;
            m_routeTypeDropdown.relativePosition = new Vector3(m_UIPadding.left, yCursor);

            yCursor += m_routeTypeDropdown.height + m_UIPadding.bottom;

            m_routeStrField = UIUtils.CreateTextField(this);
            m_routeStrField.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_routeStrField.height = 25;
            m_routeStrField.width = (this.width - m_UIPadding.left - 2 * m_UIPadding.right);
            m_routeStrField.processMarkup = false;
            m_routeStrField.textColor = Color.white;
            m_routeStrField.maxLength = 3;

            yCursor += m_routeStrField.height + m_UIPadding.bottom;

            m_destinationLabel = this.AddUIComponent<UILabel>();
            m_destinationLabel.textScale = 1f;
            m_destinationLabel.size = new Vector3(m_UIPadding.left, m_panelTitle.height + m_UIPadding.bottom);
            m_destinationLabel.textColor = new Color32(180, 180, 180, 255);
            m_destinationLabel.relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_destinationLabel.textAlignment = UIHorizontalAlignment.Left;
            m_destinationLabel.text = "Destination Name";

            yCursor += m_destinationLabel.height + m_UIPadding.bottom;

            m_destinationField[0] = UIUtils.CreateTextField(this);
            m_destinationField[0].relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_destinationField[0].height = 25;
            m_destinationField[0].width = (this.width - m_UIPadding.left - 2 * m_UIPadding.right);
            m_destinationField[0].processMarkup = false;
            m_destinationField[0].textColor = Color.white;

            yCursor += m_destinationField[0].height + m_UIPadding.bottom;

            m_destinationField[1] = UIUtils.CreateTextField(this);
            m_destinationField[1].relativePosition = new Vector3(m_UIPadding.left, yCursor);
            m_destinationField[1].height = 25;
            m_destinationField[1].width = (this.width - m_UIPadding.left - 2 * m_UIPadding.right);
            m_destinationField[1].processMarkup = false;
            m_destinationField[1].textColor = Color.white;

            yCursor += m_destinationField[1].height + m_UIPadding.bottom;
            bottomYCursor = yCursor;

            nameRoadButton = UIUtils.CreateButton(this);
            nameRoadButton.text = "Set";
            nameRoadButton.size = new Vector2(60, 30);
            nameRoadButton.relativePosition = new Vector3(this.width - nameRoadButton.width - m_UIPadding.right,yCursor);
            nameRoadButton.eventClicked += NameRoadButton_eventClicked;
            nameRoadButton.tooltip = "Create the label";

            yCursor += nameRoadButton.height + m_UIPadding.bottom;

            this.height = yCursor;
        }

        /// <summary>
        /// Gets the colour from the panel and sets it to be rendered/saved
        /// </summary>
        private void SetRoadData()
        {
            switch (m_propRenderingTypeDropdonw.selectedValue)
            {
                case OVERLAY:
                    mSignPlacementTool.useTextureReplace = false;
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
                    mSignPlacementTool.destination = m_destinationField[0].text + '\n' + m_destinationField[1].text;
                    mSignPlacementTool.SetPropInfo(m_propTypeDropDown.selectedValue);
                    break;
                case TEXTURE_REPLACE:
                    mSignPlacementTool.useTextureReplace = true;
                    List<string> textureReplaceStrings = new List<string>();
                    foreach(TextureSelectOption option in mTextureSelectOptions)
                    {
                        textureReplaceStrings.Add(option.m_textureDropdown.selectedValue);
                    }
                    mSignPlacementTool.textureReplaceStrings = textureReplaceStrings;
                    mSignPlacementTool.SetPropInfo(m_propTypeDropDown.selectedValue);
                    break;
            }
          
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

        private void M_propTextTypeDropdown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            switch (m_propRenderingTypeDropdonw.selectedValue)
            {
                case TEXTURE_REPLACE:
                    m_routeLabel.Hide();
                    m_routeTypeDropdown.Hide();
                    m_routeStrField.Hide();
                    m_destinationLabel.Hide();
                    m_destinationField[0].Hide();
                    m_destinationField[1].Hide();

                    foreach (TextureSelectOption option in mTextureSelectOptions)
                    {
                        option.isHidden = true;

                    }
                    if (SignPropConfig.texturePropInfoDict.ContainsKey(m_propTypeDropDown.selectedValue)){
                        TextureSignPropInfo info = SignPropConfig.texturePropInfoDict[m_propTypeDropDown.selectedValue];
                        for (int i = 0; i <info.numTextures; i++)
                        {
                            mTextureSelectOptions[i].isHidden = false;
                            mTextureSelectOptions[i].textureSelectLabel.text = String.Format("{0} texture", info.drawAreaDescriptors[i]);

                            mTextureSelectOptions[i].m_textureDropdown.items = null;
                            mTextureSelectOptions[i].m_textureDropdown.AddItem(RouteManager.NONE);

                            foreach ( string key in SpriteUtils.mTextureStore[m_propTypeDropDown.selectedValue].mTextureRefs[(i + 1).ToString()].Keys)
                            {
                                mTextureSelectOptions[i].m_textureDropdown.AddItem(key);
                            }
                            mTextureSelectOptions[i].m_textureDropdown.selectedIndex = 0;

                        }

                        UIComponent bottomComponent = mTextureSelectOptions[info.numTextures - 1].m_textureDropdown;
                        bottomTextureYCursor = bottomComponent.relativePosition.y + bottomComponent.height + m_UIPadding.bottom;

                    }
                    nameRoadButton.relativePosition = new Vector3(this.width - nameRoadButton.width - m_UIPadding.right, bottomTextureYCursor);
                    
                    this.height = bottomTextureYCursor + nameRoadButton.height + m_UIPadding.bottom;
                    break;
                case OVERLAY:
                    m_routeLabel.Show();
                    m_routeTypeDropdown.Show();
                    m_routeStrField.Show();
                    m_destinationLabel.Show();
                    m_destinationField[0].Show();
                    m_destinationField[1].Show();
                    foreach (TextureSelectOption option in mTextureSelectOptions)
                    {
                        option.isHidden = true;
                    }

                    nameRoadButton.relativePosition = new Vector3(this.width - nameRoadButton.width - m_UIPadding.right, bottomYCursor);
                    this.height = bottomYCursor + nameRoadButton.height + m_UIPadding.bottom;
                    break;
            }
        }


        private void M_propTypeDropDown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            m_propRenderingTypeDropdonw.selectedIndex = 0;
            m_propRenderingTypeDropdonw.items = null;
            m_propRenderingTypeDropdonw.AddItem("Overlay");
            if (SignPropConfig.texturePropInfoDict.ContainsKey(m_propTypeDropDown.selectedValue))
            {
                m_propRenderingTypeDropdonw.AddItem("Texture replace");
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
    }
}
