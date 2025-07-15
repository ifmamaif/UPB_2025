using UnityEditor;
using UnityEngine;
using static LSystemTurtle;

[CustomEditor(typeof(LSystemTurtle))]
public class LSystemTurtleEditor : Editor
{
    private LSystemTurtle m_lsystem;
    private SerializedProperty m_autoUpdateProp;
    private SerializedProperty m_renderModeProp;
    private RenderModeType m_previousRenderMode;

    private void OnEnable()
    {
        m_lsystem = (LSystemTurtle)target;
        m_autoUpdateProp = serializedObject.FindProperty("m_autoUpdate");
        m_renderModeProp = serializedObject.FindProperty("m_renderMode");

        m_previousRenderMode = (RenderModeType)m_renderModeProp.enumValueIndex;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_autoUpdate");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_autoUpdateProp, new GUIContent("Auto Update"));

        bool autoUpdate = m_autoUpdateProp.boolValue;
        bool hasChanges = serializedObject.hasModifiedProperties;

        serializedObject.ApplyModifiedProperties();

        RenderModeType currentRenderMode = (RenderModeType)m_renderModeProp.enumValueIndex;
        if (currentRenderMode != m_previousRenderMode)
        {
            m_previousRenderMode = currentRenderMode;
            m_lsystem.EnsureLineRendererIfNeeded();

            if (autoUpdate)
            {
                m_lsystem.GenerateInEditor();
                EditorUtility.SetDirty(m_lsystem);
            }
        }

        if (autoUpdate && hasChanges)
        {
            m_lsystem.GenerateInEditor();
            EditorUtility.SetDirty(m_lsystem);
        }
        else if (!autoUpdate)
        {
            if (GUILayout.Button("Generate L-System"))
            {
                m_lsystem.GenerateInEditor();
                EditorUtility.SetDirty(m_lsystem);
            }
        }
    }
}
