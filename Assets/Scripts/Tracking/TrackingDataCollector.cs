using UnityEngine;
using System.Collections;
using System.IO;


public class TrackingDataCollector : MonoBehaviour
{
    private OVREyeGaze leftEyeGaze;
    private OVREyeGaze rightEyeGaze;
    private OVRCameraRig ovrCameraRig;
    private StreamWriter writer;
    private float maxRaycastDistance = 500f;
    private LayerMask raycastLayerMask = Physics.DefaultRaycastLayers;


    // Controller button states
    private bool[] leftControllerButtonStates;
    private bool[] rightControllerButtonStates;
    
    void Start()
    {
      //  StartCoroutine(InitializeDataCollection());
    }



    IEnumerator InitializeDataCollectionWithCustomPath(string filePath)
    {
        ovrCameraRig = GetComponent<OVRCameraRig>();
        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig component not found!");
            yield break;
        }

        leftEyeGaze = GetComponentInChildren<OVREyeGaze>(true);
        rightEyeGaze = GetComponentInChildren<OVREyeGaze>(true);
        if (leftEyeGaze == null || rightEyeGaze == null)
        {
            Debug.LogError("Eye gaze components not found!");
            yield break;
        }

        writer = new StreamWriter(filePath, true);

        // (Write headers here like you already do)
        writer.WriteLine("Timestamp, ..."); // truncated for brevity

        StartCoroutine(CollectComprehensiveVRData());
    }

    IEnumerator CollectComprehensiveVRData()
    {
        while (true)
        {
            if (leftEyeGaze.isActiveAndEnabled && rightEyeGaze.isActiveAndEnabled)
            {
                Vector3 leftEyePosition = leftEyeGaze.transform.position;
                Vector3 rightEyePosition = rightEyeGaze.transform.position;
                string leftEyeObject = GetLookedAtObject(leftEyeGaze);
                string rightEyeObject = GetLookedAtObject(rightEyeGaze);


                Transform centerEyeAnchor = ovrCameraRig.centerEyeAnchor;
                Transform leftHandAnchor = ovrCameraRig.leftHandAnchor;
                Transform rightHandAnchor = ovrCameraRig.rightHandAnchor;


                // Get joystick positions
                Vector2 leftJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
                Vector2 rightJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);


                // Check button states
                UpdateButtonStates(OVRInput.Controller.LTouch, ref leftControllerButtonStates);
                UpdateButtonStates(OVRInput.Controller.RTouch, ref rightControllerButtonStates);


                string dataLine = $"{Time.time}," +
                                  $"{leftEyePosition.x},{leftEyePosition.y},{leftEyePosition.z}," +
                                  $"{rightEyePosition.x},{rightEyePosition.y},{rightEyePosition.z}," +
                                  $"{leftEyeObject},{rightEyeObject}," +
                                  $"{centerEyeAnchor.position.x},{centerEyeAnchor.position.y},{centerEyeAnchor.position.z}," +
                                  $"{centerEyeAnchor.rotation.x},{centerEyeAnchor.rotation.y},{centerEyeAnchor.rotation.z},{centerEyeAnchor.rotation.w}," +
                                  $"{centerEyeAnchor.eulerAngles.x},{centerEyeAnchor.eulerAngles.y},{centerEyeAnchor.eulerAngles.z}," +
                                  $"{leftHandAnchor.position.x},{leftHandAnchor.position.y},{leftHandAnchor.position.z}," +
                                  $"{leftHandAnchor.rotation.x},{leftHandAnchor.rotation.y},{leftHandAnchor.rotation.z},{leftHandAnchor.rotation.w}," +
                                  $"{leftHandAnchor.eulerAngles.x},{leftHandAnchor.eulerAngles.y},{leftHandAnchor.eulerAngles.z}," +
                                  $"{rightHandAnchor.position.x},{rightHandAnchor.position.y},{rightHandAnchor.position.z}," +
                                  $"{rightHandAnchor.rotation.x},{rightHandAnchor.rotation.y},{rightHandAnchor.rotation.z},{rightHandAnchor.rotation.w}," +
                                  $"{rightHandAnchor.eulerAngles.x},{rightHandAnchor.eulerAngles.y},{rightHandAnchor.eulerAngles.z}," +
                                  $"{leftJoystick.x},{leftJoystick.y},{rightJoystick.x},{rightJoystick.y}," +
                                  $"{string.Join(",", leftControllerButtonStates)}," +
                                  $"{string.Join(",", rightControllerButtonStates)}";


                writer.WriteLine(dataLine);
            }
            yield return new WaitForSeconds(0.01f); // Collect data every 10ms
        }
    }

    public void StopLogging()
    {
        StopAllCoroutines();
        if (writer != null)
        {
            writer.Close();
            writer = null;
        }
    }

    void UpdateButtonStates(OVRInput.Controller controller, ref bool[] buttonStates)
    {
        buttonStates[0] = OVRInput.Get(OVRInput.Button.One, controller);
        buttonStates[1] = OVRInput.Get(OVRInput.Button.Two, controller);
        buttonStates[2] = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller);
        buttonStates[3] = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller);
        buttonStates[4] = OVRInput.Get(OVRInput.Button.Start, controller);
        buttonStates[5] = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controller);
        buttonStates[6] = OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp, controller); // Using Up as a proxy for Touchpad
        buttonStates[7] = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controller); // Thumbstick press
    }


    string GetLookedAtObject(OVREyeGaze eyeGaze)
    {
        RaycastHit hit;
        if (Physics.Raycast(eyeGaze.transform.position, eyeGaze.transform.forward, out hit, maxRaycastDistance, raycastLayerMask))
        {
                        return hit.collider.gameObject.name;
        }
                
        return "None";
    }

    public void StartLogging(string filePath)
    {
        StopAllCoroutines();
        StartCoroutine(StartWithPath(filePath));
    }

    private IEnumerator StartWithPath(string filePath)
    {
        yield return new WaitUntil(() => OVRPlugin.eyeTrackingEnabled || !OVRPlugin.eyeTrackingSupported);

        StartCoroutine(InitializeDataCollectionWithCustomPath(filePath));
    }

    void OnApplicationQuit()
    {
        if (writer != null)
        {
            writer.Close();
        }
    }
}