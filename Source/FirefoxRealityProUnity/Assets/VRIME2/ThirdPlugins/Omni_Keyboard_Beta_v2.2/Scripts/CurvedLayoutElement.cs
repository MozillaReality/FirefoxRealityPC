// ========================================================================== //
//
//  class CurvedSurfaceTransform
//  -----
//  Purpose: Drives the transform to the specified position on the curved surface
//
//  Note: This script is for edit-time object placement only. It does not do anything
//      at run-time.
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
    public class CurvedSurfaceTransform : BaseBehaviour
    {
        [Tooltip("Desired position on the curved surface")]
        public Vector3 position;

        [Tooltip("X curvature of the surface in degrees per unit length")]
        public float xCurvature;

        [Tooltip("Y curvature of the surface in degrees per unit length")]
        public float yCurvature;

        [Tooltip("Control rotation of the transform?")]
        public bool controlRotation;

        /// <summary>
        /// Update the transform according to the current setting
        /// </summary>
        public void UpdateTransform()
        {
            var pos = position;
            pos = VectorUtil.BendPoint(pos, Vector3.right, Vector3.back, yCurvature);
            pos = VectorUtil.BendPoint(pos, Vector3.up, Vector3.back, xCurvature);
            transform.localPosition = pos;

            if (controlRotation)
            {
                var rotY = VectorUtil.GetCurvaRotation(position, Vector3.right, Vector3.back, yCurvature);
                var rotX = VectorUtil.GetCurvaRotation(position, Vector3.up, Vector3.back, xCurvature);
                transform.localRotation = rotY * rotX;
            }
        }

        void OnValidate()
        {
            UpdateTransform();
        }
    }
}