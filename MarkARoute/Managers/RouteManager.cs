using ColossalFramework;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace MarkARoute.Managers
{
    class RouteManager : Singleton<RouteManager>
    {
        public static string NONE = "None";
        /// <summary>
        /// Dictionary of routes already used, to the number of segments that use that route
        /// </summary> 
        public Dictionary<string, int> m_usedRoutes = new Dictionary<string, int>();

        /// <summary>
        /// Dictionary of the segmentId to the route name container
        /// </summary>
        public Dictionary<ushort, RouteContainer> m_routeDict = new Dictionary<ushort, RouteContainer>();

        public List<SignContainer> m_signList = new List<SignContainer>(0);

        public List<DynamicSignContainer> m_dynamicSignList = new List<DynamicSignContainer>();

        private void DecrementRoadRouteCounter(string routeStr)
        {
            if (m_usedRoutes.ContainsKey(routeStr))
            {
                m_usedRoutes[routeStr] -= 1;
                if (m_usedRoutes[routeStr] <= 0)
                {
                    m_usedRoutes.Remove(routeStr);
                }

            }
        }

        public void DelRoadRoute(ushort segmentId)
        {
            if (m_routeDict.ContainsKey(segmentId))
            {
                string routePrefix = m_routeDict[segmentId].m_routePrefix;
                string routeNum = m_routeDict[segmentId].m_route.ToString();
                string routeStr = routePrefix + '/' + routeNum;
                GameObject.Destroy(m_routeDict[segmentId].m_shieldObject);
                m_routeDict.Remove(segmentId);
                DecrementRoadRouteCounter(routeStr);
            }

        }

        public string GetRouteType(ushort segmentId)
        {
            if (RouteExists(segmentId))
            {
                RouteContainer container = m_routeDict[segmentId];
                return container.m_routePrefix;
            }
            else
            {
                return null;
            }
        }

        public string GetRouteStr(ushort segmentId)
        {
            if (RouteExists(segmentId))
            {
                RouteContainer container = m_routeDict[segmentId];
                return container.m_route;
            }
            else
            {
                return null;
            }
        }

        public bool RouteExists(ushort segmentId)
        {
            return m_routeDict.ContainsKey(segmentId);
        }

        public void SetDynamicSign(Vector3 position, float angle, string routePrefix, string route, ushort segmentId)
        {
            DynamicSignContainer signContainer = new DynamicSignContainer(position, angle, routePrefix, route, segmentId);

            signContainer.m_signObj = new GameObject(segmentId + "dynamicsign");
            signContainer.m_signObj.AddComponent<MeshRenderer>();
            signContainer.m_sign = signContainer.m_signObj.AddComponent<MeshFilter>();
            signContainer.m_messageTextMeshObj = new GameObject(position + "dynamicMsgText");
            signContainer.m_messageTextMesh = signContainer.m_messageTextMeshObj.AddComponent<TextMesh>();

            signContainer.m_sign.transform.position = position;
            signContainer.m_sign.transform.Rotate(0, -1 * Mathf.Rad2Deg * signContainer.angle, 0);

            signContainer.m_messageTextMesh.transform.position = position;
            signContainer.m_messageTextMesh.transform.parent = signContainer.m_sign.transform;

            signContainer.m_messageTextMesh.transform.position = position;
            signContainer.m_messageTextMesh.transform.Rotate(0, (-1 * Mathf.Rad2Deg * signContainer.angle) + 270, 0);
            signContainer.m_messageTextMesh.transform.localPosition = new Vector3(0.2f, 6f, -5.7f);

            m_dynamicSignList.Add(signContainer);
        }

        public void SetSign(Vector3 position, float angle, string routePrefix, string route, string destination, string signType)
        {
            SignContainer signContainer = new SignContainer(position, angle, routePrefix, route, destination);
            signContainer.m_exitNum = signType;

            m_signList.Add(signContainer);
            EventBusManager.Instance().Publish("forceUpdateSigns", null);

        }

        internal void SetSign(Vector3 position, float angle, string signType, List<string> textureReplaceStrings)
        {
            SignContainer signContainer = new SignContainer(position, angle, textureReplaceStrings);
            signContainer.m_exitNum = signType;
            m_signList.Add(signContainer);
            EventBusManager.Instance().Publish("forceUpdateSigns", null);
        }


        public void DeleteSign(SignContainer container)
        {
            GameObject.Destroy(container.m_signObj);
            m_signList.Remove(container);
            EventBusManager.Instance().Publish("forceUpdateSigns", null);
        }

        public void SetRoute(ushort segmentId, string routePrefix, string route, string oldRouteStr = null)
        {
            RouteContainer routeContainer = null;
            if (!String.IsNullOrEmpty(route))
            {
                if (m_routeDict.ContainsKey(segmentId))
                {
                    routeContainer = m_routeDict[segmentId];
                    routeContainer.m_routePrefix = routePrefix;
                    routeContainer.m_route = route;
                }
                else
                {
                    routeContainer = new RouteContainer(segmentId, routePrefix, route);
                }

                m_routeDict[segmentId] = routeContainer;

                if (routeContainer.m_shieldObject == null)
                {
                    routeContainer.m_shieldObject = new GameObject(segmentId + "shield");
                    routeContainer.m_shieldObject.AddComponent<MeshRenderer>();
                    routeContainer.m_shieldMesh = routeContainer.m_shieldObject.AddComponent<MeshFilter>();
                    GameObject numTextObject = new GameObject(segmentId + "text");
                    numTextObject.transform.parent = routeContainer.m_shieldObject.transform;
                    numTextObject.AddComponent<MeshRenderer>();
                    numTextObject.GetComponent<MeshRenderer>().sortingOrder = 1001;

                    routeContainer.m_numMesh = numTextObject.AddComponent<TextMesh>();
                }


                string routeStr = routePrefix + '/' + route;
                if (!m_usedRoutes.ContainsKey(routeStr))
                {
                    m_usedRoutes[routeStr] = 0;
                }
                m_usedRoutes[routeStr] += 1;

                if (oldRouteStr != null)
                {
                    DecrementRoadRouteCounter(oldRouteStr);
                }
            }
            else
            {
                GameObject.Destroy(m_routeDict[segmentId].m_shieldObject);
                m_routeDict.Remove(segmentId);
            }

            if (oldRouteStr != null)
            {
                DecrementRoadRouteCounter(oldRouteStr);
            }

            EventBusManager.Instance().Publish("forceupdateroutes", null);
        }

        public RouteContainer[] SaveRoutes()
        {
            return new List<RouteContainer>(m_routeDict.Values).ToArray();
        }

        public void Load(RouteContainer[] routeNames)
        {
            if (routeNames != null)
            {
                foreach (RouteContainer route in routeNames)
                {
                    m_routeDict[route.m_segmentId] = route;
                    if (route.m_shieldObject == null)
                    {
                        route.m_shieldObject = new GameObject(route.m_segmentId + "shield");
                        route.m_shieldObject.AddComponent<MeshRenderer>();
                        route.m_shieldMesh = route.m_shieldObject.AddComponent<MeshFilter>();
                        GameObject numTextObject = new GameObject(route.m_segmentId + "text");
                        numTextObject.transform.parent = route.m_shieldObject.transform;
                        numTextObject.AddComponent<MeshRenderer>();
                        numTextObject.GetComponent<MeshRenderer>().sortingOrder = 1001;
                        route.m_numMesh = numTextObject.AddComponent<TextMesh>();
                    }

                    string routeStr = route.m_routePrefix + '/' + route.m_route;
                    if (!m_usedRoutes.ContainsKey(routeStr))
                    {
                        m_usedRoutes[routeStr] = 0;
                    }
                    m_usedRoutes[routeStr] += 1;
                }
            }
        }

        public void LoadSigns(SignContainer[] signContainers)
        {
            if (signContainers != null)
            {
                foreach (SignContainer sign in signContainers)
                {
                    SetSign(new Vector3(sign.x, sign.y, sign.z), sign.angle, sign.m_routePrefix, sign.m_route, sign.m_destination, sign.m_exitNum);
                }
            }
        }

        public void LoadDynamicSigns(DynamicSignContainer[] dynamicSignContainers)
        {
            if (dynamicSignContainers != null)
            {
                foreach (DynamicSignContainer sign in dynamicSignContainers)
                {
                    SetDynamicSign(new Vector3(sign.x, sign.y, sign.z), sign.angle, sign.m_routePrefix, sign.m_route, sign.m_segment);
                }
            }
        }

    }

    [Serializable]
    public class RouteContainer
    {

        public string m_routePrefix = null;
        public string m_route = "";

        public ushort m_segmentId = 0;

        [NonSerialized]
        public GameObject m_shieldObject;

        [NonSerialized]
        public MeshFilter m_shieldMesh;

        [NonSerialized]
        public TextMesh m_numMesh;

        public RouteContainer(ushort segmentId, string routePrefix, string route)
        {
            this.m_segmentId = segmentId;
            this.m_routePrefix = routePrefix;
            this.m_route = route;
        }
    }

    [Serializable]
    public class SignContainer
    {
        [XmlElement(IsNullable = true)]
        public string m_routePrefix = null;

        [XmlElement(IsNullable = true)]
        public string m_route = null;

        [XmlElement(IsNullable = true)]
        public string m_exitNum = null;

        public string m_destination = "";

        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float angle = 0;

        [XmlElement(IsNullable = true)]
        public int shieldIndex = 0;

        [XmlElement(IsNullable = true)]
        public bool useTextureOverride = false;

        [XmlElement(IsNullable = true)]
        public List<string> textureOverrides = null;

        [XmlElement(IsNullable = true)]
        public string extras = null;

        [NonSerialized]
        public Vector3 pos = new Vector3();

        [NonSerialized]
        public MeshFilter m_sign;

        [NonSerialized]
        public GameObject m_signObj;

        [NonSerialized]
        public GameObject m_shieldObject;

        [NonSerialized]
        public MeshFilter m_shieldMesh;

        [NonSerialized]
        public TextMesh m_numMesh;

        [NonSerialized]
        public GameObject[] m_destinationMeshObject;
        [NonSerialized]
        public TextMesh[] m_destinationMesh;

        public SignContainer(Vector3 pos, float angle, string routePrefix, string route, string destination)
        {
            useTextureOverride = false;
            m_routePrefix = routePrefix;
            m_route = route;
            m_destination = destination;
            this.pos = pos;
            x = pos.x;
            y = pos.y;
            z = pos.z;
            this.angle = angle;
        }

        public SignContainer(Vector3 pos, float angle, List<string> textureOverrideStrings)
        {
            this.pos = pos;
            x = pos.x;
            y = pos.y;
            z = pos.z;
            this.angle = angle;
            useTextureOverride = true;
            textureOverrides = textureOverrideStrings;
        }

        // Stub constructor to get compiler to stop complaining about child objects
        public SignContainer(){}

    }

    [Serializable]
    public class DynamicSignContainer : SignContainer
    {

        public ushort m_segment;

        [NonSerialized]
        public float m_trafficDensity;

        [NonSerialized]
        public GameObject m_messageTextMeshObj;
        [NonSerialized]
        public TextMesh m_messageTextMesh;

        public DynamicSignContainer(Vector3 pos, float angle, string routePrefix, string route, ushort segment)
        {
            m_routePrefix = routePrefix;
            m_route = route;
            m_segment = segment;
            this.pos = pos;
            x = pos.x;
            y = pos.y;
            z = pos.z;
            this.angle = angle;
        }
    }
}
