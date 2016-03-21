using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkARoute
{
    public class MarkARouteMod: IUserMod
    {
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

    }
}
