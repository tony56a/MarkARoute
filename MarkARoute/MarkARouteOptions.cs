using ColossalFramework.UI;
using ICities;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute
{
    public class MarkARouteOptions : MonoBehaviour
    {
        public static bool mInGame = false;
        private static UICheckBox shouldLoadDefaultSign = null;
        private static UIHelperBase shieldBase = null;
        private static UIHelperBase reloadBase = null;

        private static UIDropDown shieldSelector = null;
        private static string shieldKey = "";
        private static RouteShieldInfo shieldInfo;
        private static UISlider mUpOffsetSlider = null;
        private static UISlider mLeftOffsetSlider = null;
        private static UISlider mTextSizeSlider = null;
        private static UIDropDown mTextColor = null;
        private static UIButton mSaveButton = null;

        private static UIButton mReloadButton = null;


        public void generateSettings(UIHelperBase helper)
        {
            shouldLoadDefaultSign = helper.AddCheckbox("Should show game default highway signs", ModSettings.Instance().loadMotorwaySigns, onShouldDefaultSignChecked) as UICheckBox;
            shieldBase = helper.AddGroup("Route Shield Options");
            shieldSelector = shieldBase.AddDropdown("Route Shield", null, 0, onShieldSelected) as UIDropDown;
            mUpOffsetSlider = shieldBase.AddSlider("Text Up offset", -1, 1, 0.1f, 0, onUpOffsetChanged) as UISlider;
            mLeftOffsetSlider = shieldBase.AddSlider("Text Left offset", -1, 1, 0.1f, 0, onLeftOffsetChanged) as UISlider;
            mTextSizeSlider = shieldBase.AddSlider("Text Size", 0.1f, 1, 0.1f, 0, onTextSizeChanged) as UISlider;
            mTextColor = shieldBase.AddDropdown("Text Color", new string[] { "Black", "White" }, 0, onTextColorChanged) as UIDropDown;
            mSaveButton = shieldBase.AddButton("Save", onSaveBtnClicked) as UIButton;
            reloadBase = helper.AddGroup("Sign Texture Options");
            mReloadButton = reloadBase.AddButton("Reload Textures", onReloadBtnClicked) as UIButton;

        }

        private static bool loaded()
        {
            if (RouteShieldConfig.Instance().routeShieldDictionary == null)
            {
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Nope!", "You have to load a game first!", false);
                return false;
            }
            return true;
        }

        public static void update()
        {
            if (loaded())
            {
                MarkARouteOptions.shouldLoadDefaultSign.isChecked = ModSettings.Instance().loadMotorwaySigns;
                shieldKey = RouteShieldConfig.Instance().routeShieldDictionary.Keys.ToList()[0];
                shieldInfo = RouteShieldConfig.Instance().routeShieldDictionary[shieldKey];
                int color = shieldInfo.textColor == Color.black ? 0 : 1;
                MarkARouteOptions.shieldSelector.items = RouteShieldConfig.Instance().routeShieldDictionary.Keys.ToArray();
                MarkARouteOptions.mLeftOffsetSlider.value = shieldInfo.leftOffset;
                MarkARouteOptions.mUpOffsetSlider.value = shieldInfo.upOffset;
                MarkARouteOptions.mTextSizeSlider.value = shieldInfo.textScale;
                MarkARouteOptions.mTextColor.selectedIndex = color;
            }
        }

        private void onShouldDefaultSignChecked(bool isChecked)
        {
            ModSettings.Instance().loadMotorwaySigns = isChecked;
            RenderingManager.instance.replaceProp(isChecked);
        }

        private void onSaveBtnClicked()
        {
            if (loaded())
            {
                RouteShieldConfig.SaveRouteShieldInfo();
                RenderingManager.instance.ForceUpdate(false);
            }

        }


        private void onTextColorChanged(int sel)
        {
            if (loaded())
            {
                RouteShieldConfig.Instance().routeShieldDictionary[shieldKey].textColor = sel == 1 ? Color.white : Color.black;
            }
        }

        private void onTextSizeChanged(float val)
        {
            if (loaded())
            {
                RouteShieldConfig.Instance().routeShieldDictionary[shieldKey].textScale = val;

            }
        }

        private void onLeftOffsetChanged(float val)
        {
            if (loaded())
            {
                RouteShieldConfig.Instance().routeShieldDictionary[shieldKey].leftOffset = val;

            }
        }

        private void onUpOffsetChanged(float val)
        {
            if (loaded())
            {
                RouteShieldConfig.Instance().routeShieldDictionary[shieldKey].upOffset = val;

            }
        }

        private void onShieldSelected(int sel)
        {
            if (loaded())
            {
                shieldKey = RouteShieldConfig.Instance().routeShieldDictionary.Keys.ToList()[sel];
                shieldInfo = RouteShieldConfig.Instance().routeShieldDictionary[shieldKey];
                int color = shieldInfo.textColor == Color.black ? 0 : 1;
                MarkARouteOptions.mLeftOffsetSlider.value = shieldInfo.leftOffset;
                MarkARouteOptions.mUpOffsetSlider.value = shieldInfo.upOffset;
                MarkARouteOptions.mTextSizeSlider.value = shieldInfo.textScale;
                MarkARouteOptions.mTextColor.selectedIndex = color;
            }


        }


        private void onReloadBtnClicked()
        {
            if( loaded())
            {
                SpriteUtils.ExtractAllTextures();
                RenderingManager.instance.ForceUpdate(true);
            }
         
        }

    }

}
