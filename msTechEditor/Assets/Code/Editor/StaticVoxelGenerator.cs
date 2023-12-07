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
            DefineBoundsAndCollectPolygons();
            GenerateSparseOctoTree(maxLevel);
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

            // Save SVO header
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
                byte colR = 0xFF;
                byte colG = 0x7F;
                byte colB = 0xFF;
                byte mask = CalcChildrenNodesMask(node);
                bw.Write(colR);
                bw.Write(colG);
                bw.Write(colB);
                bw.Write(mask);

                // TODO: Replace padding with some additional info
                // Padding another 4 bytes
                UInt32 padding = 0xEFBEADDE;
                bw.Write(padding);                

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

                    Vector2 uvA = mesh.uv[vertexIdA];
                    Vector2 uvB = mesh.uv[vertexIdB];
                    Vector2 uvC = mesh.uv[vertexIdC];
                    
                    Vector3 posA = tran.TransformPoint(localPosA);
                    Vector3 posB = tran.TransformPoint(localPosB);
                    Vector3 posC = tran.TransformPoint(localPosC);

                    SPolygon polygon = new SPolygon(posA, posB, posC, uvA, uvB, uvC, material);
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
            SNode rootNode = new SNode(null, _rootCenter, 0, _rootHalsSize);
            rootNode.polygons = _polygons;
            _nodes.Add(rootNode);
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
                    prevLevel = currLevel;

                    // Clear the polygons lists on previous levels
                    for (int i = nodePos - 1; i >=0; --i)
                        if (_nodes[i].level < currLevel)
                            _nodes[i].polygons = null;
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
            public SPolygon(Vector3 posA, Vector3 posB, Vector3 posC, Vector2 uvA, Vector2 uvB, Vector2 uvC, Material mat)
            {
                points = new Vector3[3];                
                points[0] = posA;
                points[1] = posB;
                points[2] = posC;

                uv = new Vector2[3];
                uv[0] = uvA;
                uv[1] = uvB;
                uv[2] = uvC;

                material = mat;
            }

            public readonly Vector3[] points;
            public readonly Vector2[] uv;
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
            }

            public readonly SNode parent;
            public readonly SNode[] children;
            public readonly Vector3 center;
            public readonly int level;
            public readonly float halfSize;

            public List<SPolygon> polygons;
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

        private Vector3 _globalMinPos;
        private Vector3 _globalMaxPos;
        private Vector3 _boundSize;
        private Vector3 _rootCenter;
        private float _rootHalsSize;

        
    }
}
