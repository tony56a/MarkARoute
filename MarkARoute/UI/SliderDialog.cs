using ColossalFramework.UI;
using MarkARoute.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MarkARoute.UI
{
    abstract class SliderDialog : UIPanel, IEventSubscriber
    {
        protected RectOffset m_UIPadding = new RectOffset(5, 5, 5, 5);

        private int titleOffset = 40;
        private TitleBar m_panelTitle;

        protected UILabel m_descLabel;
        protected UISlider m_slider;

        private Vector2 offset = Vector2.zero;

        public abstract string descText
        {
            get;
        }

        public abstract int defaultValue
        {
            get;
        }

        public abstract int minVal
        {
            get;
        }
        public abstract int maxVal
        {
            get;
        }

        public abstract float intervalVal
        {
            get;
        }

        public abstract Vector3 posOffset
        {
            get;
        }

        public abstract void SliderSetValue(float value);
        public abstract void onReceiveEvent(string eventName, object eventData);

        public override void Awake()
        {
            this.isInteractive = true;
            this.enabled = true;
            this.width = 180;
            this.height = 300;

            base.Awake();
        }

        public override void Start()
        {
            base.Start();

            m_panelTitle = this.AddUIComponent<TitleBar>();
            m_panelTitle.title = descText;
            m_panelTitle.m_closeActions.Add("unsetTools");

            CreatePanelComponents();
            this.relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2) + width, Mathf.Floor((GetUIView().fixedHeight - height) / 2)) + posOffset;
            this.backgroundSprite = "MenuPanel2";
        }

        private void CreatePanelComponents()
        {
            // From CWMlolzlz's Building Search mod
            m_descLabel = this.AddUIComponent<UILabel>();
            m_descLabel.text = descText + ":" + defaultValue;
            m_descLabel.autoSize = false;
            m_descLabel.size = new Vector2(this.width - m_UIPadding.left - m_UIPadding.right, 50);
            m_descLabel.padding = m_UIPadding;
            m_descLabel.relativePosition = new Vector2(m_UIPadding.left, titleOffset + m_UIPadding.top);
            m_descLabel.textAlignment = UIHorizontalAlignment.Left;
            m_descLabel.verticalAlignment = UIVerticalAlignment.Middle;

            m_slider = this.AddUIComponent<UISlider>();
            m_slider.minValue = minVal;
            m_slider.maxValue = maxVal;
            m_slider.stepSize = intervalVal;
            m_slider.value = defaultValue;
            m_slider.relativePosition = new Vector2(m_UIPadding.left, m_descLabel.relativePosition.y + m_descLabel.height + 2 * m_UIPadding.top);
            m_slider.size = new Vector2(this.width - m_UIPadding.left - m_UIPadding.right, 16.0f);

            UISprite thumbSprite = m_slider.AddUIComponent<UISprite>();
            thumbSprite.name = "Thumb";
            thumbSprite.spriteName = "SliderBudget";

            m_slider.backgroundSprite = "ScrollbarTrack";
            m_slider.thumbObject = thumbSprite;
            m_slider.orientation = UIOrientation.Horizontal;
            m_slider.isVisible = true;
            m_slider.enabled = true;
            m_slider.canFocus = true;
            m_slider.isInteractive = true;

            m_slider.eventValueChanged += (component, f) =>
            {
                SliderSetValue(f);
                m_descLabel.text = descText + ":" + m_slider.value.ToString("0.00");
            };

            this.height = m_slider.relativePosition.y + m_slider.height + m_UIPadding.bottom;

        }

    }
}
