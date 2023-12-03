using UnityEngine;

namespace msTech.Math
{
    public static class Intersection
    {
        public static float GetDistancePointAndPlane(Vector3 planePos, Vector3 planeNor, Vector3 point)
        {
            Vector3 delta = point - planePos;
            float distance = Vector3.Dot(planeNor, delta);
            return distance;
        }


        public static Vector3 GetNormal(Vector3 posA, Vector3 posB, Vector3 posC)
        {
            Vector3 vecBA = posB - posA;
            Vector3 vecCA = posC - posA;
            Vector3 normal = Vector3.Cross(vecBA, vecCA);
            return normal.normalized;
        }
    }
}
