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

    class AddOverrideSignPanel : AddSignPanel
    {

        public ushort netSegmentId;

        public override List<String> supportedModes
        {
            get
            {
                return new List<String> { TEXTURE_REPLACE };
            }
        }

        public override void populatePropTypes()
        {
  
            foreach (String signPropName in PropUtils.m_signPropDict.Keys.Where(key => TextureReplaceConfig.texturePropInfoDict.ContainsKey(key)))
            {
                m_propTypeDropDown.AddItem(signPropName);
            }
            m_propTypeDropDown.AddItem(RouteManager.NONE);
            m_propTypeDropDown.AddItem(RouteManager.VANILLA);

        }

        public override void SetRoadData()
        {
            List<string> textureReplaceStrings = new List<string>();
            foreach (TextureSelectOption option in mTextureSelectOptions)
            {
                textureReplaceStrings.Add(option.m_textureDropdown.selectedValue);
            }
            RouteManager.instance.SetOverrideSign(netSegmentId, m_propTypeDropDown.selectedValue, textureReplaceStrings);

            EventBusManager.Instance().Publish("closeAll", null);
        }
    }

}
