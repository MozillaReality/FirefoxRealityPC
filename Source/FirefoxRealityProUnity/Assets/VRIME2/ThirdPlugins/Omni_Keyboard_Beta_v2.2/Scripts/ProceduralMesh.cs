// ========================================================================== //
//
//  class ProceduralMesh
//  -----
//  Purpose: Represents a procedurally generated mesh
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
using UnityEngine.Profiling;

namespace Htc.Omni
{
    public abstract class ProceduralMesh : ScriptableObject
    {
        private Mesh _mesh;
        public Mesh mesh
        {
            get
            {
                if (_mesh == null)
                {
                    _mesh = new Mesh();
                    UpdateMesh();
                }

                return _mesh;
            }
        }

        private void UpdateMesh()
        {
            Profiler.BeginSample("ProceduralMesh.UpdateMesh");
            _mesh.name = name;
            OnUpdateMesh(_mesh);
            Profiler.EndSample();
        }

        protected abstract void OnUpdateMesh(Mesh mesh);

        private void OnValidate()
        {
            if (_mesh != null)
                UpdateMesh();
        }
    }
}