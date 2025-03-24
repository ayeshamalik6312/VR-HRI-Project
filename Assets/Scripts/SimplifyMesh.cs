using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMeshSimplifier;

public class SimplifyMesh : MonoBehaviour
{
    // Attach script to root of objects you want to simplify meshes for
    // Good quality number for AR Robot Visual: 0.68
    // Good quality number for Physical Robot: 0.72

    public float quality = 0.5f;
    //public bool simplify = false;
    //public bool isDone = false;

    private List<GameObject> allChildren = new List<GameObject>();

    private void Start()
    {
        SimplifyMeshFunc();
    }

    //private void Update()
    //{
    //    if (simplify)
    //    {
    //        simplify = false;
    //        SimplifyMeshFunc();
    //    }
    //    else if (isDone)
    //        this.enabled = false;
    //}

    public void SimplifyMeshFunc()
    {
        Helpers.Searcher(allChildren, this.gameObject);

        foreach (GameObject child in allChildren)
        {
            if (child.GetComponent<MeshFilter>() != null)
            {
                Mesh originalMesh = child.GetComponent<MeshFilter>().sharedMesh;
                MeshSimplifier meshSimplifier = new MeshSimplifier();
                meshSimplifier.Initialize(originalMesh);
                meshSimplifier.SimplifyMesh(quality);
                Mesh targetMesh = meshSimplifier.ToMesh();
                child.GetComponent<MeshFilter>().sharedMesh = targetMesh;
            }
        }
    }
}
