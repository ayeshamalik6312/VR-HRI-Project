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
    public TMP_Text warningMessage;

    private string filePath;
    private string[] conditionOrder = new string[3];
    private Coroutine timerCoroutine;
    private bool hasActiveParticipant = false;

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
        {
            hasActiveParticipant = true;
            warningMessage.gameObject.SetActive(false);
            LoadOrCreateParticipant(id.Trim());
        }
        else
        {
            hasActiveParticipant = false;
        }
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

                    int.TryParse(values[1], out int baselineVal);
                    int.TryParse(values[2], out int augVal);
                    int.TryParse(values[3], out int contVal);
                    UpdateConditionOrder(baselineVal, augVal, contVal);

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
            conditionOrder = new string[3];
        }
    }

    void SaveParticipantData()
    {
        string id = inputField.text.Trim();
        if (string.IsNullOrEmpty(id)) return;

        string baselineText = baselineDropdown.options[baselineDropdown.value].text;
        string augText = augOnPromptDropdown.options[augOnPromptDropdown.value].text;
        string contText = continuousAugDropdown.options[continuousAugDropdown.value].text;

        if (!int.TryParse(baselineText, out int baselineVal) ||
            !int.TryParse(augText, out int augVal) ||
            !int.TryParse(contText, out int contVal))
        {
            Debug.LogWarning("Invalid dropdown values");
            return;
        }

        List<string> updatedLines = new();
        bool updated = false;
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (line.StartsWith(id + ","))
            {
                updatedLines.Add($"{id},{baselineVal},{augVal},{contVal}");
                updated = true;
            }
            else
            {
                updatedLines.Add(line);
            }
        }

        if (!updated)
            updatedLines.Add($"{id},{baselineVal},{augVal},{contVal}");

        File.WriteAllLines(filePath, updatedLines.ToArray());

        UpdateConditionOrder(baselineVal, augVal, contVal);
    }

    void UpdateConditionOrder(int baselineVal, int augVal, int contVal)
    {
        conditionOrder = new string[3];
        if (baselineVal >= 1 && baselineVal <= 3) conditionOrder[baselineVal - 1] = "Baseline";
        if (augVal >= 1 && augVal <= 3) conditionOrder[augVal - 1] = "AugOnPrompt";
        if (contVal >= 1 && contVal <= 3) conditionOrder[contVal - 1] = "Continuous";
    }

    void SetDropdownValueFromText(TMP_Dropdown dropdown, string text)
    {
        int index = dropdown.options.FindIndex(option => option.text == text);
        dropdown.value = index >= 0 ? index : 0;
        dropdown.RefreshShownValue();
    }

    public void LaunchTutorialAndCondition1()
    {
        if (!hasActiveParticipant)
        {
            ShowWarning("Please enter Participant ID");
            return;
        }
        if (!IsConditionValid(0)) return;

        StartCoroutine(RunTutorialAndCondition(0));
    }

    public void LaunchCondition2()
    {
        if (!hasActiveParticipant)
        {
            ShowWarning("Please enter Participant ID");
            return;
        }
        if (!IsConditionValid(1)) return;

        RunCondition(conditionOrder[1]);
    }

    public void LaunchCondition3()
    {
        if (!hasActiveParticipant)
        {
            ShowWarning("Please enter Participant ID");
            return;
        }
        if (!IsConditionValid(2)) return;

        RunCondition(conditionOrder[2]);
    }

    private bool IsConditionValid(int index)
    {
        if (index < 0 || index >= conditionOrder.Length)
        {
            ShowWarning("Invalid condition index");
            return false;
        }

        string val = conditionOrder[index];
        if (string.IsNullOrEmpty(val) || !(val == "Baseline" || val == "AugOnPrompt" || val == "Continuous"))
        {
            ShowWarning("Please enter valid condition order");
            return false;
        }

        return true;
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
            Debug.LogError("Overlay or MaterialChanger missing.");
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
        }

        if (currentPhaseText) currentPhaseText.text = conditionName;

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTimer(600));
    }

    private IEnumerator StartTimer(int seconds)
    {
        int remaining = seconds;
        while (remaining > 0)
        {
            int minutes = remaining / 60;
            int secs = remaining % 60;
            if (timerText)
                timerText.text = $"{minutes:D2}:{secs:D2}";
            yield return new WaitForSeconds(1);
            remaining--;
        }

        if (timerText) timerText.text = "00:00";
        if (currentPhaseText) currentPhaseText.text = "Idle";
    }

    private void ShowWarning(string message)
    {
        if (warningMessage != null)
        {
            warningMessage.text = message;
            warningMessage.alpha = 1f;
            warningMessage.gameObject.SetActive(true);
            StartCoroutine(FadeOutWarning());
        }
    }

    private IEnumerator FadeOutWarning()
    {
        float waitTime = 1.5f;
        float fadeDuration = 0.25f;

        yield return new WaitForSeconds(waitTime);

        float elapsed = 0f;
        Color originalColor = warningMessage.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            warningMessage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        warningMessage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        warningMessage.gameObject.SetActive(false);
    }
}
