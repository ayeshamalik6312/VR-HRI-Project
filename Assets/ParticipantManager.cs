using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ParticipantManager : MonoBehaviour
{
    public Text displayText; // Assign a UI Text element in the Inspector
    public InputField participantInputField; // Assign the input field UI element

    private string filePath;

    void Start()
    {
        // Define file path relative to Unity's persistent data path
        filePath = Path.Combine(Application.persistentDataPath, "participants.csv");

        // Ensure file exists with proper headers
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "ParticipantID,ConditionOrder\n"); // Create file with headers
        }
    }

    // Called when "Generate Participant ID" is clicked
    public void GenerateNewParticipant()
    {
        // Read existing participant IDs
        HashSet<string> existingIDs = new HashSet<string>();
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string[] data = line.Split(',');
            if (data.Length > 0) existingIDs.Add(data[0]);
        }

        // Generate a unique participant ID
        string newID;
        do
        {
            newID = UnityEngine.Random.Range(100000, 999999).ToString();
        } while (existingIDs.Contains(newID));

        // Generate a random condition order
        List<string> conditions = new List<string> { "Baseline", "Augmentation on Prompt", "Continuous Augmentation" };
        ShuffleList(conditions);
        string conditionOrder = string.Join(" -> ", conditions);

        // Save to CSV
        File.AppendAllText(filePath, $"{newID},{conditionOrder}\n");

        // Display result on UI
        displayText.text = $"New Participant Created:\nID: {newID}\nOrder: {conditionOrder}";
    }

    // Called when "Look Up Participant" is clicked
    public void LookUpParticipant()
    {
        string inputID = participantInputField.text; // Get input from UI field
        if (string.IsNullOrEmpty(inputID))
        {
            displayText.text = "Please enter a Participant ID.";
            return;
        }

        if (!File.Exists(filePath))
        {
            displayText.text = "No participant data found.";
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            string[] data = line.Split(',');
            if (data[0] == inputID)
            {
                displayText.text = $"Participant {inputID}:\nOrder: {data[1]}";
                return;
            }
        }

        displayText.text = "Participant not found.";
    }

    // Helper function to shuffle conditions
    private void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
