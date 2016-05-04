using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Utils
{
    public class SignPropInfo
    {
        public string propName;
        public bool isDoubleGantry;
        public List<Vector3> textOffsetNoSign;
        public List<Vector3> textOffsetSign;
        public Vector3 shieldOffset;
        public Vector3 textScale;
        public Vector3 shieldScale;
        public string fontType;

        public SignPropInfo(string propName,
                            bool isDouble,
                            List<Vector3> textOffsetNoSign,
                            List<Vector3> textOffsetSign,
                            Vector3 shieldOffset,
                            Vector3 textScale,
                            Vector3 shieldScale,
                            string fontType)
        {
            this.propName = propName;
            this.isDoubleGantry = isDouble;
            this.textOffsetNoSign = textOffsetNoSign;
            this.textOffsetSign = textOffsetSign;
            this.shieldOffset = shieldOffset;
            this.textScale = textScale;
            this.shieldScale = shieldScale;
            this.fontType = fontType;
        }
    }
    class SignPropConfig
    {
        public static readonly Dictionary<string, SignPropInfo> signPropInfoDict = new Dictionary<string, SignPropInfo>
        {
            { "hwysign", new SignPropInfo("hwysign", false, new List<Vector3> { new Vector3(0.2f, 6.5f, -4.7f) }, new List<Vector3> { new Vector3(0.2f, 5.8f, -4.7f) },new Vector3(0.2f, 7.2f, -3.8f),new Vector3(0.1f, 0.1f, 0.1f),new Vector3(0.18f, 0.18f, 0.18f), "Highway Gothic" ) },
            { "uk_sign", new SignPropInfo("uk_sign", false, new List<Vector3> { new Vector3(0.2f,6.2f,-1f) }, new List<Vector3> { new Vector3(0.2f,6.2f,-1f) }, new Vector3(0.2f,7.5f,-1f) , new Vector3(0.1f, 0.1f, 0.1f),new Vector3(0.18f, 0.18f, 0.18f),"Transport" ) },
            { "double gantry sign", new SignPropInfo("double gantry sign", true, new List<Vector3> { new Vector3(0.75f,6.5f,0f), new Vector3(0.75f,6.5f,4.4f) }, new List<Vector3> { new Vector3(0.75f,5.8f,0f),new Vector3(0.75f,6.5f,4.4f) },new Vector3(0.8f,6.7f,0f),new Vector3(0.1f, 0.1f, 0.1f),new Vector3(0.12f, 0.12f, 0.12f),"Highway Gothic" ) },
        };
    }
}
