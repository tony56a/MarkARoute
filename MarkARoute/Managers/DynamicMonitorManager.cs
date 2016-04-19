using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Managers
{
    class DynamicMonitorManager : MonoBehaviour
    {
        private static DynamicMonitorManager instance;

        public static DynamicMonitorManager Instance()
        {
            if (instance == null)
            {
                instance = new DynamicMonitorManager();
            }
            return instance;
        }


    }
}
