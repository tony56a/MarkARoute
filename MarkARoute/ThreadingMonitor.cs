using ICities;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace MarkARoute
{
    public class ThreadingMonitor : ThreadingExtensionBase
    {

        public override void OnCreated(IThreading threading)
        {
            base.OnCreated(threading);
           
        }

        public override void OnAfterSimulationTick()
        {
            NetSegment[] buffer = NetManager.instance.m_segments.m_buffer;
            List<ushort> segments = new List<ushort>(RouteManager.instance.m_routeDict.Keys);
            foreach (ushort segment in segments)
            {
                if( (buffer[segment].m_flags) == NetSegment.Flags.None)
                {
                    RouteManager.instance.DelRoadRoute(segment);
                }
            }
            buffer = null;
            segments = null;
            base.OnAfterSimulationTick();
        }
    }
}
