using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuntimeMaterialChange : MonoBehaviour
{
    public Material targetMaterial;

    private Dictionary<GameObject, Material[]> originalMaterials = new();
    private MoveIKTarget moveIKTargetScript;
    private MoveIKTarget.State previousState;

    private List<GameObject> arTargets = new(); // Used dynamically for both modes

    private void Start()
    {
        moveIKTargetScript = FindObjectOfType<MoveIKTarget>();
        if (moveIKTargetScript != null)
            previousState = moveIKTargetScript.state;
    }

    private void Update()
    {
        if (moveIKTargetScript == null) return;

        if (moveIKTargetScript.state == MoveIKTarget.State.Connect && previousState != MoveIKTarget.State.Connect)
        {
            foreach (GameObject obj in arTargets)
            {
                if (obj.TryGetComponent(out MeshRenderer renderer))
                    renderer.enabled = true;

                ApplyMaterial(obj, targetMaterial);
            }
        }
        else if (moveIKTargetScript.state != MoveIKTarget.State.Connect && previousState == MoveIKTarget.State.Connect)
        {
            foreach (GameObject obj in arTargets)
            {
                if (obj.TryGetComponent(out MeshRenderer renderer))
                    renderer.enabled = false;

                RevertMaterial(obj);
            }
        }

        previousState = moveIKTargetScript.state;
    }

    public void ActivateContinuousOverlay(GameObject root)
    {
        arTargets.Clear();
        SearchOverlayChildren(arTargets, root, "Socket");

        foreach (var t in arTargets)
        {
            if (!originalMaterials.ContainsKey(t) && t.TryGetComponent(out Renderer renderer))
                originalMaterials[t] = renderer.materials;
        }
    }

    public void ActivatePromptOverlay(GameObject root)
    {
        arTargets.Clear();
        SearchOverlayChildren(arTargets, root, "Socket");

        foreach (var t in arTargets)
        {
            if (!originalMaterials.ContainsKey(t) && t.TryGetComponent(out Renderer renderer))
                originalMaterials[t] = renderer.materials;

            if (t.TryGetComponent(out MeshRenderer rendererTemp))
                rendererTemp.enabled = false;
        }
    }

    public void DeactivateOverlay()
    {
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

    void SearchOverlayChildren(List<GameObject> result, GameObject parent, string excludeTag)
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
}
