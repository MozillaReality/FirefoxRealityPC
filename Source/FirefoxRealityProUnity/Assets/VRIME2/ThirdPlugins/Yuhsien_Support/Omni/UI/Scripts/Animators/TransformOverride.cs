// ========================================================================== //
//
//  class TransformOverride
//  -----
//  Purpose: A collection of local transform information
//
//
//  Created: 2018-11-16
//  Updated: 2018-11-16
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;

namespace Htc.Omni
{
        [System.Serializable]
        public class TransformOverride
        {
            public bool active = false;
            public Vector3 position = Vector3.zero;
            public Vector3 rotation = Vector3.zero;
            public Vector3 scale = Vector3.one;
        }
}