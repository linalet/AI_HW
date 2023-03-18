using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInfoToggler : MonoBehaviour
{
    [SerializeField]
    private KeyCode keyToToggleDebugInfo = KeyCode.D;

    [SerializeField]
    private string tagToToggle = "Debug";

    [SerializeField]
    private bool currentState = true;

    private GameObject[] objectsToToggle;

    private void Start()
    {
        FindObjectsToToggle();
        UpdateState();
    }

    private void Update()
    {
        if(Input.GetKeyUp(keyToToggleDebugInfo))
        {
            currentState = !currentState;

            UpdateState();
        }
    }

    public void FindObjectsToToggle()
    {
        objectsToToggle = GameObject.FindGameObjectsWithTag(tagToToggle);
    }

    private void UpdateState()
    {
        for (int i = 0; i < objectsToToggle.Length; ++i)
        {
            objectsToToggle[i].SetActive(currentState);
        }
    }
}
