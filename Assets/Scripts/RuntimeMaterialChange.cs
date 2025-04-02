using System.Collections.Generic;
using UnityEngine;

public class RuntimeMaterialChange : MonoBehaviour
{
    public Material targetMaterial;

    private Dictionary<GameObject, Material[]> originalMaterials = new();
    private List<GameObject> arTargets = new(); // Active overlay objects to manage

    private MoveIKTarget moveIKTargetScript;
    private MoveIKTarget.State previousState;

    private void Start()
    {
        moveIKTargetScript = FindObjectOfType<MoveIKTarget>();
        if (moveIKTargetScript != null)
            previousState = moveIKTargetScript.state;
    }

    private void Update()
    {
        if (moveIKTargetScript == null) return;

        // Only applies to Augmentation on Prompt
        if (moveIKTargetScript.state == MoveIKTarget.State.Connect && previousState != MoveIKTarget.State.Connect)
        {
            Debug.Log("MaterialChange: CONNECT phase activated - Showing overlay + applying material");

            foreach (GameObject obj in arTargets)
            {
                if (obj.TryGetComponent(out MeshRenderer renderer))
                    renderer.enabled = true;

                ApplyMaterial(obj, targetMaterial);
            }
        }
        else if (moveIKTargetScript.state != MoveIKTarget.State.Connect && previousState == MoveIKTarget.State.Connect)
        {
            Debug.Log("MaterialChange: Exited CONNECT phase - Hiding overlay + reverting material");

            foreach (GameObject obj in arTargets)
            {
                if (obj.TryGetComponent(out MeshRenderer renderer))
                    renderer.enabled = false;

                RevertMaterial(obj);
            }
        }

        previousState = moveIKTargetScript.state;
    }

    public void ActivatePromptOverlay(GameObject root)
    {
        Debug.Log("MaterialChange: Activating PROMPT Overlay");
        arTargets.Clear();
        SearchOverlayChildren(arTargets, root, "Socket");

        foreach (var obj in arTargets)
        {
            if (!originalMaterials.ContainsKey(obj) && obj.TryGetComponent(out Renderer renderer))
                originalMaterials[obj] = renderer.materials;

            if (obj.TryGetComponent(out MeshRenderer rendererTemp))
                rendererTemp.enabled = false; // Initially hidden, shown during CONNECT
        }
    }

    public void ActivateContinuousOverlay(GameObject root)
    {
        Debug.Log("MaterialChange: Activating CONTINUOUS Overlay");
        arTargets.Clear();
        SearchOverlayChildren(arTargets, root, "Socket");

        foreach (var obj in arTargets)
        {
            if (!originalMaterials.ContainsKey(obj) && obj.TryGetComponent(out Renderer renderer))
                originalMaterials[obj] = renderer.materials;

            if (obj.TryGetComponent(out MeshRenderer rendererTemp))
                rendererTemp.enabled = true;

            ApplyMaterial(obj, targetMaterial);
        }
    }

    public void DeactivateOverlay()
    {
        Debug.Log("MaterialChange: Deactivating Overlay");
        foreach (GameObject obj in arTargets)
        {
            if (obj != null)
            {
                if (obj.TryGetComponent(out MeshRenderer renderer))
                    renderer.enabled = false;

                RevertMaterial(obj);
            }
        }

        arTargets.Clear();
    }

    private void SearchOverlayChildren(List<GameObject> result, GameObject parent, string excludeTag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.tag != excludeTag)
                result.Add(child.gameObject);

            SearchOverlayChildren(result, child.gameObject, excludeTag);
        }
    }

    private void ApplyMaterial(GameObject obj, Material newMaterial)
    {
        if (obj.TryGetComponent(out Renderer renderer))
        {
            Material[] newMats = renderer.materials;
            for (int i = 0; i < newMats.Length; i++)
                newMats[i] = newMaterial;

            renderer.materials = newMats;
        }
    }

    private void RevertMaterial(GameObject obj)
    {
        if (obj != null && originalMaterials.ContainsKey(obj) && obj.TryGetComponent(out Renderer renderer))
        {
            renderer.materials = originalMaterials[obj];
        }
    }

    private void OnDestroy()
    {
        DeactivateOverlay(); // Restore everything on quit
    }

    private void OnApplicationQuit()
    {
        DeactivateOverlay(); // Restore everything on quit
    }
}
