using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class ParticipantManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Dropdown baselineDropdown;
    public TMP_Dropdown augOnPromptDropdown;
    public TMP_Dropdown continuousAugDropdown;

    public GameObject overlayObject;
    public ForemanTutorialController characterController;
    public RuntimeMaterialChange materialChanger;


    public TMP_Text timerText;
    public TMP_Text currentPhaseText;

    private string filePath;
    private string[] conditionOrder = new string[3];
    private Coroutine timerCoroutine;

    void Start()
    {
        string dir = Path.Combine(Application.dataPath, "../ParticipantData");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        filePath = Path.Combine(dir, "participants.csv");

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "ID,Baseline,AugOnPrompt,Continuous\n");

        baselineDropdown.onValueChanged.AddListener(_ => SaveParticipantData());
        augOnPromptDropdown.onValueChanged.AddListener(_ => SaveParticipantData());
        continuousAugDropdown.onValueChanged.AddListener(_ => SaveParticipantData());
        inputField.onEndEdit.AddListener(OnInputChanged);

        if (currentPhaseText) currentPhaseText.text = "Idle";
        if (timerText) timerText.text = "10:00";
    }

    void OnInputChanged(string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
            LoadOrCreateParticipant(id.Trim());
    }

    void LoadOrCreateParticipant(string id)
    {
        bool found = false;
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (line.StartsWith(id + ","))
            {
                var values = line.Split(',');
                if (values.Length >= 4)
                {
                    SetDropdownValueFromText(baselineDropdown, values[1]);
                    SetDropdownValueFromText(augOnPromptDropdown, values[2]);
                    SetDropdownValueFromText(continuousAugDropdown, values[3]);

                    conditionOrder = new string[3];
                    conditionOrder[int.Parse(values[1]) - 1] = "Baseline";
                    conditionOrder[int.Parse(values[2]) - 1] = "AugOnPrompt";
                    conditionOrder[int.Parse(values[3]) - 1] = "Continuous";
                    found = true;
                }
                break;
            }
        }

        if (!found)
        {
            baselineDropdown.value = 0;
            augOnPromptDropdown.value = 0;
            continuousAugDropdown.value = 0;
            baselineDropdown.RefreshShownValue();
            augOnPromptDropdown.RefreshShownValue();
            continuousAugDropdown.RefreshShownValue();
        }
    }

    void SaveParticipantData()
    {
        string id = inputField.text.Trim();
        if (string.IsNullOrEmpty(id)) return;

        string baselineText = baselineDropdown.options[baselineDropdown.value].text;
        string augOnPromptText = augOnPromptDropdown.options[augOnPromptDropdown.value].text;
        string continuousText = continuousAugDropdown.options[continuousAugDropdown.value].text;

        bool validBaseline = int.TryParse(baselineText, out int baselineVal);
        bool validAugPrompt = int.TryParse(augOnPromptText, out int augOnPromptVal);
        bool validContinuous = int.TryParse(continuousText, out int continuousVal);

        if (!validBaseline || !validAugPrompt || !validContinuous)
        {
            Debug.LogWarning("One or more dropdowns are not valid integers. Participant data not saved.");
            return;
        }

        List<string> updatedLines = new List<string>();
        bool updated = false;
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (line.StartsWith(id + ","))
            {
                updatedLines.Add($"{id},{baselineVal},{augOnPromptVal},{continuousVal}");
                updated = true;
            }
            else
            {
                updatedLines.Add(line);
            }
        }

        if (!updated)
            updatedLines.Add($"{id},{baselineVal},{augOnPromptVal},{continuousVal}");

        File.WriteAllLines(filePath, updatedLines.ToArray());
    }

    void SetDropdownValueFromText(TMP_Dropdown dropdown, string text)
    {
        int index = dropdown.options.FindIndex(option => option.text == text);
        dropdown.value = index >= 0 ? index : 0;
        dropdown.RefreshShownValue();
    }

    public void LaunchTutorialAndCondition1()
    {
        StartCoroutine(RunTutorialAndCondition(0));
    }

    public void LaunchCondition2()
    {
        RunCondition(conditionOrder[1]);
    }

    public void LaunchCondition3()
    {
        RunCondition(conditionOrder[2]);
    }

    private IEnumerator RunTutorialAndCondition(int conditionIndex)
    {
        if (currentPhaseText) currentPhaseText.text = "Tutorial";

        yield return StartCoroutine(characterController.CharacterSequence());

        RunCondition(conditionOrder[conditionIndex]);
    }

    private void RunCondition(string conditionName)
    {
        if (overlayObject == null || materialChanger == null)
        {
            Debug.LogError("Overlay object or materialChanger not assigned.");
            return;
        }

        MoveIKTarget moveIKTarget = FindObjectOfType<MoveIKTarget>();
        if (moveIKTarget != null) moveIKTarget.move = true;

        switch (conditionName)
        {
            case "Baseline":
                overlayObject.SetActive(false);
                materialChanger.DeactivateOverlay();
                break;

            case "AugOnPrompt":
                overlayObject.SetActive(true);
                overlayObject.tag = "AR";
                materialChanger.ActivatePromptOverlay(overlayObject);
                break;

            case "Continuous":
                overlayObject.SetActive(true);
                overlayObject.tag = "ARcontinuous";
                materialChanger.ActivateContinuousOverlay(overlayObject);
                break;

            default:
                Debug.LogWarning("Unknown condition name: " + conditionName);
                break;
        }

        if (currentPhaseText)
            currentPhaseText.text = conditionName;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTimer(600)); // 10 mins
    }


    private IEnumerator StartTimer(int seconds)
    {
        int remaining = seconds;
        while (remaining > 0)
        {
            int minutes = remaining / 60;
            int secs = remaining % 60;
            if (timerText)
                timerText.text = minutes.ToString("D2") + ":" + secs.ToString("D2");
            yield return new WaitForSeconds(1);
            remaining--;
        }

        if (timerText)
            timerText.text = "00:00";
        currentPhaseText.text = "Idle";
    }
}
