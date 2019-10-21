// ========================================================================== //
//
//  class VectorUtil
//  -----
//  Purpose: Custom editor for ScriptMacros
//
//
//  Created: 2018-12-19
//  Updated: 2018-12-19
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Htc.Omni
{
    public static class VectorUtil
    {
        public const float MinBendingCurvature = 0.01f;

        /// <summary>
        /// Transform a point from Euclidean space to polar space with specified curvature
        /// </summary>
        /// <param name="point">The point to transform</param>
        /// <param name="axis">Axis of rotation</param>
        /// <param name="focusDir">Direction to the focal point</param>
        /// <param name="curvature">Curvature, in degrees, of a unit length in Euclidean space</param>
        /// <returns></returns>
        public static Vector3 BendPoint(Vector3 point, Vector3 axis, Vector3 focusDir, float curvature)
        {
            if (Mathf.Abs(curvature) < MinBendingCurvature)
                return point;

            var upDir = Vector3.up;
            Vector3.OrthoNormalize(ref axis, ref focusDir, ref upDir);

            var radius = Mathf.Rad2Deg / curvature;
            var focus = focusDir * radius;

            var r = radius - Vector3.Dot(point, focusDir);
            var t = Vector3.Dot(point, upDir) / -radius * Mathf.Rad2Deg;
            var z = Vector3.Dot(point, axis);

            var ret = Quaternion.AngleAxis(t, axis) * -focusDir * r + focus + axis * z;

            return ret;
        }

        public static Quaternion GetCurvaRotation(Vector3 point, Vector3 axis, Vector3 focusDir, float curvature)
        {
            if (Mathf.Abs(curvature) < MinBendingCurvature)
                return Quaternion.identity;

            var upDir = Vector3.up;
            Vector3.OrthoNormalize(ref axis, ref focusDir, ref upDir);

            var radius = Mathf.Rad2Deg / curvature;
            var t = Vector3.Dot(point, upDir) / -radius * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(t, axis);
        }

        /// <summary>
        /// Transform a point from Euclidean space to polar space with specified curvature
        /// </summary>
        /// <param name="points">The points to transform</param>
        /// <param name="axis">Axis of rotation</param>
        /// <param name="focusDir">Direction to the focal point</param>
        /// <param name="curvature">Curvature, in degrees, of a unit length in Euclidean space</param>
        /// <returns></returns>
        public static void BendPoints(List<Vector3> points, List<Vector3> normals, Vector3 axis, Vector3 focusDir, float curvature)
        {
            if (Mathf.Abs(curvature) < MinBendingCurvature)
                return;

            var upDir = Vector3.up;
            Vector3.OrthoNormalize(ref axis, ref focusDir, ref upDir);

            var radius = Mathf.Rad2Deg / curvature;
            var focus = focusDir * radius;

            var count = points.Count;

            for (var i = 0; i < count; i++)
            {
                var point = points[i];
                var r = radius - Vector3.Dot(point, focusDir);
                var t = Vector3.Dot(point, upDir) / -radius * Mathf.Rad2Deg;
                var z = Vector3.Dot(point, axis);
                var rotation = Quaternion.AngleAxis(t, axis);
                points[i] = rotation * -focusDir * r + focus + axis * z;
                if (normals != null)
                    normals[i] = rotation * normals[i];
            }
        }

        public static bool Approximately(Vector3 vec1, Vector3 vec2)
        {
            return Mathf.Approximately(vec1.x, vec2.x)
            && Mathf.Approximately(vec1.y, vec2.y)
            && Mathf.Approximately(vec1.z, vec2.z);
        }

        public static float InverseLerpUnclamped(float a, float b, float t)
        {
            return (t - a) / (b - a);
        }

        public static Vector2 Lerp(Rect rect, Vector2 vec)
        {
            return new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, vec.x), Mathf.Lerp(rect.yMin, rect.yMax, vec.y));
        }

        public static Vector2 LerpUnclamped(Rect rect, Vector2 vec)
        {
            return new Vector2(Mathf.LerpUnclamped(rect.xMin, rect.xMax, vec.x), Mathf.LerpUnclamped(rect.yMin, rect.yMax, vec.y));
        }

        public static Vector2 InverseLerp(Rect rect, Vector2 vec)
        {
            return new Vector2(Mathf.InverseLerp(rect.xMin, rect.xMax, vec.x), Mathf.InverseLerp(rect.yMin, rect.yMax, vec.y));
        }

        public static Vector2 InverseLerpUnclamped(Rect rect, Vector2 vec)
        {
            return new Vector2(InverseLerpUnclamped(rect.xMin, rect.xMax, vec.x), InverseLerpUnclamped(rect.yMin, rect.yMax, vec.y));
        }

        public static Rect Lerp(Rect rect1, Rect rect2)
        {
            var ret = new Rect();
            ret.min = Lerp(rect1, rect2.min);
            ret.max = Lerp(rect1, rect2.max);
            return ret;
        }

        public static Rect LerpUnclamped(Rect rect1, Rect rect2)
        {
            var ret = new Rect();
            ret.min = LerpUnclamped(rect1, rect2.min);
            ret.max = LerpUnclamped(rect1, rect2.max);
            return ret;
        }

        public static Rect InverseLerp(Rect rect1, Rect rect2)
        {
            var ret = new Rect();
            ret.min = InverseLerp(rect1, rect2.min);
            ret.max = InverseLerp(rect1, rect2.max);
            return ret;
        }

        public static Rect InverseLerpUnclamped(Rect rect1, Rect rect2)
        {
            var ret = new Rect();
            ret.min = InverseLerpUnclamped(rect1, rect2.min);
            ret.max = InverseLerpUnclamped(rect1, rect2.max);
            return ret;
        }

        public static Vector2 InverseScale(Vector2 vec1, Vector2 vec2)
        {
            return new Vector2(vec1.x / vec2.x, vec1.y / vec2.y);
        }
    }
}