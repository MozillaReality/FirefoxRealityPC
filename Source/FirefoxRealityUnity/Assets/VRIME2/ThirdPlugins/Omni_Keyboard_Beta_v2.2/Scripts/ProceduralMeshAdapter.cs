// ========================================================================== //
//
//  class ProceduralMeshAdapter
//  -----
//  Purpose: Attaches the procedural mesh to the mesh filter
//
//
//  Created: 2018-12-19
//  Updated: 2018-12-19
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;

namespace Htc.Omni
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ProceduralMeshAdapter : BaseBehaviour
    {
        public ProceduralMesh proceduralMesh;

        MeshFilter meshFilter { get { return GetComponent<MeshFilter>(); } }
        /// <summary>
        /// 終究是需要一個public作為滿足其他需要動態變化的介面
        /// Start只做一次
        /// OnValidate和Reset只能在UnityEditor用
        /// (OnValidate() and Reset() is only called in editor mode.)
        /// </summary>
        public void UpdateMesh()
        {
            if (meshFilter != null)
                meshFilter.sharedMesh = proceduralMesh != null ? proceduralMesh.mesh : null;
        }

        void Start()
        {
            UpdateMesh();
        }

        // void OnValidate()
        // {
        //     UpdateMesh();
        // }

        void Reset()
        {
            UpdateMesh();
        }
#if UNTIY_EDITOR
        private void Update() {
            UpdateMesh();
        }
#endif
    }
}