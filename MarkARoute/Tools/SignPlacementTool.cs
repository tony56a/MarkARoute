using ColossalFramework;
using ColossalFramework.Math;
using MarkARoute.Managers;
using MarkARoute.UI;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.Tools
{
    abstract class SignPlacementTool : DefaultTool, IEventSubscriber
    {
        public float m_angle;
        public Texture2D m_brush;
        public CursorInfo m_buildCursor;
        protected PropInfo m_propInfo;
        private bool m_mouseLeftDown;
        private bool m_mouseRightDown;
        private ToolBase.ToolErrors m_placementErrors;
        private Randomizer m_randomizer;

        protected Vector3 m_cachedPosition;
        protected float m_cachedAngle;
        protected string propName;

        public string routePrefix;
        public string routeStr;
        public string destination;

        public bool m_angleChanged;

        protected abstract override void Awake();
        protected abstract void HandleSignPlaced();

        protected void BaseAwake()
        {
            base.Awake();
        }

        protected override void OnToolGUI(UnityEngine.Event e)
        {
            if (!this.m_toolController.IsInsideUI && e.type == UnityEngine.EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    this.m_mouseLeftDown = true;
                    HandleSignPlaced();
                }
                else
                {
                    if (e.button != 1)
                        return;
                    this.m_angleChanged = false;
                    this.m_mouseRightDown = true;
                }
            }
            else
            {
                if (e.type != UnityEngine.EventType.MouseUp)
                    return;
                if (e.button == 0)
                {
                    this.m_mouseLeftDown = false;
                }
                else
                {
                    if (e.button != 1)
                        return;
                    this.m_mouseRightDown = false;
                    if (!this.m_angleChanged)
                    {
                        this.m_angle = (this.m_angle + 15f) % 360f;
                    }
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.ToolCursor = this.m_buildCursor;
            this.m_toolController.ClearColliding();
            this.m_placementErrors = ToolBase.ToolErrors.Pending;
            this.m_toolController.SetBrush((Texture2D)null, Vector3.zero, 1f);

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.ToolCursor = (CursorInfo)null;
            this.m_toolController.SetBrush((Texture2D)null, Vector3.zero, 1f);
            this.m_mouseLeftDown = false;
            this.m_mouseRightDown = false;
            this.m_placementErrors = ToolBase.ToolErrors.Pending;
            this.m_mouseRayValid = false;
        }

        public override void RenderGeometry(RenderManager.CameraInfo cameraInfo)
        {
            PropInfo info = this.m_propInfo;
            if (info != null && (this.m_placementErrors == ToolBase.ToolErrors.None && !this.m_toolController.IsInsideUI) && Cursor.visible)
            {
                Randomizer r1 = this.m_randomizer;
                Randomizer r2 = new Randomizer((int)Singleton<PropManager>.instance.m_props.NextFreeItem(ref r1));
                float scale = info.m_minScale + (float)((double)r2.Int32(10000U) * ((double)info.m_maxScale - (double)info.m_minScale) * 9.99999974737875E-05);
                Color color = info.GetColor(ref r2);
                InstanceID id = new InstanceID();
                if (info.m_requireHeightMap)
                {
                    Texture _HeightMap;
                    Vector4 _HeightMapping;
                    Vector4 _SurfaceMapping;
                    Singleton<TerrainManager>.instance.GetHeightMapping(this.m_cachedPosition, out _HeightMap, out _HeightMapping, out _SurfaceMapping);
                    PropInstance.RenderInstance(cameraInfo, info, id, this.m_cachedPosition, scale, this.m_cachedAngle, color, RenderManager.DefaultColorLocation, true, _HeightMap, _HeightMapping, _SurfaceMapping);
                }
                else
                    PropInstance.RenderInstance(cameraInfo, info, id, this.m_cachedPosition, scale, this.m_cachedAngle, color, RenderManager.DefaultColorLocation, true);
            }
            base.RenderGeometry(cameraInfo);
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            PropInfo info = this.m_propInfo;
            if (info != null && (!this.m_toolController.IsInsideUI && Cursor.visible))
            {
                Color toolColor = this.GetToolColor(false, this.m_placementErrors != ToolBase.ToolErrors.None);
                this.m_toolController.RenderColliding(cameraInfo, toolColor, toolColor, toolColor, toolColor, (ushort)0, (ushort)0);
                Randomizer r = this.m_randomizer;
                Randomizer randomizer = new Randomizer((int)PropManager.instance.m_props.NextFreeItem(ref r));
                float scale = info.m_minScale + (float)((double)randomizer.Int32(10000U) * ((double)info.m_maxScale - (double)info.m_minScale) * 9.99999974737875E-05);
                SignPlacementTool.RenderOverlay(cameraInfo, info, this.m_cachedPosition, scale, this.m_cachedAngle, toolColor);
            }
            base.RenderOverlay(cameraInfo);
        }

        public static void RenderOverlay(RenderManager.CameraInfo cameraInfo, PropInfo info, Vector3 position, float scale, float angle, Color color)
        {
            if (info == null)
                return;
            float size = Mathf.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale;
            ++Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls;
            Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, color, position, size, position.y - 100f, position.y + 100f, false, true);
        }

        protected override void OnToolUpdate()
        {
            PropInfo propInfo = this.m_propInfo;
            if (propInfo == null)
                return;

            if(Input.GetKey(KeyCode.Mouse1))
            {
                float axis = Input.GetAxis("Mouse X");
                if( Math.Abs(axis) > 0.1 ) {
                    m_angleChanged = true;
                    this.m_angle = (this.m_angle + (10f*axis)) % 360f;
                }
            }
           
        }
        protected override void OnToolLateUpdate()
        {
            this.m_mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            this.m_mouseRayLength = Camera.main.farClipPlane;
            this.m_mouseRayValid = !this.m_toolController.IsInsideUI && Cursor.visible;
            this.m_toolController.SetBrush((Texture2D)null, Vector3.zero, 1f);
            this.m_cachedPosition = this.m_mousePosition;
            this.m_cachedAngle = this.m_angle * (float)(Math.PI / 180.0);
        }

        public static void DispatchPlacementEffect(Vector3 pos, bool bulldozing)
        {
            EffectInfo effect = !bulldozing ? Singleton<PropManager>.instance.m_properties.m_placementEffect : Singleton<PropManager>.instance.m_properties.m_bulldozeEffect;
            if (effect == null)
                return;
            InstanceID instance = new InstanceID();
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(pos, Vector3.up, 1f);
            Singleton<EffectManager>.instance.DispatchEffect(effect, instance, spawnArea, Vector3.zero, 0.0f, 1f, Singleton<AudioManager>.instance.DefaultGroup);
        }

        public override void SimulationStep()
        {

            ToolBase.RaycastInput input = new ToolBase.RaycastInput(this.m_mouseRay, this.m_mouseRayLength);
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            ulong[] collidingSegments;
            ulong[] collidingBuildings;
            this.m_toolController.BeginColliding(out collidingSegments, out collidingBuildings);
            try
            {
                ToolBase.RaycastOutput output;
                if (this.m_mouseRayValid && ToolBase.RayCast(input, out output))
                {

                    float terrainHeight = TerrainManager.instance.SampleDetailHeight(output.m_hitPos);
                    output.m_hitPos.y = output.m_hitPos.y > terrainHeight ? output.m_hitPos.y : terrainHeight;
                    Randomizer r = this.m_randomizer;
                    ushort id = Singleton<PropManager>.instance.m_props.NextFreeItem(ref r);
                    this.m_mousePosition = output.m_hitPos;
                    this.m_placementErrors = ToolErrors.None;

                }
                else
                    this.m_placementErrors = ToolBase.ToolErrors.RaycastFailed;
            }
            finally
            {
                this.m_toolController.EndColliding();
            }
        }

        public override ToolBase.ToolErrors GetErrors()
        {
            return ToolErrors.None;
        }

        public void onReceiveEvent(string eventName, object eventData)
        {
            switch (eventName)
            {
                case "setAngle":
                    this.m_angle = (float)eventData;
                    break;
                default:
                    break;
            }
        }
    }
}
