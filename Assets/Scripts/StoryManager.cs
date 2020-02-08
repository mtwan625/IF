using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class StoryManager : MonoBehaviour
{
    public GameObject titleText;
    public GameObject startButton;
    public GameObject continueArrow;
    public TMP_InputField inputField;
    public GameObject loadingCircle;
    public GameObject yesOrNo;

    TextManager textManager;
    AnimatorManager animatorManager;
    Dictionary<string, Dialogue> branches;

    bool cont = false;
    public string pos = "01-01";
    string startPos;
    Dialogue current;

    int defiance = 0;
    bool exposed = false;

    void Awake()
    {
        textManager = GetComponent<TextManager>();
        animatorManager = GetComponent<AnimatorManager>();
        branches = StoryBranches.storyBranches;

        startPos = pos;

        SetUpStory();
    }

    void Update()
    {
        // check to see if story should proceed
        if (!cont)
            return;
        cont = false;

        // check endings
        switch(pos)
        {
            case "compliance":
                Debug.Log("compliance ending");
                pos = "";
                SetUpStory();
                return;
            case "deviance":
                Debug.Log("deviance ending");
                pos = "";
                SetUpStory();
                return;
            case "truedeviance":
                Debug.Log("true deviance ending");
                pos = "";
                SetUpStory();
                return;
            case "acceptance":
                Debug.Log("acceptance ending");
                pos = "";
                SetUpStory();
                return;
            case "trueacceptance":
                Debug.Log("true acceptance ending");
                Application.Quit();
                // note: editor only
                pos = "";
                SetUpStory();
                return;
        }

        // continue with story
        current = branches[pos];
        // Debug.Log(current.ToString());
        textManager.SetTextSpeed(current.GetTextSpeed());
        textManager.TypeText(current.ToString());

        if (current.GetResponseType() == Dialogue.ResponseType.None)
            textManager.ShowDisplayButton();
        else
            textManager.HideDisplayButton();

        // manage input system
        // Exception: if ResponseType.None, get wait time (if any), wait, the continue
        if (current.GetResponseType() == Dialogue.ResponseType.None && current.GetWaitTime() >= 0.0f)
            StartCoroutine(AutoGetNextDialogue(current.GetWaitTime()));
        else
            StartCoroutine(ShowInputField(current.GetResponseType()));
    }

    public string GetPosition()
    {
        return pos;
    }

    public void StartStory()
    {
        animatorManager.StartAnimator();
        loadingCircle.SetActive(false);
        titleText.SetActive(false);
        ContinueStory();
    }

    void ContinueStory()
    {
        cont = true;
    }

    public void GetNextDialogue(string input)
    {
        // remove input set up
        HideInputField(current.GetResponseType());

        if (current.GetResponseType() == Dialogue.ResponseType.FreeResponse)
            input = inputField.text;

        // get next
        pos = current.GetNextDialogue(input);
        if (pos == "")
            return; // end of IF

        // IDEA: (for branching) if pos contains a '/', check defiance level and select substring pos
        if (pos.Contains("/"))
        {
            if (!exposed)
            {
                if (defiance >= 2)
                {
                    pos = pos.Substring(pos.IndexOf("/") + 1);
                    exposed = true;
                    defiance = 0; // reset for next defiance stage
                }
                else
                    pos = pos.Substring(0, pos.IndexOf("/"));
            }
            else
            {
                if (defiance >= 3)
                    pos = pos.Substring(pos.IndexOf("/") + 1);
                else
                    pos = pos.Substring(0, pos.IndexOf("/"));
            }
            // Debug.Log(pos);
        }

        if (StoryBranches.defiantBranches.Contains(pos))
        {
            defiance++;
            Debug.Log("defiance updated to " + defiance);
        }

        // insert input into story
        branches[pos].InsertInput("[input]", input);

        ContinueStory();
    }

    IEnumerator AutoGetNextDialogue(float seconds)
    {
        yield return new WaitUntil(() => !textManager.IsAnimating());
        // Debug.Log("checked dialogue wait time");

        if (seconds > 0.0f)
            loadingCircle.SetActive(true);

        textManager.DisableDisplayInteractability();
        yield return new WaitForSeconds(seconds);
        textManager.EnableDisplayInteractability();

        pos = current.GetNextDialogue("");

        loadingCircle.SetActive(false);

        ContinueStory();
    }

    IEnumerator ShowInputField(Dialogue.ResponseType responseType)
    {
        yield return new WaitUntil(() => !textManager.IsAnimating());
        yield return new WaitUntil(() => !animatorManager.IsAnimating());

        switch (responseType)
        {
            case Dialogue.ResponseType.FreeResponse:
                // if ResponseType.FreeResponse, set up input text field
                textManager.DisableDisplayInteractability();
                inputField.gameObject.SetActive(true);
                inputField.ActivateInputField();
                inputField.text = "";
                break;
            case Dialogue.ResponseType.MultipleChoice:
                // if ResponseType.MultipleChoice, set up multiple choices
                break;
            case Dialogue.ResponseType.None:
                // if ResponseType.None and no wait time (== -1f), set up continue button
                continueArrow.SetActive(true);
                break;
        }
    }

    void HideInputField(Dialogue.ResponseType responseType)
    {
        switch (responseType)
        {
            case Dialogue.ResponseType.FreeResponse:
                // if ResponseType.FreeResponse, remove input text field
                inputField.gameObject.SetActive(false);
                break;
            case Dialogue.ResponseType.MultipleChoice:
                // if ResponseType.MultipleChoice, remove multiple choices
                yesOrNo.SetActive(false);
                break;
            case Dialogue.ResponseType.None:
                if (continueArrow.transform.GetChild(0).gameObject.activeSelf == true)
                    continueArrow.transform.GetChild(0).gameObject.SetActive(false); // remove "click to continue prompt"
                if (continueArrow.activeInHierarchy == true)
                    continueArrow.SetActive(false);
                break;
        }
    }

    int GenerateRandomNumber(int n)
    {
        return Random.Range(1, (int)Mathf.Pow(10, n)-1);
    }

    void SetUpStory()
    {
        exposed = false;
        defiance = 0;
        pos = startPos;
        textManager.ClearText();

        int citizenNumber = GenerateRandomNumber(6);
        int cityNumber = GenerateRandomNumber(1);

        foreach (var pair in branches)
        {
            if (pair.Value.GetText().Contains("[rand_6_digit_num]"))
                pair.Value.InsertInput("[rand_6_digit_num]", citizenNumber.ToString("D6"));
            if (pair.Value.GetText().Contains("[rand_1_digit_num]"))
                pair.Value.InsertInput("[rand_1_digit_num]", cityNumber.ToString());
        }

        titleText.SetActive(true);
        inputField.gameObject.SetActive(false);
        startButton.SetActive(true);
        loadingCircle.SetActive(true);
    }
}

public class Dialogue
{
    string text;
    float textSpeed = 0.05f;
    ResponseType responseType;
    List<string> next;
    Var extra;

    public Dialogue(string text, ResponseType responseType, List<string> next, Var extra)
    {
        this.text = text;
        this.responseType = responseType;
        this.next = next;
        this.extra = extra;
    }

    public Dialogue(string text, float textSpeed, ResponseType responseType, List<string> next, Var extra)
    {
        this.text = text;
        this.textSpeed = textSpeed;
        this.responseType = responseType;
        this.next = next;
        this.extra = extra;
    }

    public string GetNextDialogue(string input)
    {
        switch(responseType)
        {
            case ResponseType.FreeResponse:
                // check to see if input is in extra (valid) list
                // use regex to distinguish validity
                // Debug.Log(input);
                if (extra.valid[0].Contains("[r]"))
                {
                    // Debug.Log("regex check");
                    string rgx = extra.valid[0].Substring(3);
                    // Debug.Log(rgx);
                    bool match = Regex.IsMatch(input.Trim(), rgx);

                    if (match)
                        return next[0]; // submissive story branch
                    return next[1]; // defiant story branch
                }

                // otherwise, use exact input in valid list
                if (extra.valid.Contains(input.ToLower().Trim()))
                    return next[0]; // submissive story branch
                return next[1]; // defiant story branch

            case ResponseType.MultipleChoice:
                // retrieve new story position based on choice (as key)
                if (extra.choices.ContainsKey(input.ToLower().Trim()))
                    return extra.choices[input];
                break;
            case ResponseType.None:
                return next[0]; // go to next dialogue immediately
        }
        return "-1";
    }

    public string GetText()
    {
        return text;
    }

    public float GetTextSpeed()
    {
        return textSpeed;
    }

    public ResponseType GetResponseType()
    {
        return responseType;
    }

    public void InsertInput(string replacement, string input)
    {
        text = text.Replace(replacement, input);
    }

    // only for ResponseType.None dialogues
    public float GetWaitTime()
    {
        return responseType == ResponseType.None ? extra.time : 0.0f;
    }

    public override string ToString()
    {
        return text;
    }
    
    public enum ResponseType
    {
        FreeResponse,
        MultipleChoice,
        None
    }
}

public class Var
{
    public List<string> valid;
    public Dictionary<string, string> choices;
    public float time;

    // for ResponseType.FreeResponse
    public Var(List<string> valid)
    {
        this.valid = valid;
    }

    // for ResponseType.MultipleChoice
    public Var(Dictionary<string, string> choices)
    {
        this.choices = choices;
    }

    // for ResponseType.None
    public Var(float time)
    {
        this.time = time;
    }
}