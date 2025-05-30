using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class ParticipantManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Dropdown conditionOrderDropdown;


    public GameObject overlayObject;
    public ForemanTutorialController characterController;
    public RuntimeMaterialChange materialChanger;

    public TMP_Text timerText;
    public TMP_Text currentPhaseText;
    public TMP_Text warningMessage;

    private string filePath;
    private string participantFolderPath;
    private string currentConditionFilePath;
    private Coroutine timerCoroutine;
    private int timeRemainingInSeconds;
    public GameObject prefabToRegenerate; // Assign in Inspector
    public Transform spawnPoint; // Optional: where to place the new prefab
    public AudioSource buzzer;

    private bool hasActiveParticipant = false;
    private string[] conditionOrder = new string[3];
    private int structureCount = 0;
    public string CurrentCondition { get; private set; } = "";

    private PartsPerformanceLogger partsLogger;
    private TrackingDataCollector trackingDataCollector;
    private EyeTrackingLogger eyeTrackingLogger;




    void Start()
    {
        string dir = Path.Combine(Application.dataPath, "../ParticipantData");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        filePath = Path.Combine(dir, "participants.csv"); // <<< ADD THIS FIRST

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "ID,ConditionOrder\n");
        }

        conditionOrderDropdown.onValueChanged.AddListener(_ => SaveParticipantData());
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

            participantFolderPath = Path.Combine(Application.dataPath, "../ParticipantData", id);
            if (!Directory.Exists(participantFolderPath))
                Directory.CreateDirectory(participantFolderPath);
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
                if (values.Length >= 2)
                {
                    SetDropdownValueFromText(conditionOrderDropdown, values[1].Trim());
                    found = true;
                    UpdateConditionOrderFromDropdown(); // Make sure this uses trimmed display text
                }
                break;
            }
        }


        if (!found)
        {
            conditionOrderDropdown.value = 0;
            conditionOrderDropdown.RefreshShownValue();

            // ✅ Still good to add this here too
            UpdateConditionOrderFromDropdown();
        }
    }


    void SaveParticipantData()
    {
        string id = inputField.text.Trim();
        if (string.IsNullOrEmpty(id)) return;

        string selectedOrderText = conditionOrderDropdown.options[conditionOrderDropdown.value].text.Trim();

        List<string> updatedLines = new();
        bool updated = false;
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (line.StartsWith(id + ","))
            {
                updatedLines.Add($"{id},{selectedOrderText}");
                updated = true;
            }
            else
            {
                updatedLines.Add(line);
            }
        }

        if (!updated)
            updatedLines.Add($"{id},{selectedOrderText}");

        File.WriteAllLines(filePath, updatedLines.ToArray());

        // Ensure internal state syncs with saved value
        UpdateConditionOrderFromDropdown();
    }


    void UpdateConditionOrderFromDropdown()
    {
        string selection = conditionOrderDropdown.options[conditionOrderDropdown.value].text.Trim();

        if (selection == "Baseline -> Aug. on Prompt -> Continuous Aug.")
        {
            conditionOrder = new[] { "Baseline", "AugOnPrompt", "Continuous" };
        }
        else if (selection == "Aug. on Prompt -> Continuous Aug. -> Baseline")
        {
            conditionOrder = new[] { "AugOnPrompt", "Continuous", "Baseline" };
        }
        else if (selection == "Continuous Aug. -> Baseline -> Aug. on Prompt")
        {
            conditionOrder = new[] { "Continuous", "Baseline", "AugOnPrompt" };
        }
        else
        {
            conditionOrder = new string[0];
            ShowWarning("Invalid or missing condition order. Please select one.");
        }
    }




    void SetDropdownValueFromText(TMP_Dropdown dropdown, string text)
    {
        int index = dropdown.options.FindIndex(option => option.text.Trim() == text);
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
        UpdateConditionOrderFromDropdown(); // <<< ADD THIS LINE

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
        UpdateConditionOrderFromDropdown(); // <<< ADD THIS LINE

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
        UpdateConditionOrderFromDropdown(); // <<< ADD THIS LINE

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
        CurrentCondition = conditionName;

        buzzer.Play();
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

        structureCount = 0;
        string conditionSuffix = conditionName switch
        {
            "Baseline" => "baseline",
            "AugOnPrompt" => "augOnPrompt",
            "Continuous" => "continuousAug",
            _ => "unknown"
        };

        currentConditionFilePath = Path.Combine(participantFolderPath, $"{inputField.text.Trim()}-{conditionSuffix}-partsperformance.csv");
        if (!File.Exists(currentConditionFilePath))
            File.WriteAllText(currentConditionFilePath, "Timestamp,StructureCount,SnapOrientation,ButtonPressed,ChipStatus,DecisionCorrect\n");

        partsLogger = new PartsPerformanceLogger(currentConditionFilePath);
        // Setup tracking loggers
        if (trackingDataCollector == null)
            trackingDataCollector = GetComponent<TrackingDataCollector>();
        if (eyeTrackingLogger == null)
            eyeTrackingLogger = GetComponent<EyeTrackingLogger>();

        string motionPath = Path.Combine(participantFolderPath, $"{inputField.text.Trim()}-{conditionSuffix}-motiongaze.csv");
        string gazePath = Path.Combine(participantFolderPath, $"{inputField.text.Trim()}-{conditionSuffix}-eyelog.csv");

        if (trackingDataCollector != null)
            trackingDataCollector.StartLogging(motionPath);

        if (OVRPlugin.eyeTrackingSupported && OVRPlugin.eyeTrackingEnabled)
        {
            eyeTrackingLogger?.StartLogging(gazePath);
        }
        else
        {
            Debug.LogWarning("Gaze tracking not supported or disabled. Skipping gaze log.");
        }


        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTimer(600));

        // Find objects to track
        List<Transform> trackedObjects = new List<Transform>();

        // GameObject head = GameObject.FindWithTag("MainCamera"); // or whatever your headset is tagged
        // GameObject leftHand = GameObject.FindWithTag("LeftController"); // make sure you have proper tags
        // GameObject rightHand = GameObject.FindWithTag("RightController");

        //   if (head != null) trackedObjects.Add(head.transform);
        //  if (leftHand != null) trackedObjects.Add(leftHand.transform);
        //   if (rightHand != null) trackedObjects.Add(rightHand.transform);

        // Create motion logger

        string motionSuffix = conditionName switch
        {
            "Baseline" => "baseline-motion",
            "AugOnPrompt" => "augOnPrompt-motion",
            "Continuous" => "continuousAug-motion",
            _ => "unknown-motion"
        };

        string motionFilePath = Path.Combine(participantFolderPath, $"{inputField.text.Trim()}-{motionSuffix}.csv");
        //  motionLogger.Initialize(trackedObjects, motionFilePath);

    }

    private IEnumerator StartTimer(int seconds)
    {
        timeRemainingInSeconds = seconds;

        while (timeRemainingInSeconds > 0)
        {
            int minutes = timeRemainingInSeconds / 60;
            int secs = timeRemainingInSeconds % 60;
            if (timerText)
                timerText.text = $"{minutes:D2}:{secs:D2}";
            yield return new WaitForSeconds(1);
            timeRemainingInSeconds--;
        }

        if (timerText) timerText.text = "00:00";
        if (currentPhaseText) currentPhaseText.text = "Idle";
        buzzer.Play();

        trackingDataCollector?.StopLogging();
        eyeTrackingLogger?.StopLogging();

    }


    public void ReportSnap(bool isFlipped, string buttonPressed, string chipStatus, bool decisionCorrect)
    {
        structureCount++;
        if (partsLogger != null)
        {
            partsLogger.LogSnap(structureCount, isFlipped, timeRemainingInSeconds, buttonPressed, chipStatus, decisionCorrect);
        }
    }




    public void RestartRobotCycle()
    {
        MoveIKTarget ikTarget = FindObjectOfType<MoveIKTarget>();
        if (ikTarget != null)
        {
            ikTarget.ForceRestartCycle();
        }
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