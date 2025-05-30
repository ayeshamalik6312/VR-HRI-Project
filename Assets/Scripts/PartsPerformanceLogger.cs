using System.IO;
using UnityEngine;

public class PartsPerformanceLogger
{
    private string filePath;

    public PartsPerformanceLogger(string path)
    {
        filePath = path;
    }

    public void LogSnap(int structureCount, bool isFlipped, float timeRemaining, string buttonPressed, string chipStatus, bool decisionCorrect)
    {
        string orientation = isFlipped ? "Correct" : "Incorrect";

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        float seconds = timeRemaining % 60f;
        string formattedTime = $"{minutes:D2}:{seconds:00.0000}";

        string line = $"{formattedTime},{structureCount},{orientation},{buttonPressed},{chipStatus},{decisionCorrect}";
        File.AppendAllText(filePath, line + "\n");
    }
}
