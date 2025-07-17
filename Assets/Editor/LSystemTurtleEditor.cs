using UnityEditor;
using UnityEngine;
using static LSystemTurtle;

/// <summary>
/// Custom inspector for LSystemTurtle to support auto update,
/// render mode switching and manual generation in the editor.
/// </summary>
[CustomEditor(typeof(LSystemTurtle))]
public class LSystemTurtleEditor : Editor
{
    private LSystemTurtle m_lsystem;
    private SerializedProperty m_autoUpdateProp;
    private SerializedProperty m_renderModeProp;

    private void OnEnable()
    {
        m_lsystem = (LSystemTurtle)target;

        // Cache the serialized properties for cleaner access.
        m_autoUpdateProp = serializedObject.FindProperty("m_autoUpdate");
        m_renderModeProp = serializedObject.FindProperty("m_renderMode");
    }

    public override void OnInspectorGUI()
    {
        // Keep track of previous value for comparison.
        bool previousAutoUpdate = m_autoUpdateProp.boolValue;
        RenderModeType previousRenderMode = (RenderModeType)m_renderModeProp.enumValueIndex;

        serializedObject.Update();

        // Draw everything except the autoUpdate checkbox separately.
        DrawPropertiesExcluding(serializedObject, "m_autoUpdate");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_autoUpdateProp, new GUIContent("Auto Update"));

        bool autoUpdate = m_autoUpdateProp.boolValue;
        bool hasChanges = serializedObject.hasModifiedProperties;
        RenderModeType currentRenderMode = (RenderModeType)m_renderModeProp.enumValueIndex;

        serializedObject.ApplyModifiedProperties();

        // Detect change in render mode and regenerate immediately.
        if (currentRenderMode != previousRenderMode)
        {
            m_lsystem.SetupRenderParent();
            m_lsystem.ClearChildren();
            m_lsystem.GenerateInEditor();

            EditorUtility.SetDirty(m_lsystem);

            if (GUILayout.Button("Save as Prefab"))
            {
                SaveContainerAsPrefab();
            }

            return;
        }

        // Auto-update if enabled and any other property changed (except autoUpdate itself).
        if (autoUpdate && hasChanges)
        {
            m_lsystem.SetupRenderParent();
            m_lsystem.ClearChildren();
            m_lsystem.GenerateInEditor();

            EditorUtility.SetDirty(m_lsystem);

            if (GUILayout.Button("Save as Prefab"))
            {
                SaveContainerAsPrefab();
            }

            return;
        }

        // Manual update if autoUpdate is disabled.
        if (!autoUpdate && GUILayout.Button("Generate L-System"))
        {
            m_lsystem.SetupRenderParent();
            m_lsystem.ClearChildren();
            m_lsystem.GenerateInEditor();

            EditorUtility.SetDirty(m_lsystem);
        }

        if (GUILayout.Button("Save as Prefab"))
        {
            SaveContainerAsPrefab();
        }
    }

    private void SaveContainerAsPrefab()
    {
        GameObject container = m_lsystem.transform.Find("LSystemContainer")?.gameObject;

        if (container == null)
        {
            Debug.LogWarning("No LSystemContainer found to save.");
            return;
        }

        // Open save dialog
        string path = EditorUtility.SaveFilePanelInProject(
            "Save L-System as Prefab",
            "LSystemPrefab",
            "prefab",
            "Select location to save the prefab."
        );

        if (string.IsNullOrEmpty(path))
            return;

        // Create the prefab asset
        GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(container, path, InteractionMode.UserAction);

        if (prefab != null)
        {
            Debug.Log($"Saved prefab: {path}");
        }
        else
        {
            Debug.LogError("Failed to save prefab.");
        }
    }
}
