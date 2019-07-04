using MarkARoute.Managers;
using MarkARoute.Tools;
using MarkARoute.UI.Utils;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkARoute.UI
{
    class AddNewSignPanel : AddSignPanel
    {
        public override List<String> supportedModes
        {
            get {
                return new List<String> { OVERLAY, TEXTURE_REPLACE };
            }
        }

        public override void populatePropTypes()
        {
            foreach (String signPropName in PropUtils.m_signPropDict.Keys.Where(key => SignPropConfig.signPropInfoDict.ContainsKey(key)))
            {
                m_propTypeDropDown.AddItem(signPropName);
            }
        }


        public override void SetRoadData()
        {
            switch (m_propRenderingTypeDropdown.selectedValue)
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

                    String destString = m_destinationField[0].text + '\n' + m_destinationField[1].text;

                    mSignPlacementTool.destination = destString;
                    mSignPlacementTool.color = m_destinationField[0].textColor;
                    mSignPlacementTool.color.a = 1f;
                    mSignPlacementTool.SetPropInfo(m_propTypeDropDown.selectedValue);
                    break;
                case TEXTURE_REPLACE:
                    mSignPlacementTool.useTextureReplace = true;
                    List<string> textureReplaceStrings = new List<string>();
                    foreach (TextureSelectOption option in mTextureSelectOptions)
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
    }
}
