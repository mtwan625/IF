using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomSubmit : MonoBehaviour
{
    public StoryManager storyManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            Submit();
    }

    void Submit()
    {
        storyManager.GetNextDialogue(GetComponent<TMP_InputField>().text);
    }

    public void OnDeselect()
    {
        GetComponent<TMP_InputField>().Select();
        GetComponent<TMP_InputField>().ActivateInputField();
    }
}
