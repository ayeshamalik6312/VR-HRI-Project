using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MotionTrackerLogger : MonoBehaviour
{
    private List<Transform> trackedObjects = new List<Transform>();
    private List<string> dataBuffer = new List<string>();
    private string filePath;
    private bool isRecording = false;

    public void Initialize(List<Transform> objectsToTrack, string savePath)
    {
        trackedObjects = objectsToTrack;
        filePath = savePath;
        isRecording = true;

        // Write CSV header
        string header = "Time,ObjectName," +
                        "PosX,PosY,PosZ," +
                        "RotX,RotY,RotZ,RotW," +
                        "EulerX,EulerY,EulerZ";
        File.WriteAllText(filePath, header + "\n");
    }

    void Update()
    {
        if (!isRecording) return;

        foreach (var obj in trackedObjects)
        {
            Vector3 pos = obj.position;
            Quaternion rot = obj.rotation;
            Vector3 euler = obj.eulerAngles;

            string line = $"{Time.time:F4},{obj.name}," +
                          $"{pos.x:F4},{pos.y:F4},{pos.z:F4}," +
                          $"{rot.x:F4},{rot.y:F4},{rot.z:F4},{rot.w:F4}," +
                          $"{euler.x:F2},{euler.y:F2},{euler.z:F2}";
            dataBuffer.Add(line);
        }

        if (dataBuffer.Count >= 500)
        {
            FlushData();
        }
    }

    public void StopRecording()
    {
        isRecording = false;
        FlushData();
        Destroy(this); // optional: destroy logger component after done
    }

    private void FlushData()
    {
        if (dataBuffer.Count > 0)
        {
            File.AppendAllLines(filePath, dataBuffer);
            dataBuffer.Clear();
        }
    }
}
