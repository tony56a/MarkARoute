using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace MarkARoute.Managers
{
    class RouteManager
    {
        private static RouteManager instance = null;

        /// <summary>
        /// Dictionary of routes already used, to the number of segments that use that route
        /// </summary> 
        public Dictionary<string, int> m_usedNames = new Dictionary<string, int>();
        public Dictionary<string, int> m_usedRoutes = new Dictionary<string, int>();

        /// <summary>
        /// Dictionary of the segmentId to the route name container
        /// </summary>
        public Dictionary<ushort, RouteContainer> m_routeDict = new Dictionary<ushort, RouteContainer>();

        public List<SignContainer> m_signList = new List<SignContainer>(0);

        public Dictionary<ushort, DynamicSignContainer> m_dynamicSignDict = new Dictionary<ushort, DynamicSignContainer>();


        public static RouteManager Instance()
        {
            if (instance == null)
            {
                instance = new RouteManager();
            }
            return instance;
        }

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

            m_dynamicSignDict.Add(segmentId, signContainer);
        }

        public void SetSign(Vector3 position, float angle, string routePrefix, string route, string destination)
        {
            SignContainer signContainer = new SignContainer(position, angle, routePrefix, route, destination);

            if (routePrefix != null)
            {
                signContainer.m_shieldObject = new GameObject(position + "shield");
                signContainer.m_shieldObject.AddComponent<MeshRenderer>();
                signContainer.m_shieldMesh = signContainer.m_shieldObject.AddComponent<MeshFilter>();
                GameObject numTextObject = new GameObject(position + "text");
                numTextObject.transform.parent = signContainer.m_shieldObject.transform;
                numTextObject.AddComponent<MeshRenderer>();
                numTextObject.GetComponent<MeshRenderer>().sortingOrder = 1001;
                signContainer.m_numMesh = numTextObject.AddComponent<TextMesh>();
            }


            signContainer.m_signObj = new GameObject(position + "sign");
            signContainer.m_signObj.AddComponent<MeshRenderer>();
            signContainer.m_sign = signContainer.m_signObj.AddComponent<MeshFilter>();
            signContainer.m_destinationMeshObject = new GameObject(position + "destText");
            signContainer.m_destinationMesh = signContainer.m_destinationMeshObject.AddComponent<TextMesh>();


            //Todo: move the route info back to the renderingManager( or move the rendering position here?? )
            signContainer.m_sign.transform.position = position;
            signContainer.m_sign.transform.Rotate(0, -1 * Mathf.Rad2Deg * signContainer.angle, 0);

            signContainer.m_destinationMesh.transform.position = position;
            signContainer.m_destinationMesh.transform.parent = signContainer.m_sign.transform;

            signContainer.m_destinationMesh.transform.position = position;
            signContainer.m_destinationMesh.transform.Rotate(0, (-1 * Mathf.Rad2Deg * signContainer.angle) + 270, 0);
            signContainer.m_destinationMesh.transform.localPosition += new Vector3(0.2f, 6f, -4.7f);

            if (routePrefix != null)
            {
                signContainer.m_numMesh.GetComponent<MeshRenderer>().sortingLayerName = signContainer.m_sign.GetComponent<MeshRenderer>().sortingLayerName;
                signContainer.m_numMesh.GetComponent<MeshRenderer>().sortingLayerName = signContainer.m_sign.GetComponent<MeshRenderer>().sortingLayerName;
                signContainer.m_numMesh.GetComponent<MeshRenderer>().sortingLayerID = signContainer.m_sign.GetComponent<MeshRenderer>().sortingLayerID;
                signContainer.m_numMesh.GetComponent<MeshRenderer>().sortingLayerID = signContainer.m_sign.GetComponent<MeshRenderer>().sortingLayerID;


                signContainer.m_shieldMesh.transform.position = position;

                //TODO: Bind the elevation of the mesh to the text z offset
                signContainer.m_shieldMesh.transform.position += (Vector3.up * (0.5f));
                signContainer.m_shieldMesh.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);

                signContainer.m_numMesh.transform.position = position;
                signContainer.m_numMesh.transform.position = signContainer.m_shieldObject.GetComponent<Renderer>().bounds.center;

                signContainer.m_shieldMesh.transform.parent = signContainer.m_sign.transform;
                signContainer.m_shieldMesh.transform.Rotate(0, (-1 * Mathf.Rad2Deg * signContainer.angle) + 270, 0);

                signContainer.m_shieldMesh.transform.localPosition += new Vector3(0.2f, 6.6f, -5.6f);

                signContainer.m_numMesh.transform.parent = signContainer.m_shieldObject.transform;

            }
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

        internal void LoadSigns(SignContainer[] signContainers)
        {
            if (signContainers != null)
            {
                foreach (SignContainer sign in signContainers)
                {
                    SetSign(new Vector3(sign.x, sign.y, sign.z), sign.angle, sign.m_routePrefix, sign.m_route, sign.m_destination);
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
        public GameObject m_destinationMeshObject;
        [NonSerialized]
        public TextMesh m_destinationMesh;

        public SignContainer(Vector3 pos, float angle, string routePrefix, string route, string destination)
        {
            m_routePrefix = routePrefix;
            m_route = route;
            m_destination = destination;
            this.pos = pos;
            x = pos.x;
            y = pos.y;
            z = pos.z;
            this.angle = angle;
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
