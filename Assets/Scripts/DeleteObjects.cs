using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeleteObjects : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.transform.root.gameObject);
    }
}
