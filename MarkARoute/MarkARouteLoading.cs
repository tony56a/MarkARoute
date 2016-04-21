using ColossalFramework;
using ColossalFramework.Packaging;
using ColossalFramework.UI;
using ICities;
using MarkARoute.Managers;
using MarkARoute.UI;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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

                MarkARouteOptions.mInGame = true;
                MarkARouteOptions.update();

            }
        }

        public override void OnLevelUnloading()
        {
            // First disable dynamic sign updates
            RenderingManager.instance.disableTimer();
            MarkARouteOptions.mInGame = false;
        }

        /// <summary>
        /// Loads all custom sprites
        /// </summary>
        private void LoadSprites()
        {
            bool spriteSuccess = true;
            //TODO: Replace with a loader function( JSON mapping available )
            RouteShieldConfig.LoadRouteShieldInfo();
            string[] files = Directory.GetFiles(FileUtils.GetModPath()+ "/Icons");
            foreach ( string file in files)
            {
                string[] splitValues = file.Split('\\');
                string fileName = splitValues[splitValues.Length - 1];
                string fileKey = fileName.Split('.')[0];
                spriteSuccess = SpriteUtils.AddTexture(file, fileKey) && spriteSuccess;
                if(!RouteShieldConfig.Instance().routeShieldDictionary.ContainsKey(fileKey))
                {
                    RouteShieldConfig.Instance().routeShieldDictionary[fileKey] = new RouteShieldInfo(fileKey);
                }
            }

            files = Directory.GetFiles(FileUtils.GetModPath() + "/Shaders");
            foreach (string file in files)
            {
                string[] splitValues = file.Split('\\');
                string fileName = splitValues[splitValues.Length - 1];
                string fileKey = fileName.Split('.')[0];
                spriteSuccess = ShaderUtils.AddShader(file, fileKey) && spriteSuccess;
            }

            string fontFile = FileUtils.GetModPath() + "\\test";
            string fontDst = "test";
            if (File.Exists(fontFile))
            {
                File.Copy(fontFile, fontDst, true);

            }

            if (!spriteSuccess)
            {
                LoggerUtils.LogError("Failed to load some sprites!");
            }
            else
            {
                RouteShieldConfig.SaveRouteShieldInfo();
            }
        }
    }
}
