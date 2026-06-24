using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterGrowth : MonoBehaviour
{
    [SerializeField] private List<TMP_Text> texts = new ();
    [SerializeField] public List<string> statTexts = new ();
    [SerializeField] public Image profile;

    public void FillText(int index)
    {
        StartCoroutine(FillTextOverTime(texts[index], statTexts[index]));
    }

    private static IEnumerator FillTextOverTime(TMP_Text text, string textfill)
    {
        var time = 0f;
        var totalChars = textfill.Length;
        while (time < 1)
        {
            time += Time.deltaTime;
            var amount = Mathf.Clamp01(time / 1);
            var textShow = Mathf.RoundToInt(Mathf.Lerp(0, totalChars, amount));
            text.text = textfill[..textShow];
            yield return null;
        }
        text.text = textfill;
    }
}
