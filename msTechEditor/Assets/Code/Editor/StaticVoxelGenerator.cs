using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using msTech.Math;
using UnityEditor;
using UnityEngine;

namespace msTech.Editor
{
    public interface IStaticVoxelGenerator
    {
        int nodesCount { get; }

        void Generate(int maxLevel);
        void Draw(int levelToShow);
        void Export(string filename);
        void Export2(string filename);
    }

    public class StaticVoxelGenerator : IStaticVoxelGenerator
    {
        public int nodesCount { get { return _nodes.Count; } }

        public StaticVoxelGenerator(int maxLevelsCount)
        {
            _polygons = new List<SPolygon>(CAPACITY_VERTICES);
            _nodes = new List<SNode>(CAPACITY_NODES);
            _hierarchyInfo = new SHierarchyLevelInfo[maxLevelsCount];
        }

        public void Generate(int maxLevel)
        {
            _maxLevel = maxLevel;
            DefineBoundsAndCollectPolygons();
            GenerateSparseOctoTree(maxLevel);
            FixEmptyNodes(_rootNode, maxLevel);
            RemoveAllInvalidNodes(maxLevel);
            CalcLeafColors(maxLevel);

            Debug.LogError("Generation done.");
        }

        public void Draw(int levelToShow)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            // Draw the real bounds
            Handles.color = Color.magenta;
            Handles.DrawWireCube(_rootCenter, _boundSize);

            // Draw the hierarchy
            Handles.color = Color.grey;
            if (_nodes.Count > 0)
                for (int i = 1; i < _nodes.Count; ++i)
                    if (_nodes[i].level == levelToShow)
                    {
                        Vector3 octoNodeHalsSize = new Vector3(_nodes[i].halfSize, _nodes[i].halfSize, _nodes[i].halfSize);
                        Vector3 octoNodeSize = octoNodeHalsSize * 2.0f;
                        Handles.DrawWireCube(_nodes[i].center, octoNodeSize);
                    }

            // Draw the root node
            if (_nodes.Count > 0)
            {
                Handles.color = Color.yellow;
                Vector3 octoRootHalfSize = new Vector3(_nodes[0].halfSize, _nodes[0].halfSize, _nodes[0].halfSize);
                Vector3 octoRootSize = octoRootHalfSize * 2.0f;
                Handles.DrawWireCube(_nodes[0].center, octoRootSize);
            }

            // Draw all polygons
            /*
            Handles.color = Color.cyan;
            for (int i = 0; i < _polygons.Count; ++i)
            {
                Handles.DrawLine(_polygons[i].points[0], _polygons[i].points[1]);
                Handles.DrawLine(_polygons[i].points[1], _polygons[i].points[2]);
                Handles.DrawLine(_polygons[i].points[2], _polygons[i].points[1]);
            }
            */
        }

        public void Export(string filename)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            UInt32 nodesCount = (UInt32)_nodes.Count;

            // Save SVO header
            bw.Write(nodesCount);
            bw.Write(_maxLevel);
            bw.Write(_rootCenter.x);
            bw.Write(_rootCenter.y);
            bw.Write(_rootCenter.z);
            bw.Write(_rootHalsSize);

            // Save nodes            
            Dictionary<SNode, long> poistionsOfNodes = new Dictionary<SNode, long>();
            Dictionary<SNode, long> positionsOfPointers = new Dictionary<SNode, long>();
            for (int i = 0; i < _nodes.Count; ++i)
            {
                SNode node = _nodes[i];

                // Save position of current node
                long currentNodePosition = ms.Position;
                poistionsOfNodes.Add(node, currentNodePosition);

                // Save node data
                byte level = (byte)node.level;
                byte mask = CalcChildrenNodesMask(node);
                bw.Write(node.colR);
                bw.Write(node.colG);
                bw.Write(node.colB);
                bw.Write(node.colA);
                bw.Write(mask);
                bw.Write(level);

                // TODO: Replace padding with some additional info
                // Padding another 4 bytes
                byte padding0 = 0xAD;
                byte padding1 = 0xDE;
                bw.Write(padding1);
                bw.Write(padding0);

                // Save mocks as a child pointer (actualy it is a position in memory stream)
                for (int j = 0; j < 8; ++j)
                    if (null != node.children[j])
                    {
                        long pointerMock = j;
                        long currentPosition = ms.Position;
                        bw.Write(pointerMock);
                        positionsOfPointers.Add(node.children[j], currentPosition);
                    }
            }

            // Restore the pointers (positions in memory stream) for pointers
            foreach (var kvp in positionsOfPointers)
            {
                SNode node = kvp.Key;
                long pointerPosition = kvp.Value;

                if (poistionsOfNodes.TryGetValue(node, out long nodePosition))
                {
                    UInt64 pointerValue = (UInt64)nodePosition;
                    ms.Position = pointerPosition;
                    bw.Write(pointerValue);
                }
                else
                {
                    Debug.LogError("Can't find a real position in MemoryStream of SNode");
                }
            }

            bw.Flush();
            File.WriteAllBytes(filename, ms.ToArray());

            Debug.LogError("Export done.");
        }

        public void Export2(string filename)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            UInt32 nodesCount = (UInt32)_nodes.Count;
            UInt32 headerEndMark = 0xFEEDFEED;

            // Save SVO header
            bw.Write(nodesCount);
            bw.Write(_maxLevel);
            bw.Write(_rootCenter.x);
            bw.Write(_rootCenter.y);
            bw.Write(_rootCenter.z);
            bw.Write(_rootHalsSize);
            //bw.Write(headerEndMark);

            UInt64 firstNodeOffset = (UInt64)ms.Position;

            // Save nodes            
            Dictionary<SNode, long> poistionsOfNodes = new Dictionary<SNode, long>();
            Dictionary<SNode, long> positionsOfPointers = new Dictionary<SNode, long>();
            for (int i = 0; i < _nodes.Count; ++i)
            {
                SNode node = _nodes[i];

                // Save position of current node
                long currentNodePosition = ms.Position;
                poistionsOfNodes.Add(node, currentNodePosition);

                // Save node data
                byte padding0 = 0xAD;
                byte padding1 = 0xDE;
                byte level = (byte)node.level;
                byte mask = CalcChildrenNodesMask(node);                
                bw.Write(level);
                bw.Write(padding1);
                bw.Write(padding0);
                bw.Write(node.colR);
                bw.Write(node.colG);
                bw.Write(node.colB);
                bw.Write(node.colA);
                bw.Write(mask);

                // Save mocks as a child pointer (actualy it is a position in memory stream)
                for (int j = 0; j < 8; ++j)
                    if (null != node.children[j])
                    {
                        long pointerMock = j;
                        long currentPosition = ms.Position;
                        bw.Write(pointerMock);
                        positionsOfPointers.Add(node.children[j], currentPosition);
                    }
            }

            // Restore the pointers (positions in memory stream) for pointers
            foreach (var kvp in positionsOfPointers)
            {
                SNode node = kvp.Key;
                long pointerPosition = kvp.Value;

                if (poistionsOfNodes.TryGetValue(node, out long nodePosition))
                {
                    UInt64 pointerValue = (UInt64)nodePosition;

                    pointerValue -= firstNodeOffset;
                    Debug.Assert(0 == pointerValue % 8, "Wrong pointer value");
                    pointerValue /= 8; // Because we have uint16_t[] in binary file

                    ms.Position = pointerPosition;
                    bw.Write(pointerValue);
                }
                else
                {
                    Debug.LogError("Can't find a real position in MemoryStream of SNode");
                }
            }

            bw.Flush();
            File.WriteAllBytes(filename, ms.ToArray());

            Debug.LogError("Export done.");
        }

        private void DefineBoundsAndCollectPolygons()
        {
            _polygons.Clear();

            _globalMinPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            _globalMaxPos = -_globalMinPos;

            // Get all SkinnedMeshRenderes and define the bounds
            SkinnedMeshRenderer[] allSkins = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
            for (int i = 0; i < allSkins.Length; ++i)
                if (allSkins[i].gameObject.activeSelf && allSkins[i].gameObject.activeInHierarchy)
                    DefineMeshBoundsAndCollectPolygons(allSkins[i].gameObject.transform, allSkins[i].sharedMesh, allSkins[i].sharedMaterial);

            // Get all mesh filter and define the bounds
            MeshFilter[] allMeshFilters = UnityEngine.Object.FindObjectsOfType<MeshFilter>();
            for (int i = 0; i < allMeshFilters.Length; ++i)
                if (allMeshFilters[i].gameObject.activeSelf && allMeshFilters[i].gameObject.activeInHierarchy && null != allMeshFilters[i].mesh)
                {
                    MeshRenderer meshRenderer = allMeshFilters[i].gameObject.GetComponent<MeshRenderer>();
                    Material mat = (null != meshRenderer) ? meshRenderer.sharedMaterial : null;
                    DefineMeshBoundsAndCollectPolygons(allMeshFilters[i].gameObject.transform, allMeshFilters[i].mesh, mat);
                }

            // Calculate the root octo node size
            _boundSize = (_globalMaxPos - _globalMinPos);
            _rootHalsSize = Mathf.Max(_boundSize.x, _boundSize.y);
            _rootHalsSize = Mathf.Max(_rootHalsSize, _boundSize.z);
            _rootHalsSize *= 0.5f;
            _rootCenter = (_globalMaxPos + _globalMinPos) * 0.5f;
        }

        private void DefineMeshBoundsAndCollectPolygons(Transform tran, Mesh mesh, Material material)
        {
            if (null != mesh && null != tran)
            {
                // Collect all polygons
                int trianglesCount = mesh.triangles.Length / 3;
                for (int i = 0; i < trianglesCount; ++i)
                {
                    int offsetA = i * 3;
                    int offsetB = offsetA + 1;
                    int offsetC = offsetA + 2;

                    Debug.Assert(offsetA < mesh.triangles.Length, "Wrong offsetA.");
                    Debug.Assert(offsetB < mesh.triangles.Length, "Wrong offsetB.");
                    Debug.Assert(offsetC < mesh.triangles.Length, "Wrong offsetC.");

                    int vertexIdA = mesh.triangles[offsetA];
                    int vertexIdB = mesh.triangles[offsetB];
                    int vertexIdC = mesh.triangles[offsetC];

                    Debug.Assert(vertexIdA < mesh.vertices.Length, "Wrong vertexIdA. Out of bounds for vertex.");
                    Debug.Assert(vertexIdB < mesh.vertices.Length, "Wrong vertexIdB. Out of bounds for vertex.");
                    Debug.Assert(vertexIdC < mesh.vertices.Length, "Wrong vertexIdC. Out of bounds for vertex.");
                    Debug.Assert(vertexIdA < mesh.uv.Length, "Wrong vertexIdA. Out of bounds for UVs.");
                    Debug.Assert(vertexIdB < mesh.uv.Length, "Wrong vertexIdB. Out of bounds for UVs.");
                    Debug.Assert(vertexIdC < mesh.uv.Length, "Wrong vertexIdC. Out of bounds for UVs.");

                    Vector3 localPosA = mesh.vertices[vertexIdA];
                    Vector3 localPosB = mesh.vertices[vertexIdB];
                    Vector3 localPosC = mesh.vertices[vertexIdC];

                    Vector3 localNorA = mesh.normals[vertexIdA];
                    Vector3 localNorB = mesh.normals[vertexIdB];
                    Vector3 localNorC = mesh.normals[vertexIdC];

                    Vector2 uvA = mesh.uv[vertexIdA];
                    Vector2 uvB = mesh.uv[vertexIdB];
                    Vector2 uvC = mesh.uv[vertexIdC];
                    
                    Vector3 posA = tran.TransformPoint(localPosA);
                    Vector3 posB = tran.TransformPoint(localPosB);
                    Vector3 posC = tran.TransformPoint(localPosC);

                    Vector3 norA = tran.TransformDirection(localNorA);
                    Vector3 norB = tran.TransformDirection(localNorB);
                    Vector3 norC = tran.TransformDirection(localNorC);

                    SPolygon polygon = new SPolygon(posA, posB, posC, norA, norB, norC, uvA, uvB, uvC, material);
                    _polygons.Add(polygon);

                    // Define bounds
                    _globalMinPos = Vector3.Min(_globalMinPos, posA);
                    _globalMaxPos = Vector3.Max(_globalMaxPos, posA);

                    _globalMinPos = Vector3.Min(_globalMinPos, posB);
                    _globalMaxPos = Vector3.Max(_globalMaxPos, posB);

                    _globalMinPos = Vector3.Min(_globalMinPos, posC);
                    _globalMaxPos = Vector3.Max(_globalMaxPos, posC);
                }
            }
        }

        private void GenerateSparseOctoTree(int maxLevel)
        {
            _nodes.Clear();

            // Define root node with zero level
            _rootNode = new SNode(null, _rootCenter, 0, _rootHalsSize);
            _rootNode.polygons = _polygons;
            _nodes.Add(_rootNode);
            ++_hierarchyInfo[0].nodesCount;

            // Report variables
            int prevLevel = -1;
            string progressBarString = "";

            // Process all nodes
            int processedNodesCountAtCurrentLevel = 0;
            int nodePos = 0;
            do
            {
                // Splitting
                SNode thisNode = _nodes[nodePos];

                // Make some report to users. Don't make them worry about slow processing speed
                int currLevel = thisNode.level;
                if (currLevel != prevLevel)
                {
                    progressBarString = "Node splitting. Level " + currLevel + ".";
                    processedNodesCountAtCurrentLevel = 0;
                    

                    // Clear the polygons lists on previous levels
                    /*
                    for (int i = nodePos - 1; i >=0; --i)
                        if (_nodes[i].level < prevLevel)
                            _nodes[i].polygons = null;
                    //*/
                    prevLevel = currLevel;
                }
                float progress = (float)processedNodesCountAtCurrentLevel / (float)_hierarchyInfo[currLevel].nodesCount;
                EditorUtility.DisplayProgressBar("Processing", progressBarString, progress);

                // Do we need to split this node or its level very high
                if (thisNode.level < maxLevel)
                {
                    int childLevel = thisNode.level + 1;
                    float childHalfSize = thisNode.halfSize * 0.5f;
                    for (int i = 0; i < 8; ++i)
                    {
                        Vector3 childCenter = thisNode.center + g_cubePointShifts[i] * childHalfSize;
                        SNode childNode = new SNode(thisNode, childCenter, childLevel, childHalfSize);

                        List<SPolygon> childPolygons = GetIntersectedPolygons(childNode, thisNode.polygons);
                        if (childPolygons.Count > 0)
                        {
                            childNode.polygons = childPolygons;
                            thisNode.children[i] = childNode;
                            _nodes.Add(childNode);

                            // Statistics
                            ++_hierarchyInfo[childLevel].nodesCount;
                            ++processedNodesCountAtCurrentLevel;
                        }
                    }
                }

                ++nodePos;
            }
            while (nodePos < _nodes.Count);

            EditorUtility.ClearProgressBar();
        }

        private void RemoveAllInvalidNodes(int maxLevel)
        {
            // Copy valid nodes to temp array
            List<SNode> newList = new List<SNode>(_nodes.Count);
            for (int i = 0; i < _nodes.Count; ++i)
                if (_nodes[i].isValid)
                    newList.Add(_nodes[i]);

            // Copy back valid nodes
            _nodes.Clear();
            for (int i = 0; i < newList.Count; ++i)
                _nodes.Add(newList[i]);

            // CRAP - Let's check again
            for (int i = 0; i < _nodes.Count; ++i)
            {
                SNode node = _nodes[i];
                bool hasAnyChild = false;
                for (int j = 0; j < 8; ++j)
                    if (null != node.children[j])
                    {
                        hasAnyChild = true;
                        break;
                    }

                if (hasAnyChild)
                    continue;

                if (node.level < maxLevel)
                {
                    Debug.LogError("Ta-Da");
                }
            }
            // end of CRAP
        }

        private void FixEmptyNodes(SNode node, int maxLevel)
        {
            for (int i = 0; i < 8; ++i)
                if (null != node.children[i])
                    FixEmptyNodes(node.children[i], maxLevel);

            bool hasAnyChild = false;
            for (int i = 0; i < 8; ++i)
                if (null != node.children[i])
                {
                    hasAnyChild = true;
                    break;
                }

            if (!hasAnyChild && node.level < maxLevel)
            {
                RemoveChildFromParent(node);
                node.isValid = false;
            }
        }

        private void RemoveChildFromParent(SNode child)
        {
            if (null == child || null == child.parent)
                return;

            for (int i = 0; i < 8; ++i)
                if (child.parent.children[i] == child)
                {
                    child.parent.children[i] = null;
                    return;
                }
        }

        private void CalcLeafColors(int maxLevel)
        {
            for (int i = 0; i < _nodes.Count; ++i)
            {
                float progress = (float)i / (float)_nodes.Count;
                EditorUtility.DisplayProgressBar("Processing", "Calculate leaf color", progress);

                SNode node = _nodes[i];
                bool hasAnyChild = false;
                for (int j = 0; j < 8; ++j)
                    if (null != node.children[j])
                    {
                        hasAnyChild = true;
                        break;
                    }

                if (hasAnyChild)
                    continue;

                // This node is a leaf
                if (node.level < maxLevel || null == node.polygons || node.polygons.Count == 0)
                {
                    Debug.LogError("Leaf node doesn't have any child or has wrong level");
                    continue;
                }

                CalcNodeColor(node);
            }

            EditorUtility.ClearProgressBar();
        }

        private void CalcNodeColor(SNode node)
        {
            Vector4 closestColor = Vector4.zero;
            float minSqrDist = float.MaxValue;
            for (int i = 0; i < node.polygons.Count; ++i)
            {
                SClosestColor closestColorToPolygon = GetPolygonClosestColor(node.polygons[i], node.center);
                if (closestColorToPolygon.sqrDist < minSqrDist)
                {
                    closestColor = closestColorToPolygon.color;
                    minSqrDist = closestColorToPolygon.sqrDist;
                }
            }

            node.colR = (byte)(closestColor.x * 255.0f);
            node.colG = (byte)(closestColor.y * 255.0f);
            node.colB = (byte)(closestColor.z * 255.0f);
            node.colA = (byte)(closestColor.w * 255.0f);
        }

        private SClosestColor GetPolygonClosestColor(SPolygon poly, Vector3 pos)
        {
            SClosestColor retColor = new SClosestColor();
            retColor.color = Vector4.zero;
            retColor.sqrDist = float.MaxValue;

            if (null == poly.material)
                return retColor;

            Texture2D texture = poly.material.mainTexture as Texture2D;
            if (null == texture)
                return retColor;

            Vector3 closestPoint = GetClosestPointToPolygon(poly, pos);

            // Caclulate barycentric coordinates
            Vector3 posA = poly.points[0];
            Vector3 posB = poly.points[1];
            Vector3 posC = poly.points[2];
            float triangleAreaABC = CalcBarycentricTriangleArea(posA, posB, posC);
            float triangleAreaABP = CalcBarycentricTriangleArea(posA, posB, closestPoint);
            float triangleAreaBCP = CalcBarycentricTriangleArea(posB, posC, closestPoint);
            float triangleAreaCAP = CalcBarycentricTriangleArea(posC, posA, closestPoint);

            float u = triangleAreaCAP / triangleAreaABC;
            float v = triangleAreaABP / triangleAreaABC;
            float w = triangleAreaBCP / triangleAreaABC;
            

            // CRAP - let's check ourself and restore the position of point by coefficients
            Vector3 restoredClosestPoint = posA * w + posB * u + posC * v;
            Vector3 deltaRestoredPos = closestPoint - restoredClosestPoint;
            if ( deltaRestoredPos.magnitude > 0.001f)
            {
                Vector3 checkClosestPoint = GetClosestPointToPolygon(poly, pos);
                //Debug.LogError("The position was restored with error.");
                int a = 0;
                ++a;
            }
            // end

            Vector2 uvA = poly.uv[0];
            Vector2 uvB = poly.uv[1];
            Vector2 uvC = poly.uv[2];
            Vector3 uv = uvA * w + uvB * u + uvC * v;

            /*
            Vector3 dirCA = poly.points[2] - poly.points[0];
            Vector3 dirBA = poly.points[1] - poly.points[0];
            Vector3 dirPA = closestPoint - poly.points[0];

            Vector3 norCA = dirCA.normalized;
            Vector3 norBA = dirBA.normalized;
            float lenProjCA = Vector3.Dot(dirPA, norCA);
            float lenProjBA = Vector3.Dot(dirPA, norBA);

            float coefCA = lenProjCA / dirCA.magnitude;
            float coefBA = lenProjBA / dirBA.magnitude;

            

            Vector2 uvCA = poly.uv[2] - poly.uv[0];
            Vector2 uvBA = poly.uv[1] - poly.uv[0];

            Vector2 uv = poly.uv[0] + uvCA * coefCA + uvBA * coefBA;
            */

            Color col = texture.GetPixelBilinear(uv.x, uv.y);

            retColor.color = new Vector4(col.r, col.g, col.b, col.a);
            retColor.sqrDist = (closestPoint - pos).sqrMagnitude;

            return retColor;
        }

        private float CalcBarycentricTriangleArea(Vector3 posA, Vector3 posB, Vector3 posC)
        {
            Vector3 vecCA = posC - posA;
            Vector3 vecBA = posB - posA;
            Vector3 cross = Vector3.Cross(vecCA, vecBA);
            return cross.magnitude * 0.5f;
        }

        private bool IsPointInsidePolygon(SPolygon poly, Vector3 pos)
        {
            for (int i = 0; i < 3; ++i)
            {
                int idA = i;
                int idB = (i + 1) % 3;
                int idC = (i + 2) % 3;

                Vector3 vecBA = poly.points[idB] - poly.points[idA];
                Vector3 vecCA = poly.points[idC] - poly.points[idA];
                Vector3 vecPA = pos - poly.points[idA];

                float absX = Mathf.Abs(vecPA.x);
                float absY = Mathf.Abs(vecPA.y);
                float absZ = Mathf.Abs(vecPA.z);

                if (absX < float.Epsilon && absY < float.Epsilon && absZ < float.Epsilon)
                    continue;

                Vector3 norBA = vecBA.normalized;
                Vector3 norCA = vecCA.normalized;
                Vector3 norPA = vecPA.normalized;

                float dotCPA = Vector3.Dot(norCA, norPA);
                float dotBPA = Vector3.Dot(norBA, norPA);

                float edgeDotThreshold = 0.99999f;

                bool isOnEdgeCA = dotCPA >= edgeDotThreshold;
                bool isOnEdgeBA = dotBPA >= edgeDotThreshold;

                if (isOnEdgeCA || isOnEdgeBA)
                    continue;

                Vector3 crossBAP = Vector3.Cross(vecBA, vecPA);
                Vector3 crossCAP = Vector3.Cross(vecCA, vecPA);
                float dot = Vector3.Dot(crossBAP, crossCAP);
                if (dot > 0.0f)
                    return false;
            }

            return true;
        }

        private Vector3 GetClosestPointToPolygon(SPolygon poly, Vector3 pos)
        {
            // Candidate points are vertex, projection to plane, and projections to edges
            List<Vector3> candidatePoints = new List<Vector3>(7);

            // Vertices of polygon
            candidatePoints.Add(poly.points[0]);
            candidatePoints.Add(poly.points[1]);
            candidatePoints.Add(poly.points[2]);

            // Try projection on plane. We add it only if projection belongs to polygon.
            Vector3 deltaPos = poly.points[0] - pos;            
            float distance = Vector3.Dot(poly.normal, deltaPos);
            Vector3 planeProjection = pos + poly.normal * distance;
            if (IsPointInsidePolygon(poly, planeProjection))
                candidatePoints.Add(planeProjection);

            // Calculate projection to edges
            for (int i = 0; i < 3; ++i)
            {
                int idA = i;
                int idB = (i + 1) % 3;
                if (HasValidEdgeProjection(pos, poly, idA, idB, out Vector3 proj))
                    candidatePoints.Add(proj);
            }

            // Find the closest point from candidates
            float minSqrDist = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;
            for (int i = 0; i < candidatePoints.Count; ++i)
            {
                if (!IsPointInsidePolygon(poly, candidatePoints[i]))
                {
                    Debug.LogError("Candidate point is outside of polygon.");
                }

                float sqrDist = (candidatePoints[i] - pos).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    closestPoint = candidatePoints[i];
                    minSqrDist = sqrDist;
                }
            }

            

            return closestPoint;
        }

        private bool HasValidEdgeProjection(Vector3 pos, SPolygon poly, int idA, int idB, out Vector3 proj)
        {
            Debug.Assert(idA >= 0 && idA < 3, "Wrong edge IdA");
            Debug.Assert(idB >= 0 && idB < 3, "Wrong edge IdB");

            Vector3 edgePosA = poly.points[idA];
            Vector3 edgePosB = poly.points[idB];
            Vector3 edgeDir = edgePosB - edgePosA;
            Vector3 edgeNor = edgeDir.normalized;

            Vector3 deltaPos = pos - edgePosA;            
            float projCoef = Vector3.Dot(edgeNor, deltaPos);
            proj = edgePosA + edgeNor * projCoef;

            Vector3 vecPA = proj - edgePosA;
            Vector3 vecPB = proj - edgePosB;

            Vector3 norPA = vecPA.normalized;
            Vector3 norPB = vecPB.normalized;
            float dot = Vector3.Dot(norPA, norPB);

            // CRAP
            float dot2 = Vector3.Dot(norPA, edgeNor);
            // end of CRAP

            if (dot < 0.0f)
            {
                if (!IsPointInsidePolygon(poly, proj))
                    Debug.LogError("Projection to edge isn't valid");

                return true;
            }

            return false;
        }

        private List<SPolygon> GetIntersectedPolygons(SNode node, List<SPolygon> parentPolygons)
        {
            List<SPolygon> childList = new List<SPolygon>(parentPolygons.Count);
            for (int i = 0; i < parentPolygons.Count; ++i)
                if (HasIntersection(node, parentPolygons[i]))
                    childList.Add(parentPolygons[i]);

            return childList;
        }

        private bool HasIntersection(SNode node, SPolygon poly)
        {
            // Test all cube points against polygon plane
            Vector3 polyNormal = Intersection.GetNormal(poly.points[0], poly.points[1], poly.points[2]);
            int countOfNegativeDistance = 0;
            for (int indexCubePoint = 0; indexCubePoint < 8; ++indexCubePoint)
            {
                Vector3 cubePoint = node.center + g_cubePointShifts[indexCubePoint] * node.halfSize;
                float distance = Intersection.GetDistancePointAndPlane(poly.points[0], polyNormal, cubePoint);
                if (distance <= -float.Epsilon )
                    ++countOfNegativeDistance;
            }

            // All points lay on the one side of the plane of polygon: negative (negativeCount==8),
            // or positive (negativeCount==0)
            if (0 == countOfNegativeDistance || 8 == countOfNegativeDistance)
                return false;

            // Test all polygon points against cube planes
            for (int indexCubeNormal = 0; indexCubeNormal < 6; ++indexCubeNormal)
            {
                Vector3 cubeNor = g_cubeNormals[indexCubeNormal];
                Vector3 cubePos = node.center + cubeNor * node.halfSize;                

                int countOfPositiveDistance = 0;
                for (int indexPolyPoint = 0; indexPolyPoint < 3; ++indexPolyPoint)
                {
                    float distance = Intersection.GetDistancePointAndPlane(cubePos, cubeNor, poly.points[indexPolyPoint]);
                    if (distance > -float.Epsilon )
                        ++countOfPositiveDistance;
                }

                // All points of polygon lay outside of cube plane. It means we don't have an intersection
                if (3 == countOfPositiveDistance)
                    return false;
            }

            // Otherwise we have an intersection
            return true;
        }

        byte CalcChildrenNodesMask(SNode node)
        {
            byte retMask = 0x00;

            if (null != node && null != node.children)
                for (int i = 0; i < 8; ++i)
                    if (null != node.children[i])
                    {
                        byte nodeMask = (byte)(1 << i);                    
                        retMask |= nodeMask;
                    }

            return retMask;
        }

        struct SHierarchyLevelInfo
        {
            public int nodesCount;
            public Mesh auxMesh;
        }

        struct SPolygon
        {
            public SPolygon(Vector3 posA, Vector3 posB, Vector3 posC,
                            Vector3 norA, Vector3 norB, Vector3 norC,
                            Vector2 uvA, Vector2 uvB, Vector2 uvC, Material mat)
            {
                points = new Vector3[3];                
                points[0] = posA;
                points[1] = posB;
                points[2] = posC;

                normals = new Vector3[3];
                normals[0] = norA;
                normals[1] = norB;
                normals[2] = norC;

                uv = new Vector2[3];
                uv[0] = uvA;
                uv[1] = uvB;
                uv[2] = uvC;

                material = mat;

                // Caclulate geometry normal for polygon
                Vector3 vecBA = posB - posA;
                Vector3 vecCA = posC - posA;
                normal = Vector3.Cross(vecBA, vecCA).normalized;
            }

            public readonly Vector3[] points;
            public readonly Vector3[] normals;
            public readonly Vector2[] uv;
            public readonly Vector3 normal;
            public readonly Material material;
        }

        class SNode
        {
            public SNode(SNode _p, Vector3 _c, int _l, float _hs)
            {
                parent = _p;
                children = new SNode[8];
                center = _c;
                level = _l;
                halfSize = _hs;

                colR = 0xFF;
                colG = 0x00;
                colB = 0xFF;
                colA = 0xFF;
            }

            public readonly SNode parent;
            public readonly SNode[] children;
            public readonly Vector3 center;
            public readonly int level;
            public readonly float halfSize;

            public byte colR;
            public byte colG;
            public byte colB;
            public byte colA;

            public List<SPolygon> polygons;

            public bool isValid = true;
        }

        private struct SClosestColor
        {
            public Vector4 color;
            public float sqrDist;
        }

        private static readonly int CAPACITY_VERTICES = 200000;
        private static readonly int CAPACITY_NODES = 1000000;

        private static readonly Vector3[] g_cubeNormals =
        {
            new Vector3( 1.0f,  0.0f,  0.0f),
            new Vector3(-1.0f,  0.0f,  0.0f),

            new Vector3( 0.0f,  1.0f,  0.0f),
            new Vector3( 0.0f, -1.0f,  0.0f),

            new Vector3( 0.0f,  0.0f,  1.0f),
            new Vector3( 0.0f,  0.0f, -1.0f),
        };

        private static readonly Vector3[] g_cubePointShifts =
        {
            new Vector3(  1.0f,  1.0f,  1.0f ),
            new Vector3(  1.0f, -1.0f,  1.0f ),
            new Vector3( -1.0f, -1.0f,  1.0f ),
            new Vector3( -1.0f,  1.0f,  1.0f ),

            new Vector3(  1.0f,  1.0f, -1.0f ),
            new Vector3(  1.0f, -1.0f, -1.0f ),
            new Vector3( -1.0f, -1.0f, -1.0f ),
            new Vector3( -1.0f,  1.0f, -1.0f ),
        };

        private readonly List<SPolygon> _polygons;
        private readonly List<SNode> _nodes;
        private readonly SHierarchyLevelInfo[] _hierarchyInfo = new SHierarchyLevelInfo[20];

        private SNode   _rootNode;
        private int     _maxLevel;

        private Vector3 _globalMinPos;
        private Vector3 _globalMaxPos;
        private Vector3 _boundSize;
        private Vector3 _rootCenter;
        private float   _rootHalsSize;

        
    }
}
