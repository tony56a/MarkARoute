using System.Collections.Generic;
using UnityEngine;

namespace MarkARoute.Utils
{
    public class TextureReplaceConfig
    {
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
    }
}
