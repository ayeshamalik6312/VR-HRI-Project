using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class MoveBody : MonoBehaviour
{
    public GameObject body;
    public GameObject camera;
    public Transform target;
    public bool setup = false;

    private void Update()
    {
        if (setup)
        {
            Setup();
            setup = false;
        }
    }

    private void Setup()
    {
        float rotAngleY = camera.transform.rotation.eulerAngles.y - target.transform.rotation.eulerAngles.y;
        body.transform.Rotate(0, -rotAngleY, 0);

        Vector3 distanceDiff = target.position - camera.transform.position;

        body.transform.position += distanceDiff;
    }
}
