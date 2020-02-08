using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class TextManager : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public Button displayButton;
    public float textSpeed = 0.05f;

    bool animatingText = false;

    void Awake()
    {
        DisableDisplayInteractability();
    }

    public void TypeText(string text)
    {
        StartCoroutine(AnimateText(text));
    }

    public void SetTextSpeed(float textSpeed)
    {
        this.textSpeed = textSpeed;
    }

    IEnumerator AnimateText(string text)
    {
        animatingText = true;
        string cleanText = Regex.Replace(text, @"<[^>]*>", "");
        cleanText = cleanText.Replace("Save her...", ""); // note: special case
        DisableDisplayInteractability();
        for (int i = 1; i < cleanText.Length + 1; i++)
        {
            displayText.text = cleanText.Substring(0, i);
            yield return new WaitForSeconds(textSpeed);
        }
        displayText.text = text;
        EnableDisplayInteractability();
        animatingText = false;
    }

    public bool IsAnimating()
    {
        return animatingText;
    }

    public void DisableDisplayInteractability()
    {
        displayButton.interactable = false;
    }

    public void EnableDisplayInteractability()
    {
        displayButton.interactable = true;
    }

    public void HideDisplayButton()
    {
        displayButton.gameObject.SetActive(false);
    }

    public void ShowDisplayButton()
    {
        displayButton.gameObject.SetActive(true);
    }

    public void ClearText()
    {
        displayText.text = "";
    }
}
