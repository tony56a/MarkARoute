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
        public static readonly float RENDER_HEIGHT = 1000f;

        private int m_lastCount = 0;
        private bool textHidden = false;
        public bool m_alwaysShowText = false;
        public bool m_registered = false;
        public bool m_routeEnabled = true;

        Timer messageUpdateTimer = new Timer();
        public volatile bool m_updateDynamicSignFlag = false;
        System.Random messageRandom = new System.Random();

        Dictionary<string, OverrideSignInfo> ref1 = SignPropConfig.overrideSignValues;
        List<NetLaneProps.Prop> ref2 = PropUtils.findHighwaySignProp();

        public void replaceProp(bool shouldLoad)
        {
            List<NetLaneProps.Prop> props = PropUtils.findHighwaySignProp();
            NetCollection[] propCollections = FindObjectsOfType<NetCollection>();

            foreach( NetLaneProps.Prop prop in props)
            {
                prop.m_probability = shouldLoad ? 100 : 0;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            LoggerUtils.Log("Initialising RoadRenderingManager");
            if (!PropUtils.LoadPropMeshes())
            {
                LoggerUtils.LogError("Failed to load some props!");
            }
            else
            {
                LoggerUtils.Log("Props loaded!");
            }
            replaceProp(ModSettings.Instance().loadMotorwaySigns);
            RouteManager.instance.LoadOverrideTextures();

            DistrictManager districtManager = DistrictManager.instance;
            ShaderUtils.m_shaderStore.Add("fallback", districtManager.m_properties.m_areaNameShader);

            //Only start dynamic signs after everything's loaded
            RenderingManager.instance.initTimer();
        }

        protected override void BeginOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            NetManager netManager = NetManager.instance;
            DistrictManager districtManager = DistrictManager.instance;
            cameraInfo.m_height = 100;
            if (m_lastCount != RouteManager.instance.m_routeDict.Count)
            {
                m_lastCount = RouteManager.instance.m_routeDict.Count;

                try
                {
                    RenderText(false);
                }
                catch (Exception ex)
                {
                    LoggerUtils.LogException(ex);
                }
            }

            if (!textHidden && cameraInfo.m_height > RENDER_HEIGHT )
            {
                foreach (RouteContainer route in RouteManager.instance.m_routeDict.Values)
                {
                    route.m_shieldMesh.GetComponent<Renderer>().enabled = false;
                    route.m_numMesh.GetComponent<Renderer>().enabled = false;
                }

                foreach (SignContainer sign in RouteManager.instance.m_signList)
                {
                    sign.m_sign.GetComponent<Renderer>().enabled = false;
                }
                textHidden = true;
            }
            else if (textHidden && cameraInfo.m_height <= RENDER_HEIGHT ) //This is a mess, and I'll sort it soon :)
            {
                if (m_routeEnabled)
                {
                    foreach (RouteContainer route in RouteManager.instance.m_routeDict.Values)
                    {
                        route.m_shieldMesh.GetComponent<Renderer>().enabled = true;
                        route.m_numMesh.GetComponent<Renderer>().enabled = true;
                    }

                    foreach (SignContainer sign in RouteManager.instance.m_signList)
                    {
                        sign.m_sign.GetComponent<Renderer>().enabled = true;
                    }
                }
                textHidden = false;
            }

            if (m_updateDynamicSignFlag)
            {
                m_updateDynamicSignFlag = false;
                UpdateDynamicSigns(netManager);
            }

        }

        private void UpdateDynamicSigns(NetManager netManager)
        {
            float avg;
            float lowTrafficMsgChance = 0.8f;
            String msgText;
            foreach (DynamicSignContainer sign in RouteManager.instance.m_dynamicSignList)
            {
                avg = (float)sign.m_trafficDensity;
                avg -= sign.m_trafficDensity / 3;
                avg += netManager.m_segments.m_buffer[sign.m_segment].m_trafficDensity / 3;
                sign.m_trafficDensity = avg;
                msgText = (sign.m_route == null ? "Traffic" : (sign.m_routePrefix + '-' + sign.m_route)) +
                String.Format(" moving {0}", sign.m_trafficDensity > 65 ? "slowly" : "well");

                sign.m_messageTextMesh.text = (messageRandom.NextDouble() > lowTrafficMsgChance) ? msgText :
                    DynamicSignConfig.Instance().msgStrings[messageRandom.Next(DynamicSignConfig.Instance().msgStrings.Count)];
            }
        }

        /// <summary>
        /// Redraw the text to be drawn later with a mesh. Use sparingly, as 
        /// this is an expensive task.
        /// </summary>
        private void RenderText(bool forceTextureUpdates)
        {

            DistrictManager districtManager = DistrictManager.instance;
            NetManager netManager = NetManager.instance;

            if (districtManager.m_properties.m_areaNameFont != null)
            {
                UIFontManager.Invalidate(districtManager.m_properties.m_areaNameFont);

                foreach (RouteContainer route in RouteManager.instance.m_routeDict.Values)
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

                                if (!SpriteUtils.mSpriteStore.ContainsKey(shieldInfo.textureName))
                                {
                                    LoggerUtils.Log("WTF, No texture found for route shield" + shieldInfo.textureName);
                                }
                                Material mat = SpriteUtils.mSpriteStore[shieldInfo.textureName];
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

                                route.m_numMesh.font = FontUtils.m_fontStore.ContainsKey("Highway Gothic") ? FontUtils.m_fontStore["Highway Gothic"] : districtManager.m_properties.m_areaNameFont.baseFont;
                                route.m_numMesh.GetComponent<Renderer>().material = route.m_numMesh.font.material;
                                if (ShaderUtils.m_shaderStore.ContainsKey("font"))
                                {
                                    route.m_numMesh.GetComponent<Renderer>().material.shader = ShaderUtils.m_shaderStore["font"];
                                }
                                else
                                {
                                    route.m_numMesh.GetComponent<Renderer>().material.shader = ShaderUtils.m_shaderStore["fallback"];
                                }

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

                DrawSignProps(forceTextureUpdates, districtManager);

                foreach (DynamicSignContainer sign in RouteManager.instance.m_dynamicSignList)
                {
                    Vector3 position = new Vector3(sign.x, sign.y, sign.z);

                    sign.m_sign.GetComponent<Renderer>().material = PropUtils.m_signPropDict["electronic_sign_gantry"].m_material;
                    //TODO: Make mesh size dependent on text size
                    sign.m_sign.mesh = PropUtils.m_signPropDict["electronic_sign_gantry"].m_mesh;
                    sign.m_sign.transform.position = position;

                    sign.m_messageTextMesh.anchor = TextAnchor.MiddleLeft;
                    sign.m_messageTextMesh.font = FontUtils.m_fontStore.ContainsKey("Electronic Highway Sign") ? FontUtils.m_fontStore["Electronic Highway Sign"] : districtManager.m_properties.m_areaNameFont.baseFont;

                    if (ShaderUtils.m_shaderStore.ContainsKey("font"))
                    {
                        sign.m_messageTextMesh.font.material.shader = ShaderUtils.m_shaderStore["font"];
                    }
                    else
                    {
                        sign.m_numMesh.GetComponent<Renderer>().material.shader = ShaderUtils.m_shaderStore["fallback"];
                    }

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

        private void DrawSignProps(bool forceTextureUpdates, DistrictManager districtManager)
        {

            foreach (SignContainer sign in RouteManager.instance.m_signList)
            {
                Vector3 position = new Vector3(sign.x, sign.y, sign.z);
                string signPropType = (sign.m_exitNum == null || !PropUtils.m_signPropDict.ContainsKey(sign.m_exitNum)) ? "hwysign" : sign.m_exitNum;
                SignPropInfo signPropInfo = SignPropConfig.signPropInfoDict[signPropType];
                int numSignProps = signPropInfo.isDoubleGantry ? 2 : 1;

                if (sign.m_signObj != null)
                {
                    if (forceTextureUpdates)
                    {
                        MaybeDrawSignTexture(sign, signPropType);
                    }
                    continue;
                }

                if (sign.m_routePrefix != null)
                {
                    sign.m_shieldObject = new GameObject(position + "shield");
                    sign.m_shieldObject.AddComponent<MeshRenderer>();
                    sign.m_shieldMesh = sign.m_shieldObject.AddComponent<MeshFilter>();
                    GameObject numTextObject = new GameObject(position + "text");
                    numTextObject.transform.parent = sign.m_shieldObject.transform;
                    numTextObject.AddComponent<MeshRenderer>();
                    numTextObject.GetComponent<MeshRenderer>().sortingOrder = 1001;
                    sign.m_numMesh = numTextObject.AddComponent<TextMesh>();
                }

                sign.m_signObj = new GameObject(position + "sign");
                sign.m_signObj.AddComponent<MeshRenderer>();
                sign.m_sign = sign.m_signObj.AddComponent<MeshFilter>();

                sign.m_destinationMeshObject = new GameObject[numSignProps];
                sign.m_destinationMesh = new TextMesh[numSignProps];

                //Todo: move the route info back to the renderingManager( or move the rendering position here?? )
                sign.m_sign.transform.position = position;
                sign.m_sign.transform.Rotate(0, -1 * Mathf.Rad2Deg * sign.angle, 0);

                for (int i = 0; i < numSignProps; i++)
                {
                    if (!string.IsNullOrEmpty(sign.m_destination))
                    {
                        sign.m_destinationMeshObject[i] = new GameObject(position + i.ToString() + "destText");
                        sign.m_destinationMesh[i] = sign.m_destinationMeshObject[i].AddComponent<TextMesh>();

                        sign.m_destinationMesh[i].transform.position = position;
                        sign.m_destinationMesh[i].transform.parent = sign.m_sign.transform;

                        sign.m_destinationMesh[i].transform.position = position;
                        sign.m_destinationMesh[i].transform.Rotate(0, (-1 * Mathf.Rad2Deg * sign.angle) + 270 + signPropInfo.angleOffset, 0);
                    }
                }

                if (sign.m_routePrefix != null)
                {
                    sign.m_shieldMesh.transform.position = position;

                    //TODO: Bind the elevation of the mesh to the text z offset
                    sign.m_shieldMesh.transform.position += (Vector3.up * (0.5f));
                    sign.m_shieldMesh.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);

                    sign.m_numMesh.transform.position = position;
                    sign.m_numMesh.transform.position = sign.m_shieldObject.GetComponent<Renderer>().bounds.center;

                    sign.m_shieldMesh.transform.parent = sign.m_sign.transform;
                    sign.m_shieldMesh.transform.Rotate(0, (-1 * Mathf.Rad2Deg * sign.angle) + 270 + signPropInfo.angleOffset, 0);

                    sign.m_shieldMesh.transform.localPosition += new Vector3(0.2f, 6.6f, -5.6f);

                    sign.m_numMesh.transform.parent = sign.m_shieldObject.transform;

                }

                MaybeDrawSignTexture(sign, signPropType);
                //TODO: Make mesh size dependent on text size
                sign.m_sign.mesh = PropUtils.m_signPropDict[signPropType].m_mesh;
                sign.m_sign.transform.position = position;

                if (sign.useTextureOverride)
                {
                    continue;
                }

                if (sign.m_routePrefix != null)
                {
                    RouteShieldInfo shieldInfo = RouteShieldConfig.Instance().GetRouteShieldInfo(sign.m_routePrefix);
                    Material mat = SpriteUtils.mSpriteStore[shieldInfo.textureName];
                    sign.m_shieldObject.GetComponent<Renderer>().material = mat;

                    //TODO: Make mesh size dependent on text size
                    sign.m_shieldMesh.mesh = MeshUtils.CreateRectMesh(mat.mainTexture.width, mat.mainTexture.height);
                    sign.m_shieldMesh.transform.position = position;

                    //TODO: Bind the elevation of the mesh to the text z offset
                    sign.m_shieldMesh.transform.position += (Vector3.up * (0.5f));
                    sign.m_shieldMesh.transform.localScale = signPropInfo.shieldScale;
                    sign.m_shieldObject.GetComponent<Renderer>().sortingOrder = 1000;

                    sign.m_numMesh.anchor = TextAnchor.MiddleCenter;
                    sign.m_numMesh.font = FontUtils.m_fontStore.ContainsKey("Highway Gothic") ? FontUtils.m_fontStore["Highway Gothic"] : districtManager.m_properties.m_areaNameFont.baseFont;
                    sign.m_numMesh.GetComponent<Renderer>().material = sign.m_numMesh.font.material;
                    if (ShaderUtils.m_shaderStore.ContainsKey("font"))
                    {
                        sign.m_numMesh.GetComponent<Renderer>().material.shader = ShaderUtils.m_shaderStore["font"];
                    }
                    else
                    {
                        sign.m_numMesh.GetComponent<Renderer>().material.shader = ShaderUtils.m_shaderStore["fallback"];
                    }
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

                    sign.m_shieldMesh.transform.localPosition = signPropInfo.shieldOffset[0];
                }


                string[] destinationStrings = sign.m_destination.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < numSignProps; i++)
                {
                    String msgString = signPropInfo.isDoubleGantry ? destinationStrings[i] : sign.m_destination;

                    if (sign.m_destinationMesh[i] != null)
                    {
                        sign.m_destinationMesh[i].anchor = TextAnchor.MiddleCenter;
                        sign.m_destinationMesh[i].font = FontUtils.m_fontStore.ContainsKey(signPropInfo.fontType) ? FontUtils.m_fontStore[signPropInfo.fontType] : districtManager.m_properties.m_areaNameFont.baseFont;
                        sign.m_destinationMesh[i].font.material.SetColor("Text Color", Color.white);

                        if (ShaderUtils.m_shaderStore.ContainsKey("font"))
                        {
                            sign.m_destinationMesh[i].font.material.shader = ShaderUtils.m_shaderStore["font"];
                        }
                        else
                        {
                            sign.m_destinationMesh[i].font.material.shader = ShaderUtils.m_shaderStore["fallback"];
                        }

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
                        sign.m_destinationMesh[i].text = msgString.Replace("\\n", "\n");

                        sign.m_destinationMesh[i].transform.localPosition = sign.m_routePrefix == null ? signPropInfo.textOffsetNoSign[i] : signPropInfo.textOffsetSign[i];
                    }

                }

            }
        }

        private void MaybeDrawSignTexture(SignContainer sign, string signPropType)
        {
            Material material = PropUtils.m_signPropDict[signPropType].m_material;
            if (sign.useTextureOverride)
            {
                material = PropUtils.ReplaceTexture(signPropType, sign.textureOverrides);
            }

            sign.m_sign.GetComponent<Renderer>().material = material;
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
        public void ForceUpdate( bool forceAllUpdates )
        {
            if (forceAllUpdates)
            {
                RenderText(true);
            }else
            {
                m_lastCount = -1;
            }
        }

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

    }
}
