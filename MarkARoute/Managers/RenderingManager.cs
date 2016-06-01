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

        private int m_lastCount = 0;
        private bool textHidden = false;

        public float m_renderHeight = 1000f;
        public bool m_alwaysShowText = false;
        public bool m_registered = false;
        public bool m_routeEnabled = true;

        // List of fonts that we want to load for the mod
        public readonly List<string> m_desiredFonts = new List<string> { "Highway Gothic", "Electronic Highway Sign", "Transport" };

        public Dictionary<string, PropInfo> m_signPropDict;
        public Dictionary<string, Font> m_fontDict;

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

        public bool LoadPropMeshes()
        {
            m_fontDict = new Dictionary<string, Font>();
            m_signPropDict = new Dictionary<string, PropInfo>();
            List<string> meshKeys = new List<string>(SignPropConfig.signPropInfoDict.Keys);

            // Bit of a placeholder hack, since we don't support multiple type of VMS models as of yet
            meshKeys.Add("electronic_sign_gantry");

            for (uint i = 0; i < PrefabCollection<PropInfo>.PrefabCount(); ++i)
            {
                for (int j = 0; j < meshKeys.Count; j++)
                {
                    if (PrefabCollection<PropInfo>.GetPrefab(i).name.ToLower().Contains(meshKeys[j]))
                    {
                        m_signPropDict[meshKeys[j]] = PrefabCollection<PropInfo>.GetLoaded(i);
                        meshKeys.RemoveAt(j);
                    }
                }
            }

            foreach (string fontName in m_desiredFonts)
            {
                Font font = Font.CreateDynamicFontFromOSFont(fontName, DistrictManager.instance.m_properties.m_areaNameFont.baseFont.fontSize);
                if (font != null)
                {
                    m_fontDict[fontName] = font;
                }
            }

            return meshKeys.Count == 0;
        }

        public void replaceProp(bool shouldLoad)
        {
            NetCollection[] propCollections = FindObjectsOfType<NetCollection>();
            foreach (NetCollection collection in propCollections)
            {
                foreach (NetInfo prefab in collection.m_prefabs.Where(prefab => prefab.m_lanes != null))
                {
                    foreach (NetInfo.Lane lane in prefab.m_lanes.Where(lane => lane.m_laneProps != null))
                    {
                        foreach (NetLaneProps.Prop prop in lane.m_laneProps.m_props.Where(prop => prop != null)) 
                        {
                            if (prop.m_prop != null && prop.m_prop.name != null && prop.m_prop.name.ToLower().Contains("motorway overroad signs"))
                            {
                                prop.m_probability = shouldLoad ? 100 : 0;
                            }
                        }
                    }

                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            LoggerUtils.Log("Initialising RoadRenderingManager");

            if (!LoadPropMeshes())
            {
                LoggerUtils.LogError("Failed to load some props!");
            }
            else
            {
                LoggerUtils.Log("Props loaded!");

            }

            replaceProp(ModSettings.Instance().settings.Contains("loadMotorwaySigns") ? (bool)ModSettings.Instance().settings["loadMotorwaySigns"] : true);

            //Only start dynamic signs after everything's loaded
            RenderingManager.instance.initTimer();
        }

        protected override void BeginOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            NetManager netManager = NetManager.instance;
            DistrictManager districtManager = DistrictManager.instance;
            cameraInfo.m_height = 100;
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

            if (!textHidden && cameraInfo.m_height > m_renderHeight )
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
            else if (textHidden && cameraInfo.m_height <= m_renderHeight ) //This is a mess, and I'll sort it soon :)
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

            if (m_updateDynamicSignFlag)
            {

                m_updateDynamicSignFlag = false;
                float avg;
                float lowTrafficMsgChance;
                String msgText;
                foreach (DynamicSignContainer sign in RouteManager.Instance().m_dynamicSignList)
                {
                    avg = (float)sign.m_trafficDensity;
                    avg -= sign.m_trafficDensity / 3;
                    avg += netManager.m_segments.m_buffer[sign.m_segment].m_trafficDensity / 3;
                    sign.m_trafficDensity = avg;
                    msgText = (sign.m_route == null ? "Traffic" : (sign.m_routePrefix + '-' + sign.m_route)) +
                    String.Format(" moving {0}", sign.m_trafficDensity > 65 ? "slowly" : "well");

                    if (sign.m_trafficDensity < 35)
                    {
                        lowTrafficMsgChance = 0.8f;
                    }
                    else
                    {
                        lowTrafficMsgChance = 0.1f;
                    }

                    sign.m_messageTextMesh.text = (messageRandom.NextDouble() > lowTrafficMsgChance) ? msgText :
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

                                route.m_numMesh.font = m_fontDict.ContainsKey("Highway Gothic") ? m_fontDict["Highway Gothic"] : districtManager.m_properties.m_areaNameFont.baseFont;
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
                    string signPropType = (sign.m_exitNum == null || !m_signPropDict.ContainsKey(sign.m_exitNum)) ? "hwysign" : sign.m_exitNum;
                    SignPropInfo signPropInfo = SignPropConfig.signPropInfoDict[signPropType];
                    int numSignProps = signPropInfo.isDoubleGantry ? 2 : 1;

                    sign.m_sign.GetComponent<Renderer>().material = m_signPropDict[signPropType].m_material;
                    //TODO: Make mesh size dependent on text size
                    sign.m_sign.mesh = m_signPropDict[signPropType].m_mesh;
                    sign.m_sign.transform.position = position;

                    if (sign.m_routePrefix != null)
                    {
                        RouteShieldInfo shieldInfo = RouteShieldConfig.Instance().GetRouteShieldInfo(sign.m_routePrefix);
                        Material mat = SpriteUtils.m_textureStore[shieldInfo.textureName];
                        sign.m_shieldObject.GetComponent<Renderer>().material = mat;

                        //TODO: Make mesh size dependent on text size
                        sign.m_shieldMesh.mesh = MeshUtils.CreateRectMesh(mat.mainTexture.width, mat.mainTexture.height);
                        sign.m_shieldMesh.transform.position = position;

                        //TODO: Bind the elevation of the mesh to the text z offset
                        sign.m_shieldMesh.transform.position += (Vector3.up * (0.5f));
                        sign.m_shieldMesh.transform.localScale = signPropInfo.shieldScale;
                        sign.m_shieldObject.GetComponent<Renderer>().sortingOrder = 1000;

                        sign.m_numMesh.anchor = TextAnchor.MiddleCenter;
                        sign.m_numMesh.font = m_fontDict.ContainsKey("Highway Gothic") ? m_fontDict["Highway Gothic"] : districtManager.m_properties.m_areaNameFont.baseFont;
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

                        sign.m_shieldMesh.transform.localPosition = signPropInfo.shieldOffset;
                    }


                    string[] destinationStrings = sign.m_destination.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                    for (int i = 0; i < numSignProps; i++)
                    {
                        sign.m_destinationMesh[i].anchor = TextAnchor.MiddleCenter;
                        sign.m_destinationMesh[i].font = m_fontDict.ContainsKey(signPropInfo.fontType) ? m_fontDict[signPropInfo.fontType] : districtManager.m_properties.m_areaNameFont.baseFont;
                        sign.m_destinationMesh[i].font.material.SetColor("Text Color", Color.white);
                        sign.m_destinationMesh[i].font.material.shader = ShaderUtils.m_shaderStore["Font"];

                        sign.m_destinationMesh[i].GetComponent<Renderer>().material = sign.m_destinationMesh[i].font.material;
                        //TODO: Tie the font size to the font size option
                        sign.m_destinationMesh[i].fontSize = 50;
                        sign.m_destinationMesh[i].transform.position = position;
                        sign.m_destinationMesh[i].transform.parent = sign.m_sign.transform;

                        sign.m_destinationMesh[i].transform.position = position;
                        //Just a hack, to make sure the text actually shows up above the shield
                        sign.m_destinationMesh[i].offsetZ = 0.001f;
                        //TODO: Definitely get a map of the texture to the required text offsets 
                        //TODO: Figure out a better ratio for route markers
                        sign.m_destinationMesh[i].transform.localScale = signPropInfo.textScale;
                        sign.m_destinationMesh[i].text = signPropInfo.isDoubleGantry ? destinationStrings[i] : sign.m_destination;

                        sign.m_destinationMesh[i].transform.localPosition = sign.m_routePrefix == null ? signPropInfo.textOffsetNoSign[i] : signPropInfo.textOffsetSign[i];
                    }

                }

                foreach (DynamicSignContainer sign in RouteManager.Instance().m_dynamicSignList)
                {
                    Vector3 position = new Vector3(sign.x, sign.y, sign.z);

                    sign.m_sign.GetComponent<Renderer>().material = m_signPropDict["electronic_sign_gantry"].m_material;
                    //TODO: Make mesh size dependent on text size
                    sign.m_sign.mesh = m_signPropDict["electronic_sign_gantry"].m_mesh;
                    sign.m_sign.transform.position = position;

                    sign.m_messageTextMesh.anchor = TextAnchor.MiddleLeft;
                    sign.m_messageTextMesh.font = m_fontDict.ContainsKey("Electronic Highway Sign") ? m_fontDict["Electronic Highway Sign"] : districtManager.m_properties.m_areaNameFont.baseFont;
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
            if (!m_updateDynamicSignFlag)
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
