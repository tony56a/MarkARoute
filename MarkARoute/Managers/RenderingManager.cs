using ColossalFramework;
using ColossalFramework.UI;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace MarkARoute.Managers
{
    class RenderingManager : SimulationManagerBase<RenderingManager, DistrictProperties>, IRenderableManager, ISimulationManager
    {
        private Material m_nameMaterial = null;
        private Material m_iconMaterial = null;
        private Font m_customFont = null;
        private Font m_dynamicCustomFont = null;

        private int m_lastCount = 0;
        private bool textHidden = false;

        public float m_renderHeight = 1000f;
        public bool m_alwaysShowText = false;
        public bool m_registered = false;
        public bool m_routeEnabled = true;
        public PropInfo m_signPropInfo = null;
        public PropInfo m_dynamicSignPropInfo = null;

        Timer messageUpdateTimer = new Timer();
        public volatile bool m_updateDynamicSignFlag = false;
        System.Random messageRandom = new System.Random();
        public void initTimer()
        {
            messageUpdateTimer.Interval = 5000;
            messageUpdateTimer.Elapsed += MessageUpdateTimer_Elapsed;
            messageUpdateTimer.AutoReset = true;
            messageUpdateTimer.Enabled = true;
        }

        public void disableTimer()
        {
            messageUpdateTimer.Enabled = false;
            messageUpdateTimer.Stop();
            messageUpdateTimer.Dispose();
          
        }

        protected override void Awake()
        {
            base.Awake();

            LoggerUtils.Log("Initialising RoadRenderingManager");

            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            this.m_nameMaterial = new Material(districtManager.m_properties.m_areaNameShader);
            this.m_nameMaterial.CopyPropertiesFromMaterial(districtManager.m_properties.m_areaNameFont.material);

            this.m_iconMaterial = new Material(districtManager.m_properties.m_areaIconShader);
            this.m_iconMaterial.CopyPropertiesFromMaterial(districtManager.m_properties.m_areaIconAtlas.material);

            m_customFont = Font.CreateDynamicFontFromOSFont("Highway Gothic", districtManager.m_properties.m_areaNameFont.baseFont.fontSize);
            m_dynamicCustomFont = Font.CreateDynamicFontFromOSFont("Electronic Highway Sign", districtManager.m_properties.m_areaNameFont.baseFont.fontSize);

            for (uint i = 0; i < PrefabCollection<PropInfo>.LoadedCount(); ++i)
            {
                if (PrefabCollection<PropInfo>.GetLoaded(i).name.ToLower().Contains("hwysign"))
                {
                    this.m_signPropInfo = PrefabCollection<PropInfo>.GetLoaded(i);
                }
                else if (PrefabCollection<PropInfo>.GetLoaded(i).name.ToLower().Contains("electronic_sign_gantry"))
                {
                    this.m_dynamicSignPropInfo = PrefabCollection<PropInfo>.GetLoaded(i);
                }
            }
            //Only start dynamic signs after everything's loaded
            RenderingManager.instance.initTimer();
        }

        protected override void BeginOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            NetManager netManager = NetManager.instance;
            DistrictManager districtManager = DistrictManager.instance;

            if (m_lastCount != RouteManager.Instance().m_routeDict.Count)
            {
                m_lastCount = RouteManager.Instance().m_routeDict.Count;

                try
                {
                    RenderText();
                }
                catch (Exception ex)
                {
                    LoggerUtils.LogException(ex);
                }
            }

            if (!textHidden && cameraInfo.m_height > m_renderHeight)
            {
                foreach (RouteContainer route in RouteManager.Instance().m_routeDict.Values)
                {
                    route.m_shieldMesh.GetComponent<Renderer>().enabled = false;
                    route.m_numMesh.GetComponent<Renderer>().enabled = false;
                }

                foreach (SignContainer sign in RouteManager.Instance().m_signList)
                {
                    /*sign.m_destinationMesh.GetComponent<Renderer>().enabled = false;
                    sign.m_shieldMesh.GetComponent<Renderer>().enabled = false;
                    sign.m_numMesh.GetComponent<Renderer>().enabled = false;*/
                    sign.m_sign.GetComponent<Renderer>().enabled = false;
                }
                textHidden = true;
            }
            else if (textHidden && cameraInfo.m_height <= m_renderHeight && (districtManager.NamesVisible || m_alwaysShowText)) //This is a mess, and I'll sort it soon :)
            {

                if (m_routeEnabled)
                {
                    foreach (RouteContainer route in RouteManager.Instance().m_routeDict.Values)
                    {
                        route.m_shieldMesh.GetComponent<Renderer>().enabled = true;
                        route.m_numMesh.GetComponent<Renderer>().enabled = true;
                    }

                    foreach (SignContainer sign in RouteManager.Instance().m_signList)
                    {
                        /*sign.m_destinationMesh.GetComponent<Renderer>().enabled = true;
                        sign.m_shieldMesh.GetComponent<Renderer>().enabled = true;
                        sign.m_numMesh.GetComponent<Renderer>().enabled = true;*/
                        sign.m_sign.GetComponent<Renderer>().enabled = true;
                    }
                }
                textHidden = false;
            }

            if(m_updateDynamicSignFlag)
            {
                
                m_updateDynamicSignFlag = false;
                float avg;
                float lowTrafficMsgChance;
                foreach (DynamicSignContainer sign in RouteManager.Instance().m_dynamicSignList)
                {
                    avg = (float)sign.m_trafficDensity;
                    avg -= sign.m_trafficDensity / 3;
                    avg += netManager.m_segments.m_buffer[sign.m_segment].m_trafficDensity / 3;
                    sign.m_trafficDensity = avg;
                    String msgText = (sign.m_route == null ? "Traffic" : (sign.m_routePrefix + '-' + sign.m_route)) +
                    String.Format(" moving {0}", sign.m_trafficDensity > 65 ? "slowly" : "well" );

                    if( sign.m_trafficDensity < 35)
                    {
                        lowTrafficMsgChance = 0.8f;
                    }
                    else
                    {
                        lowTrafficMsgChance = 0.1f;
                    }

                    sign.m_messageTextMesh.text = ( messageRandom.NextDouble() > lowTrafficMsgChance ) ? msgText : 
                        DynamicSignConfig.Instance().msgStrings[messageRandom.Next(DynamicSignConfig.Instance().msgStrings.Count)];
                }
            }
         
        }

        /// <summary>
        /// Redraw the text to be drawn later with a mesh. Use sparingly, as 
        /// this is an expensive task.
        /// </summary>
        private void RenderText()
        {

            DistrictManager districtManager = DistrictManager.instance;
            NetManager netManager = NetManager.instance;

            if (districtManager.m_properties.m_areaNameFont != null)
            {
                UIFontManager.Invalidate(districtManager.m_properties.m_areaNameFont);

                foreach (RouteContainer route in RouteManager.Instance().m_routeDict.Values)
                {


                    if (route.m_segmentId != 0)
                    {
                        string routeStr = route.m_route;

                        if (routeStr != null)
                        {
                            NetSegment netSegment = netManager.m_segments.m_buffer[route.m_segmentId];
                            NetSegment.Flags segmentFlags = netSegment.m_flags;

                            if (segmentFlags.IsFlagSet(NetSegment.Flags.Created))
                            {
                                //Load a route shield type ( generic motorway shield should be default value )
                                RouteShieldInfo shieldInfo = RouteShieldConfig.Instance().GetRouteShieldInfo(route.m_routePrefix);

                                NetNode startNode = netManager.m_nodes.m_buffer[netSegment.m_startNode];
                                NetNode endNode = netManager.m_nodes.m_buffer[netSegment.m_endNode];
                                Vector3 startNodePosition = startNode.m_position;

                                Material mat = SpriteUtils.m_textureStore[shieldInfo.textureName];
                                route.m_shieldObject.GetComponent<Renderer>().material = mat;

                                //TODO: Make mesh size dependent on text size
                                route.m_shieldMesh.mesh = MeshUtils.CreateRectMesh(mat.mainTexture.width, mat.mainTexture.height);
                                route.m_shieldMesh.transform.position = startNodePosition;

                                route.m_shieldMesh.transform.LookAt(endNode.m_position, Vector3.up);
                                route.m_shieldMesh.transform.Rotate(90f, 0f, 90f);

                                //TODO: Bind the elevation of the mesh to the text z offset
                                route.m_shieldMesh.transform.position += (Vector3.up * (0.5f));
                                route.m_shieldMesh.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                route.m_shieldObject.GetComponent<Renderer>().sortingOrder = 1000;

                                route.m_numMesh.anchor = TextAnchor.MiddleCenter;
                                route.m_numMesh.font = m_customFont == null ? districtManager.m_properties.m_areaNameFont.baseFont : m_customFont;
                                route.m_numMesh.GetComponent<Renderer>().material = route.m_numMesh.font.material;
                                //TODO: Tie the font size to the font size option
                                route.m_numMesh.fontSize = 50;
                                route.m_numMesh.transform.position = startNode.m_position;
                                route.m_numMesh.transform.parent = route.m_shieldObject.transform;

                                route.m_numMesh.transform.LookAt(endNode.m_position, Vector3.up);
                                route.m_numMesh.transform.Rotate(90f, 0f, 90f);

                                route.m_numMesh.transform.position = route.m_shieldObject.GetComponent<Renderer>().bounds.center;
                                //Just a hack, to make sure the text actually shows up above the shield
                                route.m_numMesh.offsetZ = 0.001f;
                                //TODO: Definitely get a map of the texture to the required text offsets 
                                route.m_numMesh.transform.localPosition += (Vector3.up * shieldInfo.upOffset);
                                route.m_numMesh.transform.localPosition += (Vector3.left * shieldInfo.leftOffset);
                                //TODO: Figure out a better ratio for route markers
                                route.m_numMesh.transform.localScale = new Vector3(shieldInfo.textScale, shieldInfo.textScale, shieldInfo.textScale);
                                route.m_numMesh.GetComponent<Renderer>().material.color = shieldInfo.textColor;
                                route.m_numMesh.text = route.m_route.ToString();

                            }
                        }
                    }
                }

                foreach (SignContainer sign in RouteManager.Instance().m_signList)
                {

                    Vector3 position = new Vector3(sign.x, sign.y, sign.z);

                    sign.m_sign.GetComponent<Renderer>().material = this.m_signPropInfo.m_material;
                    //TODO: Make mesh size dependent on text size
                    sign.m_sign.mesh = this.m_signPropInfo.m_mesh;
                    sign.m_sign.transform.position = position;

                    if ( sign.m_routePrefix != null)
                    {
                        RouteShieldInfo shieldInfo = RouteShieldConfig.Instance().GetRouteShieldInfo(sign.m_routePrefix);
                        Material mat = SpriteUtils.m_textureStore[shieldInfo.textureName];
                        sign.m_shieldObject.GetComponent<Renderer>().material = mat;

                        //TODO: Make mesh size dependent on text size
                        sign.m_shieldMesh.mesh = MeshUtils.CreateRectMesh(mat.mainTexture.width, mat.mainTexture.height);
                        sign.m_shieldMesh.transform.position = position;

                        //TODO: Bind the elevation of the mesh to the text z offset
                        sign.m_shieldMesh.transform.position += (Vector3.up * (0.5f));
                        sign.m_shieldMesh.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                        sign.m_shieldObject.GetComponent<Renderer>().sortingOrder = 1000;

                        sign.m_numMesh.anchor = TextAnchor.MiddleCenter;
                        sign.m_numMesh.font = m_customFont == null ? districtManager.m_properties.m_areaNameFont.baseFont : m_customFont;
                        sign.m_numMesh.GetComponent<Renderer>().material = sign.m_numMesh.font.material;

                        //TODO: Tie the font size to the font size option
                        sign.m_numMesh.fontSize = 50;
                        sign.m_numMesh.transform.position = position;
                        sign.m_numMesh.transform.parent = sign.m_shieldObject.transform;

                        sign.m_numMesh.transform.position = sign.m_shieldObject.GetComponent<Renderer>().bounds.center;
                        //Just a hack, to make sure the text actually shows up above the shield
                        sign.m_numMesh.offsetZ = 0.01f;
                        //TODO: Definitely get a map of the texture to the required text offsets ds
                        sign.m_numMesh.transform.localPosition += (Vector3.up * shieldInfo.upOffset);
                        sign.m_numMesh.transform.localPosition += (Vector3.left * shieldInfo.leftOffset);
                        //TODO: Figure out a better ratio for route markers
                        sign.m_numMesh.transform.localScale = new Vector3(shieldInfo.textScale, shieldInfo.textScale, shieldInfo.textScale);
                        sign.m_numMesh.GetComponent<Renderer>().material.color = shieldInfo.textColor;
                        sign.m_numMesh.text = sign.m_route.ToString();

                        sign.m_shieldMesh.transform.parent = sign.m_sign.transform;

                        sign.m_shieldMesh.transform.localPosition = new Vector3(0.2f, 7.2f, -3.8f);
                    }
                   
                    sign.m_destinationMesh.anchor = TextAnchor.MiddleCenter;
                    sign.m_destinationMesh.font = m_customFont == null ? districtManager.m_properties.m_areaNameFont.baseFont : m_customFont;
                    sign.m_destinationMesh.font.material.SetColor("Text Color", Color.white);
                    sign.m_destinationMesh.font.material.shader = ShaderUtils.m_shaderStore["Font"];

                    sign.m_destinationMesh.GetComponent<Renderer>().material = sign.m_destinationMesh.font.material;
                    //TODO: Tie the font size to the font size option
                    sign.m_destinationMesh.fontSize = 50;
                    sign.m_destinationMesh.transform.position = position;
                    sign.m_destinationMesh.transform.parent = sign.m_sign.transform;

                    sign.m_destinationMesh.transform.position = position;
                    //Just a hack, to make sure the text actually shows up above the shield
                    sign.m_destinationMesh.offsetZ = 0.001f;
                    //TODO: Definitely get a map of the texture to the required text offsets 
                    //TODO: Figure out a better ratio for route markers
                    sign.m_destinationMesh.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    sign.m_destinationMesh.text = sign.m_destination;

                    sign.m_destinationMesh.transform.localPosition = sign.m_routePrefix == null ? new Vector3(0.2f, 6.5f, -4.7f) : new Vector3(0.2f, 5.8f, -4.7f);
                    
                }

                foreach( DynamicSignContainer sign in RouteManager.Instance().m_dynamicSignList) {
                    Vector3 position = new Vector3(sign.x, sign.y, sign.z);

                    sign.m_sign.GetComponent<Renderer>().material = this.m_dynamicSignPropInfo.m_material;
                    //TODO: Make mesh size dependent on text size
                    sign.m_sign.mesh = this.m_dynamicSignPropInfo.m_mesh;
                    sign.m_sign.transform.position = position;

                    sign.m_messageTextMesh.anchor = TextAnchor.MiddleLeft;
                    sign.m_messageTextMesh.font = m_dynamicCustomFont == null ? districtManager.m_properties.m_areaNameFont.baseFont : m_dynamicCustomFont;
                    sign.m_messageTextMesh.font.material.shader = ShaderUtils.m_shaderStore["Font"];
                    sign.m_messageTextMesh.color = (new Color(1, 0.77f, 0.56f, 1f));
                    sign.m_messageTextMesh.font.material.SetColor("Text Color", new Color(1, 0.77f, 0.56f, 1f));

                    sign.m_messageTextMesh.GetComponent<Renderer>().material = sign.m_messageTextMesh.font.material;
                    //TODO: Tie the font size to the font size option
                    sign.m_messageTextMesh.fontSize = 50;
                    sign.m_messageTextMesh.transform.position = position;
                    sign.m_messageTextMesh.transform.parent = sign.m_sign.transform;

                    sign.m_messageTextMesh.transform.position = position;
                    //Just a hack, to make sure the text actually shows up above the shield
                    sign.m_messageTextMesh.offsetZ = 0.001f;
                    //TODO: Definitely get a map of the texture to the required text odffsets 
                    //TODO: Figure out a better ratio for route markers
                    sign.m_messageTextMesh.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

                    String msgText = (sign.m_route == null ? "Traffic" : (sign.m_routePrefix + '-' + sign.m_route)) +
                                     " moving smoothly";
                    sign.m_messageTextMesh.text = msgText;

                    sign.m_messageTextMesh.transform.localPosition = new Vector3(0.7f, 8.4f, -19.7f);
                }

            }
        }

        private void MessageUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(!m_updateDynamicSignFlag)
            {
                m_updateDynamicSignFlag = true;
            }
        }

        /// <summary>
        /// Forces rendering to update immediately. Use sparingly, as it
        /// can be quite expensive.
        /// </summary>
        public void ForceUpdate()
        {
            m_lastCount = -1;
        }
    }
}
