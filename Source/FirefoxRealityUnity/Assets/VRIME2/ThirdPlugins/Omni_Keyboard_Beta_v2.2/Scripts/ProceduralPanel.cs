// ========================================================================== //
//
//  class ProceduralPanel
//  -----
//  Purpose: Represents a procedurally generated panel with bevels and rounded corners
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
    public class ProceduralPanel : ProceduralMesh
    {
        [Header("Basic")]
        public float width = 1f;
        public float height = 1f;
        public float thickness = 0.1f;

        public float cornerRadius = 0f;
        public float frontBevel = 0f;
        public float backBevel = 0f;

        [Header("Tessellation")]
        public int widthDivision = 1;
        public int heightDivision = 1;
        public int thicknessDivision = 1;

        public int cornerDivision = 0;
        public int frontBevelDivision = 0;
        public int backBevelDividion = 0;

        [Header("UV Mapping")]
        public Rect frontFaceUV = new Rect(0, 0, 0.2f, 1f);
        public Rect sideUV = new Rect(0.2f, 0, 0.6f, 1f);
        public Rect backFaceUV = new Rect(0.8f, 0, 0.2f, 1f);
        public bool mapFrontBevelUVToSide = false;
        public bool mapBackBevelUVToSide = true;

        [Header("Transforms")]
        public Vector3 extrudeDirection = Vector3.forward;

        [Tooltip("X direction curvature in degrees per unit length")]
        public float xCurvature = 0f;

        [Tooltip("Y direction curvature in degrees per unit length")]
        public float yCurvature = 0f;

        [Header("Generation")]
        public bool hasNormals = true;
        public bool hasUV = true;

        public bool hasRoundedCorners { get { return cornerDivision > 0 && cornerRadius != 0f; } }

        protected override void OnUpdateMesh(Mesh mesh)
        {
            MeshFactory.CreatePanel(mesh, this);
        }
    }
}