// ========================================================================== //
//
//  class OmniSlider
//  -----
//  Purpose: Slider class based on OmniSelectable
//
//  Usage: Manipulates 3D Transforms to visualize a slider, as opposed to
//      2D RectTransforms
//
//      Always works in local XY plane.
//
//  Based on: https://bitbucket.org/Unity-Technologies/ui/src/2017.3/UnityEngine.UI/UI/Core/Slider.cs
//
//
//  Created: 2018-11-21
//  Updated: 2018-11-21
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Htc.Omni
{
    [RequireComponent(typeof(RectTransform))]
    public class OmniSlider : OmniSelectable, IPointerDownHandler, IDragHandler, IInitializePotentialDragHandler
    {
        public enum Direction
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        private enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }

        [Serializable]
        public class SliderEvent : UnityEvent<float> { }

        [SerializeField]
        [Tooltip("The corresponding scale along the slider axis would be set to equal the length of the slider")]
        private BoxCollider _fillBox;
        public BoxCollider fillBox { get { return _fillBox; } set { if (SetClass(ref _fillBox, value)) UpdateVisuals(); } }

        [SerializeField]
        [Tooltip("This transform would be rotated so its local z-axis aligns with the slider axis. The z-scale would be set to equal the length of the slider")]
        private Transform _background;
        public Transform background { get { return _background; } set { if (SetClass(ref _background, value)) UpdateVisuals(); } }

        [SerializeField]
        [Tooltip("This transform would be positioned at the zero-position of the slider, with z-axis towards the one-position. The z-scale would track the normalized value of the slider")]
        private Transform _fill;
        public Transform fill { get { return _fill; } set { if (SetClass(ref _fill, value)) UpdateVisuals(); } }

        [SerializeField]
        [Tooltip("The position of this transform would track the position of the handle")]
        private Transform _handle;
        public Transform handle { get { return _handle; } set { if (SetClass(ref _handle, value)) UpdateVisuals(); } }

        [SerializeField]
        private float _length = 1;
        public float length { get { return _length; } set { if (SetStruct(ref _length, value)) UpdateVisuals(); } }

        // [Space]

        [SerializeField]
        private Direction _direction = Direction.LeftToRight;
        public Direction direction { get { return _direction; } set { if (SetStruct(ref _direction, value)) UpdateVisuals(); } }

        [SerializeField]
        private float _minValue = 0;
        public float minValue { get { return _minValue; } set { if (SetStruct(ref _minValue, value)) { Set(_value); UpdateVisuals(); } } }

        [SerializeField]
        private float _maxValue = 1;
        public float maxValue { get { return _maxValue; } set { if (SetStruct(ref _maxValue, value)) { Set(_value); UpdateVisuals(); } } }

        [SerializeField]
        private bool _wholeNumbers = false;
        public bool wholeNumbers { get { return _wholeNumbers; } set { if (SetStruct(ref _wholeNumbers, value)) { Set(_value); UpdateVisuals(); } } }

        [SerializeField]
        protected float _value;
        public virtual float value
        {
            get
            {
                if (wholeNumbers)
                    return Mathf.Round(_value);
                return _value;
            }
            set
            {
                Set(value);
            }
        }

        public float normalizedValue
        {
            get
            {
                if (Mathf.Approximately(minValue, maxValue))
                    return 0;
                return Mathf.InverseLerp(minValue, maxValue, value);
            }
            set
            {
                this.value = Mathf.Lerp(minValue, maxValue, value);
            }
        }

        // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
        [SerializeField]
        private SliderEvent _onValueChanged = new SliderEvent();
        public SliderEvent onValueChanged { get { return _onValueChanged; } }

        // Size of each step.
        private float stepSize { get { return wholeNumbers ? 1 : (maxValue - minValue) * 0.1f; } }

        private Axis axis { get { return (direction == Direction.LeftToRight || direction == Direction.RightToLeft) ? Axis.Horizontal : Axis.Vertical; } }
        private bool reverseValue { get { return direction == Direction.RightToLeft || direction == Direction.TopToBottom; } }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (wholeNumbers)
            {
                _minValue = Mathf.Round(_minValue);
                _maxValue = Mathf.Round(_maxValue);
            }

            //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
            if (IsActive())
            {
                Set(_value, false);
                // Update rects since other things might affect them even if value didn't change.
                UpdateVisuals();
            }
        }

#endif // if UNITY_EDITOR

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        protected override void OnEnable()
        {
            base.OnEnable();
            Set(_value, false);
            // Update rects since they need to be initialized correctly.
            UpdateVisuals();
        }

        float ClampValue(float input)
        {
            float newValue = Mathf.Clamp(input, minValue, maxValue);
            if (wholeNumbers)
                newValue = Mathf.Round(newValue);
            return newValue;
        }

        // Set the valueUpdate the visible Image.
        void Set(float input)
        {
            Set(input, true);
        }

        protected virtual void Set(float input, bool sendCallback)
        {
            // Clamp the input
            float newValue = ClampValue(input);

            // If the stepped value doesn't match the last one, it's time to update
            if (_value == newValue)
                return;

            _value = newValue;
            UpdateVisuals();
            if (sendCallback)
            {
                UISystemProfilerApi.AddMarker("Slider.value", this);
                onValueChanged.Invoke(newValue);
            }
        }

        // Force-update the slider. Useful if you've changed the properties and want it to update visually.
        private void UpdateVisuals()
        {
            var axis = Vector3.zero;
            switch (direction)
            {
                case Direction.LeftToRight: axis = Vector3.right; break;
                case Direction.RightToLeft: axis = Vector3.left; break;
                case Direction.BottomToTop: axis = Vector3.up; break;
                case Direction.TopToBottom: axis = Vector3.down; break;
            }

            var origin = -(axis * length / 2f);
            var rotation = Quaternion.LookRotation(axis);

            if (fillBox != null)
            {
                fillBox.center = Vector3.zero;
                var fs = fillBox.size;

                switch (direction)
                {
                    case Direction.LeftToRight:
                    case Direction.RightToLeft: fs.x = length; break;
                    case Direction.BottomToTop:
                    case Direction.TopToBottom: fs.y = length; break;
                }

                fillBox.size = fs;
            }

            if (background != null)
            {
                background.localPosition = Vector3.zero;
                background.localRotation = rotation;
                background.localScale = new Vector3(1f, 1f, length);
            }

            if (fill != null)
            {
                fill.localPosition = origin;
                fill.localRotation = rotation;
                fill.localScale = new Vector3(1f, 1f, normalizedValue * length);
            }

            if (handle != null)
            {
                handle.localRotation = rotation;
                handle.localPosition = axis * (normalizedValue - 0.5f) * length;
            }
        }

        // Update the slider's position based on the mouse.
        void UpdateDrag(PointerEventData eventData, Camera cam)
        {
            if (!eventData.pointerCurrentRaycast.isValid)
                return;

            var pos = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);

            var localCursor = (axis == Axis.Horizontal ? pos.x : pos.y) * (reverseValue ? -1 : 1);
            normalizedValue = Mathf.Clamp01(localCursor / length + 0.5f);
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return isActiveAndInteractable && eventData.button == PointerEventData.InputButton.Left;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            base.OnPointerDown(eventData);
            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;
            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (!isActiveAndInteractable)
            {
                base.OnMove(eventData);
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    if (axis == Axis.Horizontal)
                        Set(reverseValue ? value + stepSize : value - stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Right:
                    if (axis == Axis.Horizontal)
                        Set(reverseValue ? value - stepSize : value + stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Up:
                    if (axis == Axis.Vertical)
                        Set(reverseValue ? value - stepSize : value + stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Down:
                    if (axis == Axis.Vertical)
                        Set(reverseValue ? value + stepSize : value - stepSize);
                    else
                        base.OnMove(eventData);
                    break;
            }
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        void OnDrawGizmosSelected()
        {
            var localFrom = Vector3.zero;
            var localTo = Vector3.zero;

            var scale = 1f;

            switch (direction)
            {
                case Direction.LeftToRight: localFrom = Vector3.left; localTo = Vector3.right; scale = transform.lossyScale.x; break;
                case Direction.RightToLeft: localFrom = Vector3.right; localTo = Vector3.left; scale = transform.lossyScale.x; break;
                case Direction.BottomToTop: localFrom = Vector3.down; localTo = Vector3.up; scale = transform.lossyScale.y; break;
                case Direction.TopToBottom: localFrom = Vector3.up; localTo = Vector3.down; scale = transform.lossyScale.y; break;
            }

            var startPos = transform.TransformPoint(localFrom * length / 2);
            var endPos = transform.TransformPoint(localTo * length / 2);

            Gizmos.color = new Color(0.3f, 1, 0.3f, 0.75F);
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(Vector3.Lerp(startPos, endPos, normalizedValue), scale * 0.03f);
        }

        public static bool SetColor(ref Color currentValue, Color newValue)
        {
            if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}