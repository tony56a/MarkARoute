using ICities;
using MarkARoute.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkARoute
{
    public class ThreadingMonitor : ThreadingExtensionBase
    {
        private NetManager mNetManager;

        public override void OnCreated(IThreading threading)
        {
            mNetManager = NetManager.instance;
            base.OnCreated(threading);
        }

        public override void OnAfterSimulationTick()
        {
            NetSegment[] buffer = mNetManager.m_segments.m_buffer;
            List<ushort> segments = new List<ushort>(RouteManager.Instance().m_routeDict.Keys);
            foreach (ushort segment in segments)
            {
                if( (buffer[segment].m_flags) == NetSegment.Flags.None)
                {
                    RouteManager.Instance().DelRoadRoute(segment);
                }
            }

            List<DynamicSignContainer> signs = new List<DynamicSignContainer>(RouteManager.Instance().m_dynamicSignDict.Values);
            foreach (DynamicSignContainer sign in signs)
            {
                float avg = (float)sign.m_trafficDensity;
                avg -= sign.m_trafficDensity / 3;
                avg += buffer[sign.m_segment].m_trafficDensity / 3;
                sign.m_trafficDensity = avg;
                String msgText = (sign.m_route == null ? "Traffic" : (sign.m_routePrefix + '-' + sign.m_route)) +
                String.Format( " moving at {0}/100", buffer[sign.m_segment].m_trafficDensity );
                sign.m_messageTextMesh.text = msgText;

            }


            base.OnAfterSimulationTick();
        }
    }
}
