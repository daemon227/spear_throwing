using UnityEngine;

public static class Vector3IntExtensions
{
    public static bool IsBetween(this Vector3Int v, Vector3Int min, Vector3Int max)
    {
        return min.x <= v.x && v.x <= max.x
            && min.y <= v.y && v.y <= max.y
            && min.z <= v.z && v.z <= max.z;
    }
}
