using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimeMaterialChange : MonoBehaviour
{
    public Material targetMaterial;
    public GameObject extraObject; // New object to include in overlays


    private Dictionary<GameObject, Material[]> originalMaterials = new();
    private List<GameObject> arTargets = new(); // Active overlay objects to manage
    private List<GameObject> extraTargets = new(); // NEW: specific targets from extraObject

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

        var currentState = moveIKTargetScript.state;

        if (currentState == MoveIKTarget.State.Connect && previousState != MoveIKTarget.State.Connect)
        {
            foreach (GameObject obj in arTargets.Concat(extraTargets))
            {
                if (obj.TryGetComponent(out MeshRenderer renderer))
                    renderer.enabled = true;

                ApplyMaterial(obj, targetMaterial);
            }
        }
        else if (currentState == MoveIKTarget.State.Move && previousState != MoveIKTarget.State.Move)
        {
            // Fix: Turn off extraTargets in Continuous mode when returning to Move state
            ParticipantManager participantManager = FindObjectOfType<ParticipantManager>();
            if (participantManager != null && participantManager.CurrentCondition == "Continuous")
            {
                foreach (GameObject obj in extraTargets)
                {
                    if (obj.TryGetComponent(out MeshRenderer renderer))
                        renderer.enabled = false;
                }
            }
        }

        previousState = currentState;
    }


    public void ActivatePromptOverlay(GameObject root)
    {
      //  Debug.Log("MaterialChange: Activating PROMPT Overlay");
        arTargets.Clear();
        extraTargets.Clear(); // reset both lists

        SearchOverlayChildren(arTargets, root, "Socket");

        if (extraObject != null)
        {
            SearchOverlayChildren(extraTargets, extraObject, "Socket");
        }

        foreach (var obj in arTargets.Concat(extraTargets))
        {
            if (!originalMaterials.ContainsKey(obj) && obj.TryGetComponent(out Renderer renderer))
                originalMaterials[obj] = renderer.materials;

            if (obj.TryGetComponent(out MeshRenderer rendererTemp))
                rendererTemp.enabled = false; // Initially hidden
        }
    }

    public void DeactivatePromptOverlay()
    {
      //  Debug.Log("MaterialChange: Deactivating PROMPT Overlay");

        foreach (GameObject obj in arTargets.Concat(extraTargets))
        {
            if (obj != null)
            {
                if (obj.TryGetComponent(out MeshRenderer renderer))
                    renderer.enabled = false;

                RevertMaterial(obj);
            }
        }

        arTargets.Clear();
        extraTargets.Clear();
    }


    public void ActivateContinuousOverlay(GameObject root)
    {
      //  Debug.Log("MaterialChange: Activating CONTINUOUS Overlay");
        arTargets.Clear();
        extraTargets.Clear();

        SearchOverlayChildren(arTargets, root, "Socket");

        if (extraObject != null)
        {
            SearchOverlayChildren(extraTargets, extraObject, "Socket");
        }

        foreach (var obj in arTargets)
        {
            if (!originalMaterials.ContainsKey(obj) && obj.TryGetComponent(out Renderer renderer))
                originalMaterials[obj] = renderer.materials;

            if (obj.TryGetComponent(out MeshRenderer rendererTemp))
                rendererTemp.enabled = true;

            ApplyMaterial(obj, targetMaterial);
        }

        // NEW: extraTargets stay hidden, not active yet
        foreach (var obj in extraTargets)
        {
            if (!originalMaterials.ContainsKey(obj) && obj.TryGetComponent(out Renderer renderer))
                originalMaterials[obj] = renderer.materials;

            if (obj.TryGetComponent(out MeshRenderer rendererTemp))
                rendererTemp.enabled = false; // Hide initially
        }
    }



    public void DeactivateOverlay()
    {
       // Debug.Log("MaterialChange: Deactivating Overlay");
        foreach (GameObject obj in arTargets.Concat(extraTargets))
        {
            if (obj != null)
            {
                if (obj.TryGetComponent(out MeshRenderer renderer))
                    renderer.enabled = false;

                RevertMaterial(obj);
            }
        }

        arTargets.Clear();
        extraTargets.Clear();
    }

    private void SearchOverlayChildren(List<GameObject> result, GameObject parent, string excludeTag)
    {
        foreach (Transform child in parent.transform)
        {
            if (!child.CompareTag(excludeTag))
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
