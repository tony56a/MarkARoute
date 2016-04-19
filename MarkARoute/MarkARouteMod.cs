using ICities;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute
{
    public class MarkARouteMod: IUserMod
    {
        private MarkARouteOptions mOptions = null;

        public string Name
        {
            get
            {
                return "Mark-a-Route";
            }
        }

        public string Description
        {
            get
            {
                return "A mod that lets you label your roads with route markers!";
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            if (mOptions == null)
            {
                mOptions = new GameObject("RoadNamerOptions").AddComponent<MarkARouteOptions>();
            }
            mOptions.generateSettings(helper);
        }
    }
}
