using DongA.Game;
using DongA.GameDatas;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ball))]
public class BallEditor : Editor
{
    private void OnSceneGUI()
    {
        Ball ball = (Ball)target;
        SerializedProperty pointBProp = serializedObject.FindProperty("pointB");

        EditorGUI.BeginChangeCheck();
        Vector3 newPointB = Handles.PositionHandle(pointBProp.vector3Value, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ball, "Move Ball Point B");
            pointBProp.vector3Value = newPointB;
            serializedObject.ApplyModifiedProperties();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            ApplySpriteFromID();
        }
    }

    private void ApplySpriteFromID()
    {
        Ball ball = (Ball)target;
        var sr = ball.GetComponent<SpriteRenderer>();
        var ballSOProp = serializedObject.FindProperty("ballSO");
        if (sr == null || ballSOProp.objectReferenceValue == null) return;

        var ballSO = (BallSO)ballSOProp.objectReferenceValue;
        if (ballSO.ballDatas == null) return;

        int id = ball.BallId;
        foreach (var data in ballSO.ballDatas)
        {
            if (data.id == id)
            {
                Undo.RecordObject(sr, "Update Ball Sprite");
                sr.sprite = data.ballSprite;
                EditorUtility.SetDirty(sr);
                return;
            }
        }
    }
}
