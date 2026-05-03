using UnityEngine;
using UnityEditor;

public class LODGroupHelper
{
    [MenuItem("Tools/Add LOD Level (Append)")]
    static void AppendLODLevel()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        // Get or create LODGroup
        LODGroup lodGroup = selected.GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = selected.AddComponent<LODGroup>();
        }

        // Get all MeshRenderers
        MeshRenderer[] renderers = selected.GetComponentsInChildren<MeshRenderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("No MeshRenderers found.");
            return;
        }

        // Get existing LODs
        LOD[] existingLODs = lodGroup.GetLODs();

        // Create new LOD array (append one)
        LOD[] newLODs = new LOD[existingLODs.Length + 1];

        for (int i = 0; i < existingLODs.Length; i++)
        {
            newLODs[i] = existingLODs[i];
        }

        // Calculate a reasonable transition height
        float newTransitionHeight = 0.2f;

        if (existingLODs.Length > 0)
        {
            // Make each new LOD smaller than the last one
            newTransitionHeight = existingLODs[existingLODs.Length - 1].screenRelativeTransitionHeight * 0.5f;
        }

        // Append new LOD
        newLODs[newLODs.Length - 1] = new LOD(newTransitionHeight, renderers);

        // Apply
        lodGroup.SetLODs(newLODs);
        lodGroup.RecalculateBounds();

        Debug.Log("Added LOD level " + (newLODs.Length - 1) + 
                  " with " + renderers.Length + " renderers. " +
                  "Transition height: " + newTransitionHeight);
    }
}