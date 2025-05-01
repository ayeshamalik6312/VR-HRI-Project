using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPoint : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        gameObject.transform.position = target.position + new Vector3(0, 0, 0.005f);
        gameObject.transform.rotation = target.rotation;
    }
}
