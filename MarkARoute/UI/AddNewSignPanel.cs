using MarkARoute.Managers;
using MarkARoute.Tools;
using MarkARoute.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkARoute.UI
{
    class AddNewSignPanel : AddSignPanel
    {

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
                    mSignPlacementTool.destination = m_destinationField[0].text + '\n' + m_destinationField[1].text;
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
