// ========================================================================== //
//
//  class BaseBehaviour
//  -----
//  Purpose: Base class of all ViveToolkit classes
//
//  Usage:
//    - Removes the script field in the inspector
//    - Implements IBaseBehaviour for mocking
//
//
//  Created: 2017-07-17
//  Updated: 2017-07-17
//
//  Copyright 2017 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using System.Collections;

namespace Htc.Omni
{
    public interface IBaseBehaviour
    {
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(IEnumerator routine);
        bool isActiveAndEnabled { get; }
    }

    public class BaseBehaviour : MonoBehaviour, IBaseBehaviour { }
}