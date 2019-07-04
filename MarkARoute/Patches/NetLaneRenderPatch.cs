﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using MarkARoute.Managers;
using UnityEngine;
using MarkARoute.Utils;
using ColossalFramework.Math;

namespace MarkARoute.Patches
{
    
    [HarmonyPatch(typeof(PropInstance))]
    public class NetLaneRenderPatch
    {
        static System.Random random = new System.Random();
        static List<Material> signMaterials = new List<Material>();
        static Dictionary<ushort, Material> signs = new Dictionary<ushort, Material>();
        
        static MethodInfo TargetMethod()
        {
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

            var myOverloads = typeof(PropInstance)
                              .GetMember("RenderInstance", MemberTypes.Method, flags)
                              .Cast<MethodInfo>();
            foreach( MethodInfo method in myOverloads)
            {
                // Only target the method invoked by the NetLane.RenderInstance method
                if (method.GetParameters().Count() == 9) {
                    return method;
                }
            }

            return null;
        }

        public static bool Prefix( ref RenderManager.CameraInfo cameraInfo,ref PropInfo info, ref InstanceID id,ref Vector3 position,ref float scale, ref float angle, ref Color color, ref Vector4 objectIndex, bool active)
        {

            if (info != null && info.name != null && info.name.ToLower().Contains("motorway overroad") && RouteManager.instance.m_overrideSignDict.ContainsKey(id.NetSegment) )
            {
                OverrideSignContainer container = RouteManager.instance.m_overrideSignDict[id.NetSegment];

                // If sign is not to be rendered, then bail out, but skip original method
                if (container.m_exitNum == RouteManager.NONE)
                {
                    return false;
                }

                // Safety check, make sure the texture exists
                if (container.m_mainTexture == null)
                {
                    return true;
                }

                PropInfo propInfo = PropUtils.m_signPropDict[ container.m_exitNum ];
                OverrideSignInfo propPositionInfo = SignPropConfig.overrideSignValues[container.m_exitNum];
                NetSegment netSegment = NetManager.instance.m_segments.m_buffer[(int)id.NetSegment];
                
                float angleOffset = propPositionInfo.angleOffset * ((float)System.Math.PI / 180f);
                Quaternion finalAngle = Quaternion.AngleAxis((angle + angleOffset) * 57.29578f, Vector3.down);
                Vector3 posOffset = propPositionInfo.positionOffset;
                scale = propPositionInfo.scale;
                Vector3 direction = (netSegment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None ? netSegment.m_endDirection : netSegment.m_startDirection;
                posOffset.x *= direction.z;
                posOffset.z *= direction.x;

                if (!info.m_hasRenderer || (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) == 0)
                    return false;
                if (InfoManager.instance.CurrentMode == InfoManager.InfoMode.None)
                {
                    if (!active && !info.m_alwaysActive)
                        objectIndex.z = 0.0f;
                    else if ((double)info.m_illuminationOffRange.x < 1000.0 || info.m_illuminationBlinkType != LightEffect.BlinkType.None)
                    {
                        LightSystem lightSystem = RenderManager.instance.lightSystem;
                        Randomizer randomizer = new Randomizer(id.Index);
                        float num1 = info.m_illuminationOffRange.x + (float)((double)randomizer.Int32(100000U) * 9.99999974737875E-06 * ((double)info.m_illuminationOffRange.y - (double)info.m_illuminationOffRange.x));
                        objectIndex.z = MathUtils.SmoothStep(num1 + 0.01f, num1 - 0.01f, lightSystem.DayLightIntensity);
                        if (info.m_illuminationBlinkType != LightEffect.BlinkType.None)
                        {
                            Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                            float f = (float)((double)num1 * 3.71000003814697 + (double)SimulationManager.instance.m_simulationTimer / (double)blinkVector.w);
                            float x = (f - Mathf.Floor(f)) * blinkVector.w;
                            float num2 = MathUtils.SmoothStep(blinkVector.x, blinkVector.y, x);
                            float num3 = MathUtils.SmoothStep(blinkVector.w, blinkVector.z, x);
                            objectIndex.z *= (float)(1.0 - (double)num2 * (double)num3);
                        }
                    }
                    else
                        objectIndex.z = 1f;
                }

                if (cameraInfo == null || cameraInfo.CheckRenderDistance(position, propInfo.m_lodRenderDistance))
                {
                    Matrix4x4 matrix = new Matrix4x4();
                    matrix.SetTRS(position + posOffset, finalAngle, new Vector3(scale, scale, scale));
                    PropManager instance = PropManager.instance;
                    MaterialPropertyBlock properties = instance.m_materialBlock;
                    properties.Clear();
                    properties.SetColor(instance.ID_Color, color);
                    properties.SetVector(instance.ID_ObjectIndex, objectIndex);
                    if (propInfo.m_rollLocation != null)
                    {
                        propInfo.m_material.SetVectorArray(instance.ID_RollLocation, propInfo.m_rollLocation);
                        propInfo.m_material.SetVectorArray(instance.ID_RollParams, propInfo.m_rollParams);
                    }
                    ++instance.m_drawCallData.m_defaultCalls;
                    Graphics.DrawMesh(propInfo.m_mesh, matrix, container.m_mainTexture, propInfo.m_prefabDataLayer, (Camera)null, 0, properties);
                }
                else if ((UnityEngine.Object)propInfo.m_lodMaterialCombined == (UnityEngine.Object)null)
                {
                    Matrix4x4 matrix = new Matrix4x4();
                    matrix.SetTRS(position + posOffset, finalAngle, new Vector3(scale, scale, scale));
                    PropManager instance = PropManager.instance;
                    MaterialPropertyBlock properties = instance.m_materialBlock;
                    properties.Clear();
                    properties.SetColor(instance.ID_Color, color);
                    properties.SetVector(instance.ID_ObjectIndex, objectIndex);
                    if (propInfo.m_rollLocation != null)
                    {
                        propInfo.m_material.SetVectorArray(instance.ID_RollLocation, propInfo.m_rollLocation);
                        propInfo.m_material.SetVectorArray(instance.ID_RollParams, propInfo.m_rollParams);
                    }
                    ++instance.m_drawCallData.m_defaultCalls;
                    Graphics.DrawMesh(propInfo.m_lodMesh, matrix, propInfo.m_lodMaterial, propInfo.m_prefabDataLayer, (Camera)null, 0, properties);
                }
                else
                {
                    objectIndex.w = scale;
                    propInfo.m_lodLocations[propInfo.m_lodCount] = new Vector4(position.x + posOffset.x, position.y + posOffset.y, position.z + posOffset.z, (angle + angleOffset));
                    propInfo.m_lodObjectIndices[propInfo.m_lodCount] = objectIndex;
                    propInfo.m_lodColors[propInfo.m_lodCount] = (Vector4)color.linear;
                    propInfo.m_lodMin = Vector3.Min(propInfo.m_lodMin, position);
                    propInfo.m_lodMax = Vector3.Max(propInfo.m_lodMax, position);
                    if (++propInfo.m_lodCount != propInfo.m_lodLocations.Length)
                    {
                        return false;
                    }
                    PropInstance.RenderLod(cameraInfo, propInfo);
                }
                return false;
            }
            return true;
        }
    }
}
