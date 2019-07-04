using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Managers
{

    class PropPosUtils
    {

        public static int calcGrid(Vector3 position)
        {
            int computedX = (int)Mathf.Clamp(((position.x - 8.0f) / 64.0f + 135.0f), 0f, 269f);
            int computedZ = (int)Mathf.Clamp(((position.z - 8.0f) / 64.0f + 135.0f), 0f, 269f);

            return computedZ * 270 + computedX;
        }
    }

    class AltPropManager : Singleton<AltPropManager>
    {

        /// <summary>
        /// Dictionary of a "grid" of values on the map that correspond to the 
        /// </summary> 
        private Dictionary<int, List<PropInstance>> props = new Dictionary<int, List<PropInstance>>();

        public Dictionary<int, List<PropInstance>> Props { get => props; }

        public void SetProp(Vector3 position, PropInstance instance)
        {
            int gridVal = PropPosUtils.calcGrid(position);
            if( !Props.ContainsKey(gridVal))
            {
                Props[gridVal] = new List<PropInstance>();
            }
            Props[gridVal].Add(instance);
        }
    }
}
