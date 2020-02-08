using UnityEngine;
using TMPro;

public class ClickableText : MonoBehaviour
{
    public Color color = Color.white;

    StoryManager storyManager;
    TextManager textManager;

    TextMeshProUGUI text;

    int lastUpdateIndex = -1;

    void Awake()
    {
        storyManager = GameObject.Find("Game Manager").GetComponent<StoryManager>();
        textManager = GameObject.Find("Game Manager").GetComponent<TextManager>();

        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (textManager.IsAnimating())
            return;

        if (Input.GetMouseButtonDown(0))
            ClickLink();

        int index = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);

        if (index == lastUpdateIndex)
            return;

        if (index > -1)
        {
            if (lastUpdateIndex > -1)
            {
                text.text = text.text.Replace("<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">", "").Replace("</color>", "");
                lastUpdateIndex = index;
            }

            TMP_LinkInfo info = text.textInfo.linkInfo[index];

            int realIndex = text.text.IndexOf("<link=" + info.GetLinkID() + ">");
            // Debug.Log(info.linkTextfirstCharacterIndex + " " + info.linkIdLength + " " + info.linkTextLength);
            text.text = text.text.Insert(realIndex + info.linkIdLength + info.linkTextLength + 7, "</color>").
                Insert(realIndex + info.linkIdLength + 7, "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">");

            lastUpdateIndex = index;
        }
        else
        {
            // Debug.Log("off hover");
            text.text = text.text.Replace("<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">", "").Replace("</color>", "");
            lastUpdateIndex = index;
        }
    }

    void ClickLink()
    {
        int index = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (index > -1)
        {
            TMP_LinkInfo info = text.textInfo.linkInfo[index];
            string input = info.GetLinkID(); // <link = input> input </link>

            storyManager.GetNextDialogue(input);
        }
    }
}
