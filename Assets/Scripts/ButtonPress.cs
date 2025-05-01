using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonPress : MonoBehaviour
{
    public GameObject button;
    public UnityEvent onPress;
    public UnityEvent onRelease;

    GameObject presser;
    public AudioSource sound;
    bool isPressed;
    Vector3 originalPos;

    private void Start()
    {
        sound = GetComponent<AudioSource>();
        isPressed = false;
        originalPos = button.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            button.transform.localPosition = new Vector3(0, 0.003f, 0);
            presser = other.gameObject;
            onPress.Invoke();
            sound.Play();
            isPressed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == presser)
        {
            button.transform.localPosition = originalPos;
            onRelease.Invoke();
            isPressed = false;
        }
    }


}
