using UnityEngine;

namespace msTech.Render
{
    public class LayoutItemRect : MonoBehaviour
    {
        public void Init(float sizeX, float sizeY)
        {
            float halfSizeX = sizeX * 0.5f;
            float halfSizeY = sizeY * 0.5f;
            _pos[0] = new Vector3(-halfSizeX,  halfSizeY, 0.0f);
            _pos[1] = new Vector3( halfSizeX,  halfSizeY, 0.0f);
            _pos[2] = new Vector3( halfSizeX, -halfSizeY, 0.0f);
            _pos[3] = new Vector3(-halfSizeX, -halfSizeY, 0.0f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            for ( int i = 0; i < 4; ++i )
            {
                int idA = i;
                int idB = ( i + 1 ) & 0x03;
                Vector3 posA = _pos[idA] + transform.position;
                Vector3 posB = _pos[idB] + transform.position;
                Gizmos.DrawLine(posA, posB);
            }
        }

        private Vector3[] _pos = new Vector3[4];
    }
}
