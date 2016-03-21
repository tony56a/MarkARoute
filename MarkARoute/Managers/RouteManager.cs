using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static RouteManager Instance()
        {
            if( instance == null)
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

        public void SetRoute(ushort segmentId, string routePrefix, string route, string oldRouteStr = null)
        {
            RouteContainer routeContainer = null;
            if(!String.IsNullOrEmpty(route))
            {
                if(m_routeDict.ContainsKey(segmentId))
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
                if (routeContainer.m_shieldObject == null && routeContainer.m_numTextObject == null)
                {
                    routeContainer.m_shieldObject = new GameObject();
                    routeContainer.m_shieldObject.AddComponent<MeshRenderer>();
                    routeContainer.m_shieldMesh = routeContainer.m_shieldObject.AddComponent<MeshFilter>();
                    routeContainer.m_numTextObject = new GameObject();
                    routeContainer.m_numTextObject.AddComponent<MeshRenderer>();
                    routeContainer.m_numMesh = routeContainer.m_numTextObject.AddComponent<TextMesh>();
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
                GameObject.Destroy(m_routeDict[segmentId].m_numTextObject);
                GameObject.Destroy(m_routeDict[segmentId].m_shieldObject);
                m_routeDict.Remove(segmentId);
            }

            if( oldRouteStr != null)
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
                    if (route.m_shieldObject == null && route.m_numTextObject == null)
                    {
                        route.m_shieldObject = new GameObject();
                        route.m_shieldObject.AddComponent<MeshRenderer>();
                        route.m_shieldMesh = route.m_shieldObject.AddComponent<MeshFilter>();
                        route.m_numTextObject = new GameObject();
                        route.m_numTextObject.AddComponent<MeshRenderer>();
                        route.m_numMesh = route.m_numTextObject.AddComponent<TextMesh>();
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
        public GameObject m_numTextObject;

        [NonSerialized]
        public TextMesh m_numMesh;

        public RouteContainer(ushort segmentId, string routePrefix, string route)
        {
            this.m_segmentId = segmentId;
            this.m_routePrefix = routePrefix;
            this.m_route = route;
        }
    }
}
