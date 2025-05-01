using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers 
{
    public static void DestroyChildren(this Transform t) 
    {
        foreach (Transform child in t)
            Object.Destroy(child.gameObject);
    }

    public static void Searcher(List<GameObject> list, GameObject root)
    {
        list.Add(root);
        if (root.transform.childCount > 0)
        {
            foreach (Transform child in root.transform)
            {
                Searcher(list, child.gameObject);
            }
        }
    }
    public static void SearcherExcludingTag(List<GameObject> allChildren, GameObject parent, string excludedTag)
    {
        foreach (Transform child in parent.transform)
        {
            // If the child doesn't have the excluded tag, add it to the list
            if (child.gameObject.tag != excludedTag)
            {
                allChildren.Add(child.gameObject);

                // Recursively search through each child's children
                SearcherExcludingTag(allChildren, child.gameObject, excludedTag);
            }
        }
    }
}

