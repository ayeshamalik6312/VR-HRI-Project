using UnityEngine;

public class SnapTriggerZone : MonoBehaviour
{
    public Transform snapTarget; // assign this to the snapPoint in inspector
    public float maxDistance = 0.025f;
    public float maxAngle = 12f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("male")) return;

        Transform key = other.transform;

        float distance = Vector3.Distance(key.position, snapTarget.position);
        float angle = Quaternion.Angle(key.rotation, snapTarget.rotation);

        if (distance <= maxDistance && angle <= maxAngle)
        {
            var snapper = key.GetComponent<SnapParts>();
            if (snapper != null && !snapper.snapped)
            {
                snapper.BeginSnap(snapTarget.position, snapTarget.rotation);
            }
        }
    }
}
