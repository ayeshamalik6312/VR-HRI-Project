using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPresencePhysics : MonoBehaviour
{
    public Transform target;
    public Renderer nonPhysicalHand;

    Rigidbody rb;
    float showHandDistance = 0.05f;
    Collider[] handColliders;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        handColliders = GetComponentsInChildren<Collider>();
    }
    
    public void EnableHandCollider()
    {
        foreach (Collider collider in handColliders)
        {
            collider.enabled = true;
        }
    }
    public void EnableHandColliderDelay(float delay)
    {
        Invoke("EnableHandCollider", delay);
    }

    public void DisableHandCollider()
    {
        foreach (Collider collider in handColliders)
        {
            collider.enabled = false;
        }
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > showHandDistance)
            nonPhysicalHand.enabled = true;
        else
            nonPhysicalHand.enabled = false;
    }

    private void FixedUpdate()
    {
        // position
        rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;

        // rotation
        Quaternion rotDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        Vector3 rotDifferenceDegree = angleInDegree * rotationAxis;

        rb.angularVelocity = rotDifferenceDegree * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }
}
