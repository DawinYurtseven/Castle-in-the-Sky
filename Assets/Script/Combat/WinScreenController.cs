using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenController : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private float transitionTime = 0.5f;
    [SerializeField] private List<GameObject> createdObjects;

    //called when the game is won
    private void OnEnable()
    {
        var stats = Instantiate(buttonPrefab,transform);
        stats.transform.localPosition = new Vector3(2500, 500);
        stats.name = "Stats";
        stats.transform.DOLocalMove(new Vector3(1250, 500), transitionTime).SetEase(Ease.InExpo);
        stats.GetComponentInChildren<TMP_Text>().text = "STATS";
        stats.GetComponent<Button>().onClick.AddListener(OnStats);
        createdObjects.Add(stats);
        
        var skills = Instantiate(buttonPrefab,transform);
        skills.transform.localPosition = new Vector3(2500, 0);
        skills.name = "Skills";
        skills.transform.DOLocalMove(new Vector3(1250, 0), transitionTime * 1.2f).SetEase(Ease.InExpo);
        skills.GetComponentInChildren<TMP_Text>().text = "SKILLS";
        skills.GetComponent<Button>().onClick.AddListener(OnSkills);
        createdObjects.Add(skills);
        
        var items = Instantiate(buttonPrefab,transform);
        items.transform.localPosition = new Vector3(2500, -500);
        items.name = "Items";
        items.transform.DOLocalMove(new Vector3(1250, -500), transitionTime * 1.4f).SetEase(Ease.InExpo);
        items.GetComponentInChildren<TMP_Text>().text = "ITEMS";
        items.GetComponent<Button>().onClick.AddListener(OnItems);
        createdObjects.Add(items);
    }


    private void OnStats()
    {
        StartCoroutine(ClearAllButtons());
    }
    
    private void OnSkills()
    {
        StartCoroutine(ClearAllButtons());
    }
    
    private void OnItems()
    {
        StartCoroutine(ClearAllButtons());
        
        //make new 3 buttons for each item
    }

    
    //TODO: Why are you doing this with code when you can do this with animations.
    private IEnumerator ClearAllButtons()
    {
        for (int i = 0; i < createdObjects.Count-1; i++)
        {
            var obj = createdObjects[i];
            obj.transform.DOLocalMove(obj.transform.localPosition + new Vector3(-5000, 0), transitionTime * (1f + 0.2f * i)).SetEase(Ease.Linear);
        }
        var lastObj = createdObjects[^1];
        yield return lastObj.transform.DOLocalMove(lastObj.transform.localPosition + new Vector3(-5000, 0), transitionTime * (1f + 0.2f * (createdObjects.Count - 1)))
            .SetEase(Ease.Linear).WaitForCompletion();

        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            Destroy(createdObjects[i]);
        }
        createdObjects.Clear();
    }
}
