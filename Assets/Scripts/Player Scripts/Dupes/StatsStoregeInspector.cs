using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatsStorage))]
public class StatsStorageInspector : Editor
{
    private void OnEnable()
    {
        // Start repainting the inspector during runtime
        EditorApplication.update += RepaintInspector;
    }

    private void OnDisable()
    {
        // Stop repainting the inspector when not in use
        EditorApplication.update -= RepaintInspector;
    }

    private void RepaintInspector()
    {
        if (Application.isPlaying)
        {
            Repaint(); // Force the inspector to refresh
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw default fields like the PlayerStats reference

        StatsStorage statsStorage = (StatsStorage)target;

        if (statsStorage.PlayerStats != null)
        {
            EditorGUILayout.LabelField("Player Stats", EditorStyles.boldLabel);

            // Display dynamically changing values
            EditorGUILayout.LabelField("Health", $"{statsStorage.PlayerStats.currentHealth}/{statsStorage.PlayerStats.maxHealth}");
            EditorGUILayout.LabelField("Hunger", $"{statsStorage.PlayerStats.currentHunger}/{statsStorage.PlayerStats.maxHunger}");
            EditorGUILayout.LabelField("Thirst", $"{statsStorage.PlayerStats.currentThirst}/{statsStorage.PlayerStats.maxThirst}");
            EditorGUILayout.LabelField("Stamina", $"{statsStorage.PlayerStats.currentStamina}/{statsStorage.PlayerStats.maxStamina}");
            EditorGUILayout.LabelField("Sanity", $"{statsStorage.PlayerStats.currentSanity}/{statsStorage.PlayerStats.maxSanity}");
        }
        else
        {
            EditorGUILayout.HelpBox("No PlayerStats assigned.", MessageType.Warning);
        }
    }
}
