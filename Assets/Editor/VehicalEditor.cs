using DongA.Obstacles;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Vehical))]
public class VehicalEditor : Editor
{
    private void OnSceneGUI()
    {
        Vehical vehical = (Vehical)target;
        SerializedProperty pointBProp = serializedObject.FindProperty("pointB");

        EditorGUI.BeginChangeCheck();
        Vector3 newPointB = Handles.PositionHandle(pointBProp.vector3Value, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(vehical, "Move Vehical Point B");
            pointBProp.vector3Value = newPointB;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
