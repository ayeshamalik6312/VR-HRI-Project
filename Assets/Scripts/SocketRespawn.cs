using UnityEngine;

public class GrabbableRespawner : MonoBehaviour
{
    public Collider checkZone;           // Assign collider to check
    public GameObject prefabToSpawn;     // Assign the prefab to spawn
    public float checkInterval = 1f;     // How often to check (seconds)

    private void Start()
    {
        InvokeRepeating(nameof(CheckAndRespawn), 0f, checkInterval);
    }

    void CheckAndRespawn()
    {
        if (checkZone == null || prefabToSpawn == null) return;

        Vector3 center = checkZone.bounds.center;
        Vector3 halfExtents = checkZone.bounds.extents;
        Quaternion rotation = checkZone.transform.rotation;

        Collider[] overlapping = Physics.OverlapBox(center, halfExtents, rotation);

        if (overlapping.Length == 0)
        {
            Debug.Log("Nothing in zone. Spawning prefab.");
            Instantiate(prefabToSpawn, checkZone.transform.position, Quaternion.identity);
        }
        else
        {

            bool zoneIsEmptyOrOnlyContainsKeys = true;

            foreach (var col in overlapping)
            {
                GameObject obj = col.gameObject;

                if (obj.name != "Keys")
                {
                    zoneIsEmptyOrOnlyContainsKeys = false;
                    break;
                }
            }

            if (zoneIsEmptyOrOnlyContainsKeys)
            {
                Debug.Log("Zone is empty. Spawning prefab.");
                Instantiate(prefabToSpawn, checkZone.transform.position, Quaternion.identity);
            }
            else
            {
            }
        }
    }

    string GetFullPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    private void OnDrawGizmosSelected()
    {
        if (checkZone != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(checkZone.bounds.center, checkZone.bounds.size);
        }
    }
}
