using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kino;

public class AnimatorManager : MonoBehaviour
{
    public TMP_FontAsset glitchFont;

    GameObject canvas;
    StoryManager storyManager;
    TextManager textManager;
    string pos;
    bool isAnimating = false;

    Dictionary<string, System.Action> animations = new Dictionary<string, System.Action>();

    ColorBlock colorBlock;
    TMP_FontAsset defaultFont;
    DigitalGlitch digitalGlitch;

    void Awake()
    {
        canvas = GameObject.Find("Canvas");
        storyManager = GetComponent<StoryManager>();
        textManager = GetComponent<TextManager>();

        colorBlock = canvas.transform.Find("Industry Options").Find("Security").GetComponent<Button>().colors;
        TextMeshProUGUI displayText = canvas.transform.Find("Display Text").GetComponent<TextMeshProUGUI>();
        defaultFont = displayText.font;
        digitalGlitch = GameObject.Find("Main Camera").GetComponent<DigitalGlitch>();

        #region adding animation functions
        animations.Add("01-02", Scan);
        animations.Add("01-10", DisplayUserInformation);
        animations.Add("01-11", DisplayIndustryOptions);
        animations.Add("01-12", LimitIndustryOptions);
        animations.Add("01-13", FillIndustryLinks);
        animations.Add("01a-01", RemoveIndustryOptions);
        animations.Add("01b-01", RemoveIndustryOptions);
        animations.Add("01b-02", GlitchText);

        animations.Add("02-05", DisplayOccupationOptions);
        animations.Add("02-06", FillOccupationLinks);
        animations.Add("02a-01", RemoveOccupationOptions);
        animations.Add("02b-01", RemoveOccupationLinks);

        animations.Add("04-01", RemoveOccupationOptions);
        animations.Add("04-03", DisplayYesOrNo);
        animations.Add("04a-05", GlitchText);

        animations.Add("05-01", GlitchText);
        animations.Add("05-02", DisplayYesOrNo);
        animations.Add("05a-01", GlitchText);
        animations.Add("05a-02", GlitchText);
        animations.Add("05a-03", GlitchText);
        animations.Add("05b-07", GlitchText);

        animations.Add("08a-05", GlitchText);

        animations.Add("10a-05", GlitchText);
        animations.Add("10c-01", GlitchText);
        #endregion

        // TODO: add animations for all endings
    }

    public void StartAnimator()
    {
        StartCoroutine(Animate());
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }

    IEnumerator Animate()
    {
        while (pos != "")
        {
            yield return new WaitUntil(() => storyManager.GetPosition() != pos);
            pos = storyManager.GetPosition();

            // Debug.Log(pos);
            yield return new WaitUntil(() => !textManager.IsAnimating());
            // Debug.Log("animate for " + pos);

            if (animations.ContainsKey(pos))
            {
                isAnimating = true;
                animations[pos]();
            }
        }
        Debug.Log("end of story");
    }

    #region animation functions
    void Scan()
    {
        Animator scanline = canvas.transform.Find("Scanline").GetComponent<Animator>();
        scanline.gameObject.SetActive(true);

        AnimatorClipInfo info = scanline.GetCurrentAnimatorClipInfo(0)[0];
        float seconds = info.clip.length;

        StartCoroutine(DisableGameObject(scanline.gameObject, seconds, true));
    }

    void DisplayUserInformation()
    {
        // move display text up
        Animator displayText = canvas.transform.Find("Display Text").GetComponent<Animator>();
        displayText.SetTrigger("slideUp");

        // fill user image
        GameObject image = canvas.transform.Find("Citizen Info").gameObject;
        image.SetActive(true);

        isAnimating = false;
    }
    
    void DisplayIndustryOptions()
    {
        // remove user image
        Animator image = canvas.transform.Find("Citizen Info").GetComponent<Animator>();
        image.SetTrigger("remove");
        AnimatorClipInfo info = image.GetCurrentAnimatorClipInfo(0)[0];
        float seconds = info.clip.length;
        StartCoroutine(DisableGameObject(image.gameObject, seconds, false));

        // display industry buttons
        GameObject industry = canvas.transform.Find("Industry Options").gameObject;
        industry.SetActive(true);

        isAnimating = false;
    }

    void LimitIndustryOptions()
    {
        GameObject industry = canvas.transform.Find("Industry Options").gameObject;
        foreach(Transform child in industry.transform)
        {
            // change color of only one
            if (child.gameObject.name == "Security")
            {
                Button button = child.GetComponent<Button>();
                ColorBlock colors = button.colors;
                colors.disabledColor = colors.normalColor;
                button.colors = colors;
            }
        }

        isAnimating = false;
    }

    void FillIndustryLinks()
    {
        GameObject industry = canvas.transform.Find("Industry Options").gameObject;
        foreach (Transform child in industry.transform)
        {
            // make all interactable
            Button button = child.GetComponent<Button>();
            // button.interactable = true;

            // make correct links
            TextMeshProUGUI text = child.transform.GetComponentInChildren<TextMeshProUGUI>();
            if (child.gameObject.name == "Security")
                text.text = text.text.Insert(text.text.Length, "</link>").Insert(0, "<link=correct>");
            else
            {
                text.text = text.text.Insert(text.text.Length, "</link>").Insert(0, "<link=incorrect>");

                ColorBlock colors = button.colors;
                colors.normalColor = colors.disabledColor;
                colors.highlightedColor = colors.disabledColor;
                colors.pressedColor = colors.disabledColor;
                colors.selectedColor = colors.disabledColor;
                button.colors = colors;
            }
        }

        isAnimating = false;
    }

    void RemoveIndustryLinks()
    {
        GameObject industry = canvas.transform.Find("Industry Options").gameObject;
        foreach (Transform child in industry.transform)
        {
            // make all interactable
            Button button = child.GetComponent<Button>();
            // button.interactable = true;

            // make correct links
            TextMeshProUGUI text = child.transform.GetComponentInChildren<TextMeshProUGUI>();
            if (child.gameObject.name == "Security")
                text.text = text.text.Replace("</link>", "").Replace("<link=correct>", "");
            else
                text.text = text.text.Replace("</link>", "").Replace("<link=incorrect>", "");
        }
    }

    void RemoveIndustryOptions()
    {
        if (pos == "01b-01")
            GlitchText();

        RemoveIndustryLinks();

        Animator industry = canvas.transform.Find("Industry Options").GetComponent<Animator>();
        industry.transform.Find("Security").GetComponent<Button>().colors = colorBlock;
        industry.SetTrigger("remove");

        AnimatorClipInfo info = industry.GetCurrentAnimatorClipInfo(0)[0];
        float seconds = info.clip.length;
        StartCoroutine(DisableGameObject(industry.gameObject, seconds, false));

        Animator displayText = canvas.transform.Find("Display Text").GetComponent<Animator>();
        displayText.SetTrigger("slideDown");

        isAnimating = false;
    }

    void GlitchText()
    {
        StartCoroutine(Glitch());
    }

    void DisplayOccupationOptions()
    {
        // move display text up
        Animator displayText = canvas.transform.Find("Display Text").GetComponent<Animator>();
        displayText.SetTrigger("slideUp");

        // display occupation buttons
        GameObject occupation = canvas.transform.Find("Occupation Options").gameObject;
        occupation.SetActive(true);

        foreach (Transform child in occupation.transform)
        {
            // change color of all but one
            if (child.gameObject.name != "Next")
            {
                Button button = child.GetComponent<Button>();
                ColorBlock colors = button.colors;
                colors.disabledColor = colors.normalColor;
                button.colors = colors;
            }
        }

        isAnimating = false;
    }

    void FillOccupationLinks()
    {
        GameObject occupation = canvas.transform.Find("Occupation Options").gameObject;
        foreach (Transform child in occupation.transform)
        {
            // make all interactable
            Button button = child.GetComponent<Button>();
            // button.interactable = true;

            // make correct links
            TextMeshProUGUI text = child.transform.GetComponentInChildren<TextMeshProUGUI>();
            if (child.gameObject.name == "Next")
                text.text = text.text.Insert(text.text.Length, "</link>").Insert(0, "<link=next>");
            else
                text.text = text.text.Insert(text.text.Length, "</link>").Insert(0, "<link=any>");
        }

        isAnimating = false;
    }

    void RemoveOccupationLinks()
    {
        GameObject occupation = canvas.transform.Find("Occupation Options").gameObject;
        foreach (Transform child in occupation.transform)
        {
            // make all interactable
            Button button = child.GetComponent<Button>();
            // button.interactable = true;

            // make correct links
            TextMeshProUGUI text = child.transform.GetComponentInChildren<TextMeshProUGUI>();
            if (child.gameObject.name == "Next")
                text.text = text.text.Replace("</link>", "").Replace("<link=next>", "");
            else
                text.text = text.text.Replace("</link>", "").Replace("<link=any>", "");
        }

        if (pos == "02b-01")
            isAnimating = false;
    }

    void RemoveOccupationOptions()
    {
        if (pos == "04-01")
            GlitchText();

        RemoveOccupationLinks();

        Animator occupation = canvas.transform.Find("Occupation Options").GetComponent<Animator>();
        occupation.SetTrigger("remove");

        AnimatorClipInfo info = occupation.GetCurrentAnimatorClipInfo(0)[0];
        float seconds = info.clip.length;
        StartCoroutine(DisableGameObject(occupation.gameObject, seconds, false));

        Animator displayText = canvas.transform.Find("Display Text").GetComponent<Animator>();
        displayText.SetTrigger("slideDown");

        isAnimating = false;
    }

    void DisplayYesOrNo()
    {
        GameObject yesOrNo = canvas.transform.Find("Yes No").gameObject;
        yesOrNo.SetActive(true);
        isAnimating = false;

        if (pos == "05-02")
            GlitchText();
    }

    IEnumerator DisableGameObject(GameObject gameObject, float seconds, bool endAnimation)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
        if (endAnimation)
            isAnimating = false;
    }
    
    IEnumerator Glitch()
    {
        string currentPos = pos;
        TextMeshProUGUI displayText = canvas.transform.Find("Display Text").GetComponent<TextMeshProUGUI>();
        while (pos == currentPos)
        {
            digitalGlitch.intensity = Random.Range(0.01f, 0.02f);
            displayText.font = glitchFont;
            yield return new WaitForSeconds(Random.Range(0.02f, 0.075f));
            displayText.font = defaultFont;
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            isAnimating = false;
        }
        displayText.font = defaultFont;
        digitalGlitch.intensity = 0.0f;
    }
    #endregion
}
