using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LSystemTurtle))]
public class LSystemTurtleEditor : Editor
{
    private LSystemTurtle lsystem;
    private SerializedProperty autoUpdateProp;

    private void OnEnable()
    {
        lsystem = (LSystemTurtle)target;
        autoUpdateProp = serializedObject.FindProperty("m_autoUpdate");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_autoUpdate");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(autoUpdateProp, new GUIContent("Auto Update"));

        serializedObject.ApplyModifiedProperties();

        if (autoUpdateProp.boolValue)
        {
            if (GUI.changed)
            {
                lsystem.GenerateInEditor();
                EditorUtility.SetDirty(lsystem);
            }
        }
        else
        {
            if (GUILayout.Button("Generate L-System"))
            {
                lsystem.GenerateInEditor();
                EditorUtility.SetDirty(lsystem);
            }
        }
    }
}
