using System.IO;
using UnityEngine;

public class PartsPerformanceLogger
{
    private string filePath;

    public PartsPerformanceLogger(string path)
    {
        filePath = path;
    }

    public void LogSnap(int structureCount, bool isFlipped, float timeRemaining, string buttonPressed, string chipStatus, bool decisionCorrect, float timeToConnect, float timeFromSnapToPress)
    {
        string orientation = isFlipped ? "Incorrect" : "Correct";

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        float seconds = timeRemaining % 60f;
        string formattedTime = $"{minutes:D2}:{seconds:00.0000}";

        string line = $"{formattedTime},{structureCount},{orientation},{buttonPressed},{chipStatus},{decisionCorrect},{timeToConnect:F4},{timeFromSnapToPress:F4}";
        File.AppendAllText(filePath, line + "\n");
    }
}
