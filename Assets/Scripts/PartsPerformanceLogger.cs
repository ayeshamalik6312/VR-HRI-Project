using System.IO;
using UnityEngine;

public class PartsPerformanceLogger
{
    private string filePath;

    public PartsPerformanceLogger(string path)
    {
        filePath = path;
    }

    public void LogSnap(int structureCount, bool isFlipped, int timeRemainingInSeconds, string buttonPressed, string chipStatus, bool decisionCorrect)
    {
        string orientation = isFlipped ? "Correct" : "Incorrect";
        int minutes = timeRemainingInSeconds / 60;
        int seconds = timeRemainingInSeconds % 60;
        string formattedTime = $"{minutes:D2}:{seconds:D2}";

        string line = $"{formattedTime},{structureCount},{orientation},{buttonPressed},{chipStatus},{decisionCorrect}";
        File.AppendAllText(filePath, line + "\n");
    }

}
