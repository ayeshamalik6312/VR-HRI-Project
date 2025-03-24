using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class MoveCameraOnStart : MonoBehaviour
{
    public XROrigin xrOrigin;
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
        xrOrigin.MoveCameraToWorldLocation(target.position);
        xrOrigin.MatchOriginUpCameraForward(target.up, target.forward);
    }
}
