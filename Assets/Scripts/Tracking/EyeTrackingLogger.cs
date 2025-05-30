using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EyeTrackingLogger : MonoBehaviour
{
    public float maxRayDistance = 100f;
    private OVREyeGaze leftEye;
    private OVREyeGaze rightEye;

    private string currentObject = "None";
    private float gazeStartTime = 0f;

    private string logFilePath;

    void Start()
    {
        enabled = false;

        leftEye = GetComponentInChildren<OVREyeGaze>(true);
        rightEye = GetComponentInChildren<OVREyeGaze>(true);

        string downloadsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
        logFilePath = Path.Combine(downloadsPath, "eye_tracking_log.csv");

        File.WriteAllText(logFilePath, "Timestamp,ObjectLookedAt,GazeDuration\n");
    }

    void Update()
    {
        string lookedAt = GetLookedObject(rightEye); // Or use combined logic
        float now = Time.time;

        if (lookedAt != currentObject)
        {
            if (currentObject != "None")
            {
                float duration = now - gazeStartTime;
                File.AppendAllText(logFilePath, $"{Time.time:F2},{currentObject},{duration:F2}\n");
            }

            currentObject = lookedAt;
            gazeStartTime = now;
        }
    }
    public void StartLogging(string filePath)
    {
        logFilePath = filePath;
        gazeStartTime = Time.time;
        File.WriteAllText(logFilePath, "Timestamp,ObjectLookedAt,GazeDuration\n");
        enabled = true;
    }

    public void StopLogging()
    {
        enabled = false;

        if (currentObject != "None")
        {
            float duration = Time.time - gazeStartTime;
            File.AppendAllText(logFilePath, $"{Time.time:F2},{currentObject},{duration:F2}\n");
        }
    }

    string GetLookedObject(OVREyeGaze eye)
    {
        if (eye == null) return "None";

        Ray ray = new Ray(eye.transform.position, eye.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
            return hit.collider.name;

        return "None";
    }

    void OnApplicationQuit()
    {
        if (currentObject != "None")
        {
            float duration = Time.time - gazeStartTime;
            File.AppendAllText(logFilePath, $"{Time.time:F2},{currentObject},{duration:F2}\n");
        }
    }
}
