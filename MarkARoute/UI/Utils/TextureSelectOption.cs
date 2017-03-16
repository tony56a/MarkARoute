using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.UI.Utils
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
            retVal.m_textureDropdown.autoListWidth = true;
            retVal.m_textureDropdown.relativePosition = new Vector3(UIUtils.UIPadding.left, yPos);

            yPos += (retVal.m_textureDropdown.height + UIUtils.UIPadding.top);
            return retVal;
        }
    }
}
