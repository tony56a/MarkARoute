using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using MarkARoute.Managers;
using MarkARoute.UI;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute
{
    public class MarkARouteLoading : LoadingExtensionBase
    {


        private MarkARouteSerializer m_saveUtility = new MarkARouteSerializer();
        private RenderingManager m_renderingManager = null;
        public MainPanel UI { get; set; }

        public override void OnCreated(ILoading loading)
        {
            try //So we don't fuck up loading the city
            {
                LoadSprites();
            }
            catch (Exception ex)
            {
                LoggerUtils.LogException(ex);
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewMap || mode == LoadMode.LoadMap)
            {
                UIView view = UIView.GetAView();
                UITabstrip tabStrip = null;



                UI = ToolsModifierControl.toolController.gameObject.AddComponent<MainPanel>();

                m_renderingManager = RenderingManager.instance;
                m_renderingManager.enabled = true;

                if (m_renderingManager != null && !m_renderingManager.m_registered)
                {
                    RenderManager.RegisterRenderableManager(m_renderingManager);
                    m_renderingManager.m_registered = true;
                }

                /*OptionsManager.m_isIngame = true;
                OptionsManager.UpdateEverything();*/
            }
        }

        public override void OnLevelUnloading()
        {
            //OptionsManager.m_isIngame = false;
        }

        /// <summary>
        /// Loads all custom sprites
        /// </summary>
        private void LoadSprites()
        {
            bool spriteSuccess = true;
            //TODO: Replace with a loader function( JSON mapping available )
            RouteShieldConfig.LoadRouteShieldInfo();
            foreach (KeyValuePair<string, RouteShieldInfo> shieldInfo in RouteShieldConfig.Instance().routeShieldDictionary)
            {
                spriteSuccess = SpriteUtils.AddTexture(shieldInfo.Value.texturePath, shieldInfo.Key) && spriteSuccess;
            }

            if (!spriteSuccess)
            {
                LoggerUtils.LogError("Failed to load some sprites!");
            }
        }
    }
}
