// ========================================================================== //
//
//  class MeshFactory
//  -----
//  Purpose: Creates procedural meshes
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
    public static class MeshFactory
    {
        public class MeshBuffer
        {
            public List<Vector3> vertices;
            public List<Vector3> normals;
            public List<Vector2> uv;
            public List<int> triangles;

            public int vertCount { get { return vertices.Count; } }
            public int triCount { get { return triangles.Count / 3; } }

            public bool hasNormals { get { return normals != null; } }
            public bool hasUV { get { return uv != null; } }

            public void ApplyToMesh(Mesh mesh)
            {
                mesh.Clear();
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();

                if (normals != null)
                    mesh.normals = normals.ToArray();
                if (uv != null)
                    mesh.uv = uv.ToArray();
            }

            public MeshBuffer(bool hasNormals = false, bool hasUV = false)
            {
                vertices = new List<Vector3>();
                triangles = new List<int>();

                if (hasNormals)
                    normals = new List<Vector3>();
                if (hasUV)
                    uv = new List<Vector2>();
            }
        }

        /// <summary>
        /// Creates a panel mesh
        /// </summary>
        /// <param name="mesh">Existing mesh object to update</param>
        /// <param name="param">Parameters to generate the mesh</param>
        public static void CreatePanel(Mesh mesh, ProceduralPanel param)
        {
            if (param.widthDivision <= 0 || param.heightDivision <= 0 || param.thicknessDivision <= 0)
                return;

            var buffer = new MeshBuffer(param.hasNormals, param.hasUV);

            var width = param.width;
            var height = param.height;
            var thickness = param.thickness;

            var hasRoundedCorners = param.cornerDivision > 0 && param.cornerRadius > 0;
            var hasFrontBevel = param.frontBevelDivision > 0 && param.frontBevel > 0;
            var hasBackBevel = param.backBevelDividion > 0 && param.backBevel > 0;

            int wDiv = param.widthDivision;
            var hDiv = param.heightDivision;
            var tDiv = param.thicknessDivision;
            var cDiv = hasRoundedCorners ? param.cornerDivision : 0;
            var fbDiv = hasFrontBevel ? param.frontBevelDivision : 0;
            var bbDiv = hasBackBevel ? param.backBevelDividion : 0;

            var zFront = thickness * -0.5f;
            var zBack = thickness * 0.5f;

            var cornerRadius = hasRoundedCorners ? param.cornerRadius : 0f;
            var fbRadius = hasFrontBevel ? param.frontBevel : 0;
            var bbRadius = hasBackBevel ? param.backBevel : 0;

            // Add front face verts
            var faceVertCount = (wDiv + 1) * (hDiv + 1);
            var sideLoopVertCount = hasRoundedCorners ? (wDiv * 2 + hDiv * 2 + cDiv * 4 + 1) : ((wDiv + hDiv + 2) * 2);

            var frontFaceOffset = cornerRadius > fbRadius ? cornerRadius : fbRadius;
            var frontFaceRect = CreateCenteredRect(width - frontFaceOffset * 2, height - frontFaceOffset * 2);
            var frontUVRect = ScaleUVRect(CreateCenteredRect(width, height), param.frontFaceUV);
            AddPlaneVerts(buffer, frontFaceRect, wDiv, hDiv, zFront, Vector3.back, frontUVRect);

            // Add back face verts
            var backFaceOffset = cornerRadius > bbRadius ? cornerRadius : bbRadius;
            var backFaceRect = CreateCenteredRect(width - backFaceOffset * 2, height - backFaceOffset * 2);
            var backUVRect = ScaleUVRect(CreateCenteredRect(width, height, true), param.backFaceUV);
            AddPlaneVerts(buffer, backFaceRect, wDiv, hDiv, zBack, Vector3.forward, backUVRect);

            // Add side verts
            var fullRect = CreateCenteredRect(width, height);
            var loopUVRect = new Rect(-thickness * 0.5f, 0, thickness, 1);
            loopUVRect = ScaleUVRect(loopUVRect, param.sideUV);
            var frontBevelUVRect = param.mapFrontBevelUVToSide ? loopUVRect : frontUVRect;
            var backBevelUVRect = param.mapBackBevelUVToSide ? loopUVRect : backUVRect;

            var loopDivs = new Vector3Int(wDiv, hDiv, cDiv);
            var loopUVScale = new Vector3(width - cornerRadius * 2f, height - cornerRadius * 2f, cornerRadius * Mathf.PI / 2f);

            var zFlatStart = zFront + fbRadius;
            var zFlatEnd = zBack - bbRadius;

            // -- Front bevel
            if (hasFrontBevel)
            {
                var cornerDiff = cornerRadius < fbRadius ? cornerRadius : fbRadius;

                if (param.mapFrontBevelUVToSide)
                {
                    var rect = CreateCenteredRect(width - fbRadius * 2, height - fbRadius * 2);
                    AddRectLoopVerts(buffer, rect, cornerRadius - cornerDiff, loopDivs, zFront, 0, frontUVRect, loopUVScale, false);
                }

                for (var i = 0; i < fbDiv; i++)
                {
                    var ratio = i / (float)fbDiv;
                    var radiusOffset = fbRadius * (1 - Mathf.Sin(ratio * Mathf.PI / 2));
                    var cornerOffset = cornerDiff * (1 - Mathf.Sin(ratio * Mathf.PI / 2));
                    var z = zFront + fbRadius * (1 - Mathf.Cos(ratio * Mathf.PI / 2));

                    var rect = CreateCenteredRect(width - radiusOffset * 2, height - radiusOffset * 2);
                    AddRectLoopVerts(buffer, rect, cornerRadius - cornerOffset, loopDivs, z, ratio / 2, frontBevelUVRect, loopUVScale, param.mapFrontBevelUVToSide);
                }

                if (!param.mapFrontBevelUVToSide)
                    AddRectLoopVerts(buffer, fullRect, cornerRadius, loopDivs, zFlatStart, 0.5f, frontUVRect, loopUVScale, false);
            }
            else
                AddRectLoopVerts(buffer, fullRect, cornerRadius, loopDivs, zFlatStart, 0, frontUVRect, loopUVScale, false);


            // -- Constant thickness sections
            for (var it = 0; it <= tDiv; it++)
            {
                var z = Mathf.Lerp(zFlatStart, zFlatEnd, it / (float)tDiv);
                AddRectLoopVerts(buffer, fullRect, cornerRadius, loopDivs, z, 0.5f, loopUVRect, loopUVScale, true);
            }

            // -- Back bevel
            if (hasBackBevel)
            {
                var cornerDiff = cornerRadius < bbRadius ? cornerRadius : bbRadius;

                if (!param.mapBackBevelUVToSide)
                    AddRectLoopVerts(buffer, fullRect, cornerRadius, loopDivs, zFlatEnd, 0.5f, backUVRect, loopUVScale, false);

                for (var i = 1; i <= bbDiv; i++)
                {
                    var ratio = i / (float)bbDiv;
                    var radiusOffset = bbRadius * (1 - Mathf.Cos(ratio * Mathf.PI / 2));
                    var cornerOffset = cornerDiff * (1 - Mathf.Cos(ratio * Mathf.PI / 2));
                    var z = zBack - bbRadius * (1 - Mathf.Sin(ratio * Mathf.PI / 2));

                    var rect = CreateCenteredRect(width - radiusOffset * 2, height - radiusOffset * 2);
                    AddRectLoopVerts(buffer, rect, cornerRadius - cornerOffset, loopDivs, z, ratio / 2 + 0.5f, backBevelUVRect, loopUVScale, param.mapBackBevelUVToSide);
                }

                if (param.mapBackBevelUVToSide)
                {
                    var rect = CreateCenteredRect(width - bbRadius * 2, height - bbRadius * 2);
                    AddRectLoopVerts(buffer, rect, cornerRadius - cornerDiff, loopDivs, zBack, 1, backUVRect, loopUVScale, false);
                }
            }
            else
                AddRectLoopVerts(buffer, fullRect, cornerRadius, loopDivs, zFlatEnd, 1, backUVRect, loopUVScale, false);

            // Add front face tris
            AddPlaneTriangles(buffer.triangles, 0, wDiv, hDiv, true);

            // Connect front face with side
            if (hasRoundedCorners)
                ConnectPlaneAndRectLoop(buffer.triangles, 0, faceVertCount * 2, wDiv, hDiv, cDiv, false);

            // Add side tris
            var sideLoopStart = faceVertCount * 2 + (hasFrontBevel ? 0 : sideLoopVertCount);
            var sideStripCount = fbDiv + bbDiv + tDiv;
            var iSide = 0;
            while (iSide < sideStripCount)
            {
                if ((hasFrontBevel && iSide == 0 && param.mapFrontBevelUVToSide)
                || (hasFrontBevel && iSide == fbDiv && !param.mapFrontBevelUVToSide)
                || (hasBackBevel && iSide == fbDiv + tDiv && !param.mapBackBevelUVToSide))
                    sideLoopStart += sideLoopVertCount;

                if (hasRoundedCorners)
                {
                    AddQuadStrip(buffer.triangles, sideLoopStart, 1, sideLoopStart + sideLoopVertCount, 1, sideLoopVertCount, true);
                }
                else
                {
                    var start = sideLoopStart;
                    AddQuadStrip(buffer.triangles, start, 1, start + sideLoopVertCount, 1, wDiv + 1, false);
                    start += wDiv + 1;
                    AddQuadStrip(buffer.triangles, start, 1, start + sideLoopVertCount, 1, hDiv + 1, false);
                    start += hDiv + 1;
                    AddQuadStrip(buffer.triangles, start, 1, start + sideLoopVertCount, 1, wDiv + 1, false);
                    start += wDiv + 1;
                    AddQuadStrip(buffer.triangles, start, 1, start + sideLoopVertCount, 1, hDiv + 1, false);
                }

                sideLoopStart += sideLoopVertCount;
                iSide++;
            }

            if (!hasBackBevel || param.mapBackBevelUVToSide)
                sideLoopStart += sideLoopVertCount;

            // Connect side with back face
            if (hasRoundedCorners)
                ConnectPlaneAndRectLoop(buffer.triangles, faceVertCount, sideLoopStart, wDiv, hDiv, cDiv, true);

            // Add back face tris
            AddPlaneTriangles(buffer.triangles, faceVertCount, wDiv, hDiv, false);

            // Apply extrude direction
            var extrudeDir = param.extrudeDirection.normalized;
            if (!VectorUtil.Approximately(extrudeDir, Vector3.forward))
            {
                var rotation = Quaternion.FromToRotation(Vector3.forward, extrudeDir);
                var count = buffer.vertices.Count;
                for (var i = 0; i < count; i++)
                    buffer.vertices[i] = rotation * buffer.vertices[i];

                if (buffer.hasNormals)
                {
                    for (var i = 0; i < count; i++)
                        buffer.normals[i] = rotation * buffer.normals[i];
                }
            }

            // Apply bending
            VectorUtil.BendPoints(buffer.vertices, buffer.normals, Vector3.right, Vector3.back, param.yCurvature);
            VectorUtil.BendPoints(buffer.vertices, buffer.normals, Vector3.up, Vector3.back, param.xCurvature);

            // Apply
            buffer.ApplyToMesh(mesh);
        }

        private static void AddPlaneVerts(MeshBuffer buffer, Rect rect, int wDiv, int hDiv, float z, Vector3 normal, Rect uv)
        {
            if (wDiv <= 0 || hDiv <= 0)
                throw new System.ArgumentException("div must be greater than 0");

            for (var ih = 0; ih <= hDiv; ih++)
            {
                var y = Mathf.Lerp(rect.yMin, rect.yMax, ih / (float)hDiv);
                for (var iw = 0; iw <= wDiv; iw++)
                {
                    var x = Mathf.Lerp(rect.xMin, rect.xMax, iw / (float)wDiv);
                    buffer.vertices.Add(new Vector3(x, y, z));
                    if (buffer.hasNormals)
                        buffer.normals.Add(normal);
                    if (buffer.hasUV)
                        buffer.uv.Add(ScaleUV(uv, new Vector2(x, y)));
                }
            }
        }

        /// <summary>
        /// Generates a vertex loop representing a rounded rectangle
        /// </summary>
        /// <param name="buffer">Buffer to store the data</param>
        /// <param name="rect">the outline of the rectangle</param>
        /// <param name="cornerRadius">Corner radius</param>
        /// <param name="divs">Divisions - (width div, height div, corner div)</param>
        /// <param name="z">Z position of the rect loop</param>
        /// <param name="normalDir">Normal directions, 0: -Z, 1: +Z</param>
        /// <param name="uvRect">UV mapping rectagle</param>
        /// <param name="LoopUVScale">Defines scaling of UV on (width, height, corner)</param>
        /// <param name="loopingUV">True: UV.u will be mapped to z, UV.v will be mapped along the loop, False: UV will be mapped from XY plane </param>
        private static void AddRectLoopVerts(MeshBuffer buffer, Rect rect, float cornerRadius, Vector3Int divs, float z, float normalDir, Rect uvRect, Vector3 loopUVScale, bool loopingUV)
        {
            var wDiv = divs.x;
            var hDiv = divs.y;
            var cDiv = divs.z;

            if (wDiv <= 0 || hDiv <= 0)
                throw new System.ArgumentException("div must be greater than 0");

            var hasRoundedCorners = cDiv > 0;

            var normalR = Mathf.Sin(normalDir * Mathf.PI);
            var normalZ = -Mathf.Cos(normalDir * Mathf.PI);

            if (cDiv <= 0)
                cornerRadius = 0;

            // Bottom edge
            var start = new Vector3(rect.xMin + cornerRadius, rect.yMin, z);
            var end = new Vector3(rect.xMax - cornerRadius, rect.yMin, z);
            var normal = new Vector3(0, -normalR, normalZ);
            AddLinearVerts(buffer, start, end, wDiv, normal, hasRoundedCorners);

            // Bottom-right corner
            if (hasRoundedCorners)
            {
                start = end;
                end = new Vector3(rect.xMax, rect.yMin + cornerRadius, z);
                var anchor = new Vector3(rect.xMax - cornerRadius, rect.yMin + cornerRadius, z);
                AddArcVerts(buffer, anchor, start, end, cDiv, normal, true);
            }

            // Right edge
            start = end;
            end = new Vector3(rect.xMax, rect.yMax - cornerRadius, z);
            normal = new Vector3(normalR, 0, normalZ);
            AddLinearVerts(buffer, start, end, hDiv, normal, hasRoundedCorners);

            // Top-right corner
            if (hasRoundedCorners)
            {
                start = end;
                end = new Vector3(rect.xMax - cornerRadius, rect.yMax, z);
                var anchor = new Vector3(rect.xMax - cornerRadius, rect.yMax - cornerRadius, z);
                AddArcVerts(buffer, anchor, start, end, cDiv, normal, true);
            }

            // Top edge
            start = end;
            end = new Vector3(rect.xMin + cornerRadius, rect.yMax, z);
            normal = new Vector3(0, normalR, normalZ);
            AddLinearVerts(buffer, start, end, wDiv, normal, hasRoundedCorners);

            // Top-left corner
            if (hasRoundedCorners)
            {
                start = end;
                end = new Vector3(rect.xMin, rect.yMax - cornerRadius, z);
                var anchor = new Vector3(rect.xMin + cornerRadius, rect.yMax - cornerRadius, z);
                AddArcVerts(buffer, anchor, start, end, cDiv, normal, true);
            }

            // Left edge
            start = end;
            end = new Vector3(rect.xMin, rect.yMin + cornerRadius, z);
            normal = new Vector3(-normalR, 0, normalZ);
            AddLinearVerts(buffer, start, end, hDiv, normal, hasRoundedCorners);

            // Bottom-left corner
            if (hasRoundedCorners)
            {
                start = end;
                end = new Vector3(rect.xMin + cornerRadius, rect.yMin, z);
                var anchor = new Vector3(rect.xMin + cornerRadius, rect.yMin + cornerRadius, z);
                AddArcVerts(buffer, anchor, start, end, cDiv, normal, false);
            }

            // Generate UV
            if (buffer.hasUV)
            {
                if (loopingUV)
                {

                    var totalLen = (loopUVScale.x + loopUVScale.y + loopUVScale.z * 2f) * 2f;
                    var len = 0f;

                    var isCorner = false;
                    var isHeight = false;

                    var step = loopUVScale.x;
                    var count = divs.x;

                    // var vertStart = buffer.vertCount - buffer.uv.Count;

                    for (var iSide = 0; iSide < 8; iSide++)
                    {
                        for (var i = 0; i < count; i++)
                        {
                            buffer.uv.Add(ScaleUV(uvRect, new Vector2(z, len / totalLen)));
                            len += step;
                        }

                        isCorner = !isCorner;
                        if (isCorner)
                        {
                            step = divs.z > 0 ? loopUVScale.z / divs.z : 0;
                            count = divs.z > 0 ? divs.z : 1;
                        }
                        else
                        {
                            isHeight = !isHeight;
                            if (isHeight)
                            {
                                step = loopUVScale.y / divs.y;
                                count = divs.y;
                            }
                            else
                            {
                                step = loopUVScale.x / divs.x;
                                count = divs.x;
                            }
                        }
                    }

                    if (divs.z > 0)
                        buffer.uv.Add(ScaleUV(uvRect, new Vector2(z, 1)));

                }
                else
                {
                    var i = buffer.uv.Count;
                    var count = buffer.vertCount;
                    while (i < count)
                    {
                        var vert = buffer.vertices[i];
                        buffer.uv.Add(ScaleUV(uvRect, new Vector2(vert.x, vert.y)));
                        i++;
                    }
                }

                if (buffer.vertCount != buffer.uv.Count)
                    Debug.LogError("Vertex count mismatch, vert: " + buffer.vertCount + ", uv: " + buffer.uv.Count);
            }
        }

        private static void AddPlaneTriangles(List<int> triangles, int start, int wDiv, int hDiv, bool reversed)
        {
            for (var ih = 0; ih < hDiv; ih++)
            {
                for (var iw = 0; iw < wDiv; iw++)
                {
                    var v0 = start + ih * (wDiv + 1) + iw;
                    var v1 = start + ih * (wDiv + 1) + iw + 1;
                    var v2 = start + (ih + 1) * (wDiv + 1) + iw + 1;
                    var v3 = start + (ih + 1) * (wDiv + 1) + iw;
                    AddQuad(triangles, v0, v1, v2, v3, reversed);
                }
            }
        }

        private static void AddQuadStrip(List<int> triangles, int line1Start, int line1Step, int line2Start, int line2Step, int vertCountPerLine, bool loop, bool reversed = false)
        {
            var line1Vert = line1Start;
            var line2Vert = line2Start;

            for (var i = 0; i < vertCountPerLine - 1; i++)
            {
                AddQuad(triangles, line1Vert, line1Vert + line1Step, line2Vert + line2Step, line2Vert, reversed);
                line1Vert += line1Step;
                line2Vert += line2Step;
            }

            if (loop)
                AddQuad(triangles, line1Vert, line1Start, line2Start, line2Vert);
        }

        private static void ConnectPlaneAndRectLoop(List<int> triangles, int planeStart, int rectLoopStart, int wDiv, int hDiv, int cornerDiv, bool reversed)
        {
            if (cornerDiv <= 0)
                return;

            // Connect Bottom edge
            var planeVert = planeStart;
            var loopVert = rectLoopStart;

            // Bottom edge
            AddQuadStrip(triangles, planeVert, 1, loopVert, 1, wDiv + 1, false, reversed);
            planeVert += wDiv;
            loopVert += wDiv;

            // Bottom-right corner
            AddFan(triangles, planeVert, loopVert, cornerDiv, !reversed);
            loopVert += cornerDiv;

            // Right edge
            AddQuadStrip(triangles, planeVert, wDiv + 1, loopVert, 1, hDiv + 1, false, reversed);
            planeVert += hDiv * (wDiv + 1);
            loopVert += hDiv;

            // Top-right corner
            AddFan(triangles, planeVert, loopVert, cornerDiv, !reversed);
            loopVert += cornerDiv;

            // Top edge
            AddQuadStrip(triangles, planeVert, -1, loopVert, 1, wDiv + 1, false, reversed);
            planeVert -= wDiv;
            loopVert += wDiv;

            // Top-left corner
            AddFan(triangles, planeVert, loopVert, cornerDiv, !reversed);
            loopVert += cornerDiv;

            // Left edge
            AddQuadStrip(triangles, planeVert, -wDiv - 1, loopVert, 1, hDiv + 1, false, reversed);
            planeVert = planeStart;
            loopVert += hDiv;

            // Bottom-left corner
            AddFan(triangles, planeVert, loopVert, cornerDiv, !reversed);
        }

        private static void AddFan(List<int> triangles, int anchor, int start, int triCount, bool reversed)
        {
            for (var i = 0; i < triCount; i++)
                AddTriangle(triangles, anchor, start + i, start + i + 1, reversed);
        }

        private static void AddQuad(List<int> triangles, int v0, int v1, int v2, int v3, bool reversed = false)
        {
            triangles.Add(v0);
            triangles.Add(reversed ? v3 : v1);
            triangles.Add(v2);

            triangles.Add(v2);
            triangles.Add(reversed ? v1 : v3);
            triangles.Add(v0);
        }

        private static void AddTriangle(List<int> triangles, int v0, int v1, int v2, bool reversed = false)
        {
            triangles.Add(v0);
            triangles.Add(reversed ? v2 : v1);
            triangles.Add(reversed ? v1 : v2);
        }

        private static void AddLinearVerts(MeshBuffer buffer, Vector3 start, Vector3 end, int div, Vector3 normal, bool skipLast = false)
        {
            if (div <= 0)
                throw new System.ArgumentException("div must be greater than 0");

            var i = 0;
            var iMax = skipLast ? div - 1 : div;

            while (i <= iMax)
            {
                buffer.vertices.Add(Vector3.Lerp(start, end, i / (float)div));
                if (buffer.hasNormals)
                    buffer.normals.Add(normal);
                i++;
            }
        }

        private static void AddArcVerts(MeshBuffer buffer, Vector3 anchor, Vector3 start, Vector3 end, int div, Vector3 normal, bool skipLast = false)
        {
            if (div <= 0)
                throw new System.ArgumentException("div must be greater than 0");

            var i = 0;
            var iMax = skipLast ? div - 1 : div;

            var start2 = start - anchor;
            var end2 = end - anchor;

            var rotation = Quaternion.FromToRotation(start2, end2);

            while (i <= iMax)
            {
                var ratio = i / (float)div;
                var rot = Quaternion.Slerp(Quaternion.identity, rotation, ratio);
                buffer.vertices.Add(anchor + Vector3.Slerp(start2, end2, ratio));
                if (buffer.hasNormals)
                    buffer.normals.Add(rot * normal);
                i++;
            }
        }

        private static Rect CreateCenteredRect(float width, float height, bool flipX = false, bool flipY = false)
        {
            if (flipX && flipY)
                return new Rect(width * 0.5f, height * 0.5f, -width, -height);
            else if (flipX)
                return new Rect(width * 0.5f, height * -0.5f, -width, height);
            else if (flipY)
                return new Rect(width * -0.5f, height * 0.5f, width, -height);
            else
                return new Rect(width * -0.5f, height * -0.5f, width, height);
        }

        /// <summary>
        /// Returns a Rect rect that InverseLerp(rect, vec) == LerpUnclamped(rect2, InverseLerp(rect1, vec))
        /// </summary>
        /// <param name="rect1">First rect</param>
        /// <param name="rect2">Second rect</param>
        /// <returns></returns>
        public static Rect ScaleUVRect(Rect rect, Rect scale)
        {
            var xScaleIsZero = Mathf.Approximately(scale.size.x, 0);
            var yScaleIsZero = Mathf.Approximately(scale.size.y, 0);

            var del = Vector2.zero;
            del.x = xScaleIsZero ? 0 : rect.size.x / scale.size.x;
            del.y = xScaleIsZero ? 0 : rect.size.y / scale.size.y;

            var ret = new Rect();
            ret.xMin = xScaleIsZero ? scale.xMin : rect.xMin - scale.xMin * del.x;
            ret.yMin = yScaleIsZero ? scale.yMin : rect.yMin - scale.yMin * del.y;

            ret.max = ret.min + del;

            return ret;
        }

        /// <summary>
        /// Returns a Rect rect that InverseLerp(rect, vec) == LerpUnclamped(rect2, InverseLerp(rect1, vec))
        /// </summary>
        /// <param name="rect1">First rect</param>
        /// <param name="rect2">Second rect</param>
        /// <returns></returns>
        public static Vector2 ScaleUV(Rect rect, Vector2 vec)
        {
            var ret = Vector2.zero;
            ret.x = Mathf.Approximately(rect.width, 0) ? rect.xMin : VectorUtil.InverseLerpUnclamped(rect.xMin, rect.xMax, vec.x);
            ret.y = Mathf.Approximately(rect.height, 0) ? rect.yMin : VectorUtil.InverseLerpUnclamped(rect.yMin, rect.yMax, vec.y);

            return ret;
        }
    }
}