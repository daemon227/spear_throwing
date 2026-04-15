using DongA.Characters;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Wizard1))]
public class Wizard1Editor : Editor
{
    private void OnSceneGUI()
    {
        Wizard1 wizard = (Wizard1)target;

        Handles.color = Color.yellow;
        EditorGUI.BeginChangeCheck();

        Vector2 newB = Handles.FreeMoveHandle(
            wizard.PointB,
            0.2f,
            Vector3.zero,
            Handles.SphereHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(wizard, "Move Wizard1 Point B");
            wizard.PointB = newB;
        }

        Handles.color = Color.cyan;
        Handles.Label((Vector2)wizard.transform.position + Vector2.up * 0.3f, "A");
        Handles.color = Color.yellow;
        Handles.Label(wizard.PointB + Vector2.up * 0.3f, "B");
    }
}
