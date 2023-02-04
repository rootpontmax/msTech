using System;
using msTech.Data;
using msTech.Render;
using UnityEngine;

namespace msTech.Editor
{
    public interface ILayoutGO : IDisposable
    {
        void Tick();
        void Refresh(Layout layout);
    }

    public class LayoutGO : ILayoutGO
    {
        public LayoutGO()
        {
            _root = new GameObject("Root");
        }

        public void Dispose()
        {
            if ( null != _root )
                GameObject.DestroyImmediate(_root);
        }

        public void Tick()
        {
            if ( null != _root )
            {
                _root.transform.position = Vector3.zero;
                _root.transform.rotation = Quaternion.identity;
            }
        }

        public void Refresh(Layout layout)
        {
            ClearChildren(_root);

            if ( null == layout || null == layout.elements || 0 == layout.elements.Length )
                return;

            DefineFrameSize(layout);
            for ( int i = 0; i < layout.elements.Length; ++i )
            {
                UIElement item = layout.elements[i];
                GameObject childGO = new GameObject(item.name);
                childGO.transform.parent = _root.transform;
                childGO.transform.position = GetAnchorPosition(item, i);//new Vector3(item.offsetX, item.offsetY, (float)i);
                CreateMesh(childGO, item);
                CreateAuxRect(childGO, item);
            }
        }

        private void ClearChildren(GameObject go)
        {
            int childCount = go.transform.childCount;
            while ( childCount > 0 )
            {
                GameObject.DestroyImmediate(go.transform.GetChild(0).gameObject);
                childCount = go.transform.childCount;
            }
        }

        private void DefineFrameSize(Layout layout)
        {
            // Draw frame always
            _sizeX = 1.0f;
            _sizeY = 1.0f;
            if ( LayoutOrientation.Portrait == layout.orientation )
            {
                _sizeX = 1.0f;
                _sizeY = layout.aspect;
            }
            else if ( LayoutOrientation.Landscape == layout.orientation )
            {
                _sizeX = layout.aspect;
                _sizeY = 1.0f;
            }
            else
            {
                Debug.LogError("Unknown type of Layout Orientation");
            }
        }

        private Vector3 GetAnchorPosition(UIElement item, float zOffset)
        {
            float x = item.offsetX;
            float y = item.offsetY;

            if ( HorizontalAnchor.Left == item.anchorH )
                x = -_sizeX + item.offsetX;
            else if( HorizontalAnchor.Right == item.anchorH )
                x = _sizeX - item.offsetX;

            if ( VerticalAnchor.Top == item.anchorV )
                y = _sizeY - item.offsetY;
            else if( VerticalAnchor.Bottom == item.anchorV )
                y = -_sizeY + item.offsetY;

            return new Vector3(x, y, zOffset);
        }

        private void CreateMesh(GameObject go, UIElement item)
        {
            if ( null == item.normalSprite )
                return;

            if ( !item.isVisible )
                return;

            Shader shader = Shader.Find("UI/Default");
            Material material = new Material(shader);
            material.mainTexture = item.normalSprite;

            int verticesCount = 4;
            int indexesCount = 6;
            Vector3[] pos = new Vector3[verticesCount];
            Vector2[] uvs = new Vector2[verticesCount];
            int[] indexes = new int[indexesCount];

            float halfSizeX = item.sizeX * 0.5f;
            float halfSizeY = item.sizeY * 0.5f;

            // Position
            pos[0] = new Vector3(-halfSizeX, -halfSizeY, 0.0f);
            pos[1] = new Vector3(-halfSizeX,  halfSizeY, 0.0f);
            pos[2] = new Vector3( halfSizeX,  halfSizeY, 0.0f);
            pos[3] = new Vector3( halfSizeX, -halfSizeY, 0.0f);

            // Resolve mirror
            /*
            Rect pixelRect = data.sprite.rect;
            float minX = data.hasMirrorX ? pixelRect.max.x : pixelRect.min.x;
            float maxX = data.hasMirrorX ? pixelRect.min.x : pixelRect.max.x;
            float minY = data.hasMirrorY ? pixelRect.max.y : pixelRect.min.y;
            float maxY = data.hasMirrorY ? pixelRect.min.y : pixelRect.max.y;
            */
                
            float minU = 0.0f;
            float maxU = 1.0f;
            float minV = 0.0f;
            float maxV = 1.0f;

            // Texture coordinates
            uvs[0].x = minU;
            uvs[1].x = minU;
            uvs[2].x = maxU;
            uvs[3].x = maxU;

            uvs[0].y = minV;
            uvs[1].y = maxV;
            uvs[2].y = maxV;
            uvs[3].y = minV;

            // Indexes for two triangles
            indexes[0] = 0;
            indexes[1] = 1;
            indexes[2] = 2;

            indexes[3] = 2;
            indexes[4] = 3;
            indexes[5] = 0;

            Mesh mesh = new Mesh();
            mesh.vertices = pos;
            mesh.uv = uvs;
            mesh.triangles = indexes;
            mesh.name = "AutoGenerated";

            MeshFilter filter = go.AddComponent<MeshFilter>();
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            
            // Tune components
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.allowOcclusionWhenDynamic = false;
            renderer.material = material;

            filter.mesh = mesh;
        }

        private void CreateAuxRect(GameObject go, UIElement item)
        {
            LayoutItemRect rect = go.AddComponent<LayoutItemRect>();
            rect.Init(item.sizeX, item.sizeY);
        }

        private readonly GameObject _root;
        private float _sizeX;
        private float _sizeY;
    }
}
