using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenController : MonoBehaviour
{
    private static readonly int Expand = Animator.StringToHash("Expand");
    private static readonly int Collapse = Animator.StringToHash("Collapse");
    private static readonly int Exit = Animator.StringToHash("Exit");
    private static readonly int Enter = Animator.StringToHash("Enter");
    [SerializeField] private GameObject buttonPrefab, itemSelectPrefab;
    [SerializeField] private float transitionTime = 0.5f;
    [SerializeField] private List<GameObject> createdObjects;
    
    private RectTransform rectTransform;

    //called when the game is won
    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        
        //replace with animations
        
        var stats = Instantiate(buttonPrefab,transform);
        stats.transform.localPosition = new Vector3(rectTransform.rect.width * 1.5f,rectTransform.rect.height * 0.25f );
        stats.name = "Stats";
        stats.GetComponentInChildren<TMP_Text>().text = "STATS";
        stats.GetComponent<Animator>().SetTrigger(Enter);
        stats.GetComponent<Button>().onClick.AddListener(OnStats);
        createdObjects.Add(stats);
        
        var skills = Instantiate(buttonPrefab,transform);
        skills.transform.localPosition = new Vector3(rectTransform.rect.width * 1.5f, 0);
        skills.name = "Skills";
        skills.GetComponentInChildren<TMP_Text>().text = "SKILLS";
        skills.GetComponent<Animator>().SetTrigger(Enter);
        skills.GetComponent<Button>().onClick.AddListener(OnSkills);
        createdObjects.Add(skills);
        
        var items = Instantiate(buttonPrefab,transform);
        items.transform.localPosition = new Vector3(rectTransform.rect.width * 1.5f, rectTransform.rect.height * -0.25f);
        items.name = "Items";
        items.GetComponentInChildren<TMP_Text>().text = "ITEMS";
        items.GetComponent<Animator>().SetTrigger(Enter);
        items.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OnItems()));
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
    
    private IEnumerator OnItems()
    {
        yield return ClearAllButtons();
        
        var item1 = Instantiate(itemSelectPrefab, transform);
        item1.transform.localPosition = new Vector3(-rectTransform.rect.width * 0.3f, 0);
        var item2 = Instantiate(itemSelectPrefab, transform);
        item2.transform.localPosition = new Vector3(0, 0);
        var item3 = Instantiate(itemSelectPrefab, transform);
        item3.transform.localPosition = new Vector3(rectTransform.rect.width * 0.3f, 0);

        //generate 3 random items
        var i1 = new StrengthPendant();
        var i2 = new LuckPendant();
        var i3 = new SpeedPendant();
        
        //expanding the item tabs
        var animator1 = item1.GetComponent<Animator>();
        animator1.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator1.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator1.IsInTransition(0));
        StartCoroutine(InsertTextIntoObject(item1.transform.GetChild(0).GetComponent<TMP_Text>(), i1.ItemName));
        StartCoroutine(InsertTextIntoObject(item1.transform.GetChild(1).GetComponent<TMP_Text>(), i1.ItemDescription));
        
        var animator2 = item2.GetComponent<Animator>();
        animator2.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator2.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator2.IsInTransition(0));
        StartCoroutine(InsertTextIntoObject(item2.transform.GetChild(0).GetComponent<TMP_Text>(), i2.ItemName));
        StartCoroutine(InsertTextIntoObject(item2.transform.GetChild(1).GetComponent<TMP_Text>(), i2.ItemDescription));
        
        var animator3 = item3.GetComponent<Animator>();
        animator3.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator3.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator3.IsInTransition(0));
        StartCoroutine(InsertTextIntoObject(item3.transform.GetChild(0).GetComponent<TMP_Text>(), i3.ItemName));
        StartCoroutine(InsertTextIntoObject(item3.transform.GetChild(1).GetComponent<TMP_Text>(), i3.ItemDescription));
        var unit = new List<Unit>(BattleSystem.system.playerUnits);
        item1.GetComponent<Button>().onClick.AddListener(() =>
        {
            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.Add(item2);
            others.Add(item3);
            otherAnimators.Add(animator2);
            otherAnimators.Add(animator3);
            StartCoroutine(OnTimeClickEvent(i1, item1, others, animator1, otherAnimators));
            
            //TODO: MoveToMap()
        });
        item2.GetComponent<Button>().onClick.AddListener(() =>
        {
            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.Add(item1);
            others.Add(item3);
            otherAnimators.Add(animator1);
            otherAnimators.Add(animator3);
            StartCoroutine(OnTimeClickEvent(i2, item2, others, animator2, otherAnimators));

            //TODO: MoveToMap()
        });
        item3.GetComponent<Button>().onClick.AddListener(() =>
        {
            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.Add(item1);
            others.Add(item2);
            otherAnimators.Add(animator1);
            otherAnimators.Add(animator2);
            StartCoroutine(OnTimeClickEvent(i3, item3, others, animator3, otherAnimators));
            
            //TODO: MoveToMap()

        });

        //await animations type shit.

        //make new 3 buttons for each item
    }
    
    //TODO: make a MoveToMap() function

    private IEnumerator OnTimeClickEvent(Items ie, GameObject target, List<GameObject> others, Animator animator,
        List<Animator> otherAnimators)
    {
        var unit = new List<Unit>(BattleSystem.system.playerUnits);
        ie.Acquire(unit);
        for (int i = 0; i < others.Count; i++)
        {
            StartCoroutine(ClearTextInObject(others[i].transform.GetChild(0).GetComponent<TMP_Text>()));
            if(i != others.Count - 1)
                StartCoroutine(ClearTextInObject(others[i].transform.GetChild(1).GetComponent<TMP_Text>()));
            else
                yield return ClearTextInObject(others[i].transform.GetChild(1).GetComponent<TMP_Text>());
        }
        foreach (var ani in otherAnimators)
        {
            ani.SetTrigger(Collapse);
        }
        
        yield return target.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InExpo).WaitForCompletion();
        yield return new WaitForSeconds(1f);
        StartCoroutine(ClearTextInObject(target.transform.GetChild(0).GetComponent<TMP_Text>()));
        yield return ClearTextInObject(target.transform.GetChild(1).GetComponent<TMP_Text>());
        animator.SetTrigger(Collapse);
    }

    private IEnumerator InsertTextIntoObject(TMP_Text textObj, string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            textObj.text += text[i];
            yield return null;
        }
    }

    private IEnumerator ClearTextInObject(TMP_Text textObj)
    {
        while(textObj.text.Length > 0)
        {
            textObj.text = textObj.text.Substring(0, textObj.text.Length - 1);
            yield return null;
        }
    }

    
    //TODO: Why are you doing this with code when you can do this with animations.
    private IEnumerator ClearAllButtons()
    {
        for (int i = 0; i < createdObjects.Count-1; i++)
        {
            createdObjects[i].GetComponent<Animator>().SetTrigger(Exit);
        }
        var length = createdObjects[0].GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(length + 0.2f);

        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            Destroy(createdObjects[i]);
        }
        createdObjects.Clear();
    }
}
