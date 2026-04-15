using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3Int RoundToVector3Int(this Vector3 vector)
    {
        return new Vector3Int(
            Mathf.RoundToInt(vector.x),
            Mathf.RoundToInt(vector.y),
            Mathf.RoundToInt(vector.z)
        );
    }
}
