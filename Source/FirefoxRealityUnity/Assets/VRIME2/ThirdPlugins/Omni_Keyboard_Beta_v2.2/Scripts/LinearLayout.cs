// ========================================================================== //
//
//  class LinearLayout
//  -----
//  Purpose: Align all child game objects' position along a path
//
//  Note: This script is for edit-time object placement only. It does not do anything
//      at run-time.
//
//
//  Created: 2018-12-19
//  Updated: 2018-12-19
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Htc.Omni
{
    [ExecuteInEditMode]
    public class LinearLayout : BaseBehaviour
    {
        private const float curvatureCutoff = 0.1f;

        public enum Axis
        {
            X,
            Y,
            Z
        }

        public enum Alignment
        {
            Head,
            Center,
            Tail
        }

        public Axis axis;
        public Alignment alignment;
        public float defaultElementWidth = 1;
        public float margin = 0;

        [Tooltip("Angle per unit length, in degrees")]
        public float curvature = 0;

        public float axialRotation = 0;

        public bool controlRotation = false;

        public bool reverseElementOrder = false;

        private List<float> elementWidths = new List<float>();

        private Vector3 axisDir
        {
            get
            {
                if (axis == Axis.X)
                    return Vector3.right;
                else if (axis == Axis.Y)
                    return Vector3.up;
                else
                    return Vector3.forward;
            }
        }

        private Vector3 curveDir
        {
            get
            {
                if (axis == Axis.X)
                    return Vector3.up;
                else if (axis == Axis.Y)
                    return Vector3.forward;
                else
                    return Vector3.right;
            }
        }

        private Vector3 rotateDir
        {
            get
            {
                if (axis == Axis.X)
                    return Vector3.forward;
                else if (axis == Axis.Y)
                    return Vector3.right;
                else
                    return Vector3.up;
            }
        }

        /// <summary>
        /// Update the transform of child game objects according to the current setting
        /// </summary>
        public void UpdateLayout()
        {
            if (transform.childCount > 0)
            {
                var totalWidth = GetElementWidths(elementWidths);
                var offset = 0f;
                if (alignment == Alignment.Center)
                    offset = totalWidth / -2f;
                else if (alignment == Alignment.Tail)
                    offset = -totalWidth;

                var pos = offset;

                var widthEnum = elementWidths.GetEnumerator();
                widthEnum.MoveNext();

                var transList = GetTransformList(reverseElementOrder);
                foreach (Transform trans in transList)
                {
                    var width = widthEnum.Current;
                    UpdateTransform(trans, pos + width / 2f);

                    pos += width + margin;
                    widthEnum.MoveNext();
                }
            }
        }

        private void UpdateTransform(Transform trans, float pos)
        {
            if (Mathf.Abs(curvature) > curvatureCutoff)
            {
                var radius = Mathf.Rad2Deg / curvature;
                var focus = Quaternion.AngleAxis(axialRotation, axisDir) * curveDir * radius;
                var rotDir = Quaternion.AngleAxis(axialRotation, axisDir) * rotateDir;
                var finalRot = Quaternion.AngleAxis(Mathf.Rad2Deg * pos / radius, rotDir);
                trans.localPosition = finalRot * -focus + focus;

                if (controlRotation)
                {
                    trans.localRotation = finalRot;
                }
            }
            else
            {
                trans.localPosition = axisDir * pos;
                trans.localRotation = Quaternion.identity;
            }
        }

        private float GetElementWidths(List<float> widths)
        {
            if (widths != null)
                widths.Clear();

            if (transform.childCount == 0)
                return 0;

            var total = 0f;
            var count = 0;
            var transList = GetTransformList(reverseElementOrder);
            foreach (Transform trans in transList)
            {
                var width = defaultElementWidth;
                var elem = trans.GetComponent<LinearLayoutElement>();

                if (elem != null)
                {
                    if (axis == Axis.X)
                        width = elem.scaledSize.x;
                    else if (axis == Axis.Y)
                        width = elem.scaledSize.y;
                    else
                        width = elem.scaledSize.z;
                }

                if (widths != null)
                    widths.Add(width);
                total += width;
                count++;
            }

            return total + (count - 1) * margin;
        }

        private IEnumerable GetTransformList(bool reverseOrder)
        {
            var list = new List<Transform>();

            if (reverseElementOrder)
            {
                foreach (Transform trans in transform)
                {
                    if (trans.gameObject.activeInHierarchy)
                        list.Insert(0, trans);
                }
            }
            else
            {
                foreach (Transform trans in transform)
                {
                    if (trans.gameObject.activeInHierarchy)
                        list.Add(trans);
                }
            }

            return list;
        }

#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying)
                UpdateLayout();
        }
#endif

        void Reset()
        {
            UpdateLayout();
        }

        void OnValidate()
        {
            UpdateLayout();
        }
    }
}