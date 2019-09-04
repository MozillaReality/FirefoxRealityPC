// ========================================================================== //
//
//  class LinearLayoutElement
//  -----
//  Purpose: Defines the size of this game object in linear layout
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
    public class LinearLayoutElement : BaseBehaviour
    {
        public Vector3 size = Vector3.one;
        public Vector3 scaledSize { get { return Vector3.Scale(size, transform.localScale); } }
    }
}