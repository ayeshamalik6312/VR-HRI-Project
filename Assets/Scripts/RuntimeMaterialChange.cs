using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimeMaterialChange : MonoBehaviour
{
    public Material targetMaterial;

    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();

    private List<GameObject> ARContinuousParents = new List<GameObject>();
    private List<GameObject> ARParents = new List<GameObject>();
    private List<GameObject> allChildrenContinuous = new List<GameObject>();
    private List<GameObject> allChildrenAR = new List<GameObject>();

    private MoveIKTarget moveIKTargetScript;
    private MoveIKTarget.State previousState;

    private void Start()
    {
        ARContinuousParents = GameObject.FindGameObjectsWithTag("ARcontinuous").ToList();
        ARParents = GameObject.FindGameObjectsWithTag("AR").ToList();

        moveIKTargetScript = FindObjectOfType<MoveIKTarget>();
        previousState = moveIKTargetScript.state;

        foreach (GameObject obj in ARContinuousParents)
        {
            Helpers.SearcherExcludingTag(allChildrenContinuous, obj, "Socket");
        }

        foreach (GameObject t in allChildrenContinuous)
        {
            Renderer rendererTemp;

            if (t.gameObject.GetComponent<Renderer>() != null)
            {
                rendererTemp = t.gameObject.GetComponent<Renderer>();
                Material[] materials = rendererTemp.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = targetMaterial;
                    Debug.Log("Material Changed on " + rendererTemp.gameObject.name);
                }
                rendererTemp.materials = materials;
            }
        }
    }

    private void Update()
    {
        if (moveIKTargetScript != null)
        {
            if (moveIKTargetScript.state == MoveIKTarget.State.Connect && previousState != MoveIKTarget.State.Connect)
            {
                allChildrenAR.Clear();

                foreach (GameObject obj in ARParents)
                {
                    Helpers.SearcherExcludingTag(allChildrenAR, obj, "Socket");
                }

                foreach (GameObject t in allChildrenAR)
                {
                    if (t.GetComponent<MeshRenderer>())
                    {
                        t.GetComponent<MeshRenderer>().enabled = true;
                    }
                    ApplyMaterial(t, targetMaterial);
                }
            }
            else if (moveIKTargetScript.state != MoveIKTarget.State.Connect && previousState == MoveIKTarget.State.Connect)
            {
                foreach (GameObject t in allChildrenAR)
                {
                    if (t.GetComponent<MeshRenderer>())
                    {
                        t.GetComponent<MeshRenderer>().enabled = false;
                    }
                    RevertMaterial(t);
                }
            }

            previousState = moveIKTargetScript.state;
        }
    }

    private void ApplyMaterial(GameObject obj, Material newMaterial)
    {
        Renderer rendererTemp;

        if (obj != null && obj.gameObject.GetComponent<Renderer>() != null)
        {
            rendererTemp = obj.gameObject.GetComponent<Renderer>();

            if (!originalMaterials.ContainsKey(obj))
            {
                originalMaterials[obj] = rendererTemp.materials;
            }

            Material[] updatedMaterials = rendererTemp.materials;
            for (int i = 0; i < updatedMaterials.Length; i++)
            {
                updatedMaterials[i] = newMaterial;
            }

            rendererTemp.materials = updatedMaterials;
        }
    }

    private void RevertMaterial(GameObject obj)
    {
        Renderer rendererTemp;

        if (obj != null && obj.gameObject.GetComponent<Renderer>() != null)
        {
            rendererTemp = obj.gameObject.GetComponent<Renderer>();

            if (originalMaterials.ContainsKey(obj))
            {
                rendererTemp.materials = originalMaterials[obj];
            }
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject obj in allChildrenContinuous.Concat(allChildrenAR))
        {
            if (obj != null)
            {
                Renderer rendererTemp = obj.GetComponent<Renderer>();
                if (rendererTemp != null)
                {
                    if (originalMaterials.ContainsKey(obj))
                    {
                        rendererTemp.materials = originalMaterials[obj];
                    }
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        foreach (GameObject obj in allChildrenContinuous.Concat(allChildrenAR))
        {
            if (obj != null)
            {
                Renderer rendererTemp = obj.GetComponent<Renderer>();
                if (rendererTemp != null)
                {
                    if (originalMaterials.ContainsKey(obj))
                    {
                        rendererTemp.materials = originalMaterials[obj];
                    }
                }
            }
        }
    }
}
