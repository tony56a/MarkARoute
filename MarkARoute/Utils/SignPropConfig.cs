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
        public List<Vector3> shieldOffset;
        public Vector3 textScale;
        public Vector3 shieldScale;
        public string fontType;
        public float angleOffset;

        public SignPropInfo(string propName,
                            bool isDouble,
                            List<Vector3> textOffsetNoSign,
                            List<Vector3> textOffsetSign,
                            List<Vector3> shieldOffset,
                            Vector3 textScale,
                            Vector3 shieldScale,
                            string fontType,
                            float angleOffset)
        {
            this.propName = propName;
            this.isDoubleGantry = isDouble;
            this.textOffsetNoSign = textOffsetNoSign;
            this.textOffsetSign = textOffsetSign;
            this.shieldOffset = shieldOffset;
            this.textScale = textScale;
            this.shieldScale = shieldScale;
            this.fontType = fontType;
            this.angleOffset = angleOffset;
        }
    }

    public class TextureSignPropInfo
    {
        public int numTextures;
        public List<float> rotationOffsets;
        public List<Rect> drawAreas;
        public List<string> drawAreaDescriptors;

        public TextureSignPropInfo(int numTextures, List<float> rotationOffsets, List<Rect> drawAreas, List<string> drawAreaDescriptors)
        {
            this.numTextures = numTextures;
            this.rotationOffsets = rotationOffsets;
            this.drawAreas = drawAreas;
            this.drawAreaDescriptors = drawAreaDescriptors;
        }
    }

    class SignPropConfig
    {
        public static readonly Dictionary<string, TextureSignPropInfo> texturePropInfoDict = new Dictionary<string, TextureSignPropInfo>
        {
            { "hwysign",
                new TextureSignPropInfo(1,
                    new List<float> { 0f },
                    new List<Rect> {new Rect(133,15,780,348) },
                    new List<string> { "Main Sign" } )
            },
            { "double gantry sign",
                new TextureSignPropInfo(3,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(220,5,581,364),
                                     new Rect(435,655,580,362),
                                     new Rect(493,381,294,75) },
                    new List<string> { "Right Sign",
                                       "Left Sign",
                                       "Exit Sign" } )
            },
            { "hs_signs01a_us",
                new TextureSignPropInfo(2,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(0,274,397,120),
                                     new Rect(0,394,397,118) },
                    new List<string> { "Right Sign",
                                       "Left Sign" } )
            },
            { "hs_signs01b_us",
                new TextureSignPropInfo(3,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(0,274,397,120),
                                     new Rect(0,394,397,118),
                                     new Rect(409,98,92,28) },
                    new List<string> { "Right Sign",
                                       "Left Sign",
                                       "Exit Sign" } )
            },
            { "hs_signs01a_german",
                new TextureSignPropInfo(2,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(0,394,284,118),
                                     new Rect(0,274,284,120) },
                    new List<string> { "Right Sign",
                                       "Left Sign" } )
            },
             { "hs_signs01b_german",
                new TextureSignPropInfo(2,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(0,394,284,118),
                                     new Rect(0,274,284,120) },
                    new List<string> { "Right Sign",
                                       "Left Sign" } )
            },
            { "hs_signs02a_german",
                new TextureSignPropInfo(1,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(0,350,512,162) },
                    new List<string> { "Main Sign" } )
            },
            { "hs_signs02cl_german",
                new TextureSignPropInfo(1,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(128,192,177,155) },
                    new List<string> { "Main Sign" } )
            },
               { "hs_signs02cr_german",
                new TextureSignPropInfo(1,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(318,194,179,151) },
                    new List<string> { "Main Sign" } )
            },
          { "hs_signs02d_german",
                new TextureSignPropInfo(2,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(318,194,179,151),
                                     new Rect(128,192,178,153) },
                    new List<string> { "Right Sign",
                                       "Left Sign" } )
            },
           { "hs_signs02e_german",
                new TextureSignPropInfo(1,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(3,258,200,254) },
                    new List<string> { "Main Sign" } )
            },
            { "single gantry",
                new TextureSignPropInfo(2,
                    new List<float> { 0f,0f,0f },
                    new List<Rect> { new Rect(220,5,581,364),
                                     new Rect(493,381,294,75) },
                    new List<string> { "Main Sign",
                                       "Exit Sign" } )
            },
        };

        public static readonly Dictionary<string, SignPropInfo> signPropInfoDict = new Dictionary<string, SignPropInfo>
        {
            { "hwysign",
                new SignPropInfo("hwysign",
                    false,
                    new List<Vector3> { new Vector3(0.2f, 6.5f, -4.7f) },
                    new List<Vector3> { new Vector3(0.2f, 5.8f, -4.7f) },
                    new List<Vector3> { new Vector3(0.2f, 7.2f, -3.8f) },
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(0.18f, 0.18f, 0.18f),
                    "Highway Gothic",
                    0 ) },
            { "uk_sign",
                new SignPropInfo("uk_sign",
                    false,
                    new List<Vector3> { new Vector3(0.2f,6.2f,-1f) },
                    new List<Vector3> { new Vector3(0.2f,6.2f,-1f) },
                    new List<Vector3> {new Vector3(0.2f,7.5f,-1f) },
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(0.18f, 0.18f, 0.18f),
                    "Transport",0 ) },
            { "double gantry sign",
                new SignPropInfo("double gantry sign",
                    true,
                    new List<Vector3> { new Vector3(0.75f,6.5f,0f), new Vector3(0.75f,6.5f,4.4f) },
                    new List<Vector3> { new Vector3(0.75f,5.8f,0f),new Vector3(0.75f,6.5f,4.4f) },
                    new List<Vector3> { new Vector3(0.8f,6.7f,0f) },
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(0.12f, 0.12f, 0.12f),
                    "Highway Gothic",
                    0 ) },
            { "single gantry",
                new SignPropInfo("single gantry",
                    false,
                    new List<Vector3> { new Vector3(0.53f,6.5f,-1.8f) },
                    new List<Vector3> { new Vector3(0.53f,6f,-1.8f) },
                    new List<Vector3> { new Vector3(0.53f,7.1f,-1.8f) },
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(0.12f, 0.12f, 0.12f),
                    "Highway Gothic",
                    0 ) },
            { "hs_signs01b_german",
                new SignPropInfo("hs_signs01b_german",
                    true, new List<Vector3> { new Vector3(21f,10f,0.25f), new Vector3(8.3f,10f,0.25f) },
                    new List<Vector3> { new Vector3(20f, 10f, 0.25f),new Vector3(8.3f, 10f, 0.25f) },
                    new List<Vector3> { new Vector3(23f,10f,0.25f) },
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.16f, 0.16f, 0.16f),
                    "Highway Gothic",
                    -90 ) },
            { "hs_signs02a_german",
                new SignPropInfo("hs_signs02a_german",
                    true,
                    new List<Vector3> { new Vector3(15f,10f,0.5f), new Vector3(8f,11f,0.5f) },
                    new List<Vector3> { new Vector3(14f, 10f, 0.5f),new Vector3(8f, 11f, 0.5f) },
                    new List<Vector3> { new Vector3(17.5f,10f,0.5f) },
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.16f, 0.16f, 0.16f),
                    "Highway Gothic",
                    -90 ) },
            { "hs_signs01a_german",
                new SignPropInfo("hs_signs01a_german",
                    true,
                    new List<Vector3> { new Vector3(21f,10f,0.25f), new Vector3(8.3f,10f,0.25f) },
                    new List<Vector3> { new Vector3(20f, 10f, 0.25f),new Vector3(8.3f, 10f, 0.25f) },
                    new List<Vector3> {new Vector3(24f,10f,0.25f) },
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.16f, 0.16f, 0.16f),
                    "Highway Gothic",
                    -90 ) },
            { "hs_signs02d_german",
                new SignPropInfo("hs_signs02d_german",
                    true,
                    new List<Vector3> { new Vector3(4.6f,10.2f,8.5f), new Vector3(-4.6f, 10.2f, 8.5f) },
                    new List<Vector3> { new Vector3(4.6f,10.2f,8.5f), new Vector3(-4.6f, 10.2f, 8.5f) },
                    new List<Vector3> { new Vector3(4.6f,11.5f,8.5f) },
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.16f, 0.16f, 0.16f),
                    "Highway Gothic",
                    -90 ) },
            { "hs_signs02e_german",
                new SignPropInfo("hs_signs02e_german",
                    false,
                    new List<Vector3> { new Vector3(-1f, 4.5f, 8.7f) },
                    new List<Vector3> { new Vector3(-1f,4.5f,8.7f) },
                    new List<Vector3> {new Vector3(-1f,6.5f,8.7f) } ,
                    new Vector3(0.15f, 0.15f, 0.15f),
                    new Vector3(0.18f, 0.18f, 0.18f),
                    "Highway Gothic",
                    -90 ) },
            { "hs_signs02cl_german",
                new SignPropInfo("hs_signs02cl_german",
                    false,
                    new List<Vector3> { new Vector3(-12.5f, 10.2f, 0.5f) },
                    new List<Vector3> { new Vector3(-12.5f, 10.2f, 0.5f) },
                    new List<Vector3> {new Vector3(-12.6f, 11.5f, 0.5f) },
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(0.15f, 0.15f, 0.15f),
                    "Highway Gothic",
                    -90 ) },
            { "hs_signs02cr_german",
                new SignPropInfo("hs_signs02cr_german",
                    false,
                    new List<Vector3> { new Vector3(12.5f, 10f, 0.5f) },
                    new List<Vector3> { new Vector3(12.5f, 10f, 0.5f) },
                    new List<Vector3> {new Vector3(12.6f, 11.5f, 0.5f) },
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(0.15f, 0.15f, 0.15f),
                    "Highway Gothic",
                    -90 ) },
            { "hs_signs01a_us",
                new SignPropInfo("hs_signs01a_us",
                    true,
                    new List<Vector3> { new Vector3(21f,10f,0.25f), new Vector3(11.2f,10f,0.25f) },
                    new List<Vector3> { new Vector3(20f, 10f, 0.25f),new Vector3(11.2f, 10f, 0.25f) },
                    new List<Vector3> {new Vector3(24f,10f,0.25f) },
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.16f, 0.16f, 0.16f),
                    "Highway Gothic",
                    -90 ) },
            { "hs_signs01b_us",
                new SignPropInfo("hs_signs01b_us",
                    true,
                    new List<Vector3> { new Vector3(21f,10f,0.25f), new Vector3(11.2f,10f,0.25f) },
                    new List<Vector3> { new Vector3(20f, 10f, 0.25f),new Vector3(11.2f, 10f, 0.25f) },
                    new List<Vector3> {new Vector3(24f,10f,0.25f) },
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.16f, 0.16f, 0.16f),
                    "Highway Gothic",
                    -90 ) },

        };
    }
}
