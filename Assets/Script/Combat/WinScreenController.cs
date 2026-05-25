using System;
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
    [SerializeField] private GameObject buttonPrefab,statSelectPrefab, skillsSelectPrefab ,itemSelectPrefab;
    [SerializeField] private List<GameObject> createdObjects;
    [SerializeField] public PlayerUnit mainCharacter;
    
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
        stats.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OnStats()));
        createdObjects.Add(stats);
        
        var skills = Instantiate(buttonPrefab,transform);
        skills.transform.localPosition = new Vector3(rectTransform.rect.width * 1.5f, 0);
        skills.name = "Skills";
        skills.GetComponentInChildren<TMP_Text>().text = "SKILLS";
        skills.GetComponent<Animator>().SetTrigger(Enter);
        skills.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OnSkills()));
        createdObjects.Add(skills);
        
        var items = Instantiate(buttonPrefab,transform);
        items.transform.localPosition = new Vector3(rectTransform.rect.width * 1.5f, rectTransform.rect.height * -0.25f);
        items.name = "Items";
        items.GetComponentInChildren<TMP_Text>().text = "ITEMS";
        items.GetComponent<Animator>().SetTrigger(Enter);
        items.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OnItems()));
        createdObjects.Add(items);
    }


    private IEnumerator OnStats()
    {
        yield return ClearAllButtons();
        var stats = Instantiate(statSelectPrefab, transform);
        stats.transform.localPosition = new Vector3(0, 0);
        
        var anim = stats.GetComponent<Animator>();
        anim.SetTrigger(Expand);
        yield return null;
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0));

        var statlist = mainCharacter.GetStats();
        int allocatable = 10;
        stats.transform.GetChild(5).gameObject.SetActive(true);
        for (int i = 0; i < 5; i++)
        {
            var button = stats.transform.GetChild(i).gameObject;
            button.SetActive(true);
            yield return InsertTextIntoObject(button.GetComponent<TMP_Text>(),$"{statlist[i].Item1} => {statlist[i].Item2}");
            var minus = button.transform.GetChild(0);
            minus.gameObject.SetActive(true);
            
            //TODO: add some chance that a stat is not interactable
            
            minus.GetComponent<Button>().interactable = false;
            var plus = button.transform.GetChild(1);
            plus.gameObject.SetActive(true);

            var i1 = i;
            minus.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (allocatable < 10 && mainCharacter.GetStat(statlist[i1].Item1) > statlist[i1].Item2)
                {
                    mainCharacter.IncreaseStat(statlist[i1].Item1, -1);
                    allocatable++;
                    stats.transform.GetChild(5).GetComponent<TMP_Text>().text = allocatable.ToString();
                    button.GetComponent<TMP_Text>().text = $"{statlist[i1].Item1} => {mainCharacter.GetStat(statlist[i1].Item1)}";
                    if(mainCharacter.GetStat(statlist[i1].Item1) == statlist[i1].Item2) minus.GetComponent<Button>().interactable = false;
                }
            });

            var i2 = i;
            plus.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (allocatable > 0)
                {
                    mainCharacter.IncreaseStat(statlist[i2].Item1, 1);
                    allocatable--;
                    stats.transform.GetChild(5).GetComponent<TMP_Text>().text = allocatable.ToString();
                    button.GetComponent<TMP_Text>().text = $"{statlist[i1].Item1} => {mainCharacter.GetStat(statlist[i1].Item1)}";
                    if(!minus.GetComponent<Button>().interactable) minus.GetComponent<Button>().interactable = true;
                }
            });
        }
        
        stats.transform.GetChild(6).gameObject.SetActive(true);
        stats.transform.GetChild(6).GetComponent<Button>().onClick.AddListener(() =>
        {
            if (allocatable == 0) 
            {
                //TODO: end the screen and go to menu
                Debug.Log("YEeeees");
            }
           
        });
    }
    
    private IEnumerator OnSkills()
    {
        yield return ClearAllButtons();
        
        var skill1 = Instantiate(skillsSelectPrefab, transform);
        skill1.transform.localPosition = new Vector3(-rectTransform.rect.width * 0.3f, 0);
        var skill2 = Instantiate(skillsSelectPrefab, transform);
        skill2.transform.localPosition = new Vector3(0, 0);
        var skill3 = Instantiate(skillsSelectPrefab, transform);
        skill3.transform.localPosition = new Vector3(rectTransform.rect.width * 0.3f, 0);
        
        List<Skill> skills = new List<Skill>();
        for (int i = 0; i < 3; i++)
        {
            skills.Add(Skill.GetRandomSkill(skills));
        }
        
        //expanding the item tabs
        var animator1 = skill1.GetComponent<Animator>();
        animator1.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator1.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator1.IsInTransition(0));
        skill1.transform.GetChild(0).gameObject.SetActive(true);
        skill1.transform.GetChild(1).gameObject.SetActive(true);
        skill1.transform.GetChild(2).gameObject.SetActive(true);
        StartCoroutine(InsertTextIntoObject(skill1.transform.GetChild(1).GetComponentInChildren<TMP_Text>(), skills[0].skillName));
        StartCoroutine(InsertTextIntoObject(skill1.transform.GetChild(0).GetComponentInChildren<TMP_Text>(), skills[0].skillDescription));
        StartCoroutine(InsertTextIntoObject(skill1.transform.GetChild(2).GetComponentInChildren<TMP_Text>(),
            skills[0].skillCost.ToString()));
        
        var animator2 = skill2.GetComponent<Animator>();
        animator2.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator2.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator2.IsInTransition(0));
        skill2.transform.GetChild(0).gameObject.SetActive(true);
        skill2.transform.GetChild(1).gameObject.SetActive(true);
        skill2.transform.GetChild(2).gameObject.SetActive(true);
        StartCoroutine(InsertTextIntoObject(skill2.transform.GetChild(1).GetComponentInChildren<TMP_Text>(), skills[1].skillName));
        StartCoroutine(InsertTextIntoObject(skill2.transform.GetChild(0).GetComponentInChildren<TMP_Text>(), skills[1].skillDescription));
        StartCoroutine(InsertTextIntoObject(skill2.transform.GetChild(2).GetComponentInChildren<TMP_Text>(), skills[1].skillCost.ToString()));
        
        var animator3 = skill3.GetComponent<Animator>();
        animator3.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator3.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator3.IsInTransition(0));
        skill3.transform.GetChild(0).gameObject.SetActive(true);
        skill3.transform.GetChild(1).gameObject.SetActive(true);
        skill3.transform.GetChild(2).gameObject.SetActive(true);
        StartCoroutine(InsertTextIntoObject(skill3.transform.GetChild(1).GetComponentInChildren<TMP_Text>(), skills[2].skillName));
        StartCoroutine(InsertTextIntoObject(skill3.transform.GetChild(0).GetComponentInChildren<TMP_Text>(), skills[2].skillDescription));
        StartCoroutine(InsertTextIntoObject(skill3.transform.GetChild(2).GetComponentInChildren<TMP_Text>(), skills[2].skillCost.ToString()));
        
        
        skill1.GetComponent<Button>().onClick.AddListener(() =>
        {
            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.Add(skill2);
            others.Add(skill3);
            otherAnimators.Add(animator2);
            otherAnimators.Add(animator3);
            
            mainCharacter.AddSkill(skills[0]);

            skill3.transform.GetChild(0).gameObject.SetActive(false);
            skill3.transform.GetChild(1).gameObject.SetActive(false);
            skill3.transform.GetChild(2).gameObject.SetActive(false);
            
            skill2.transform.GetChild(0).gameObject.SetActive(false);
            skill2.transform.GetChild(1).gameObject.SetActive(false);
            skill2.transform.GetChild(2).gameObject.SetActive(false);

            StartCoroutine(OnTimeClickEvent( skill1, others, animator1, otherAnimators,() =>{}));
            
            //TODO: MoveToMap()
        });
        skill2.GetComponent<Button>().onClick.AddListener(() =>
        {
            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.Add(skill1);
            others.Add(skill3);
            otherAnimators.Add(animator1);
            otherAnimators.Add(animator3);
            
            mainCharacter.AddSkill(skills[1]);
            skill3.transform.GetChild(0).gameObject.SetActive(false);
            skill3.transform.GetChild(1).gameObject.SetActive(false);
            skill3.transform.GetChild(2).gameObject.SetActive(false);
            
            skill1.transform.GetChild(2).gameObject.SetActive(false);
            skill1.transform.GetChild(0).gameObject.SetActive(false);
            skill1.transform.GetChild(1).gameObject.SetActive(false);
            StartCoroutine(OnTimeClickEvent( skill2, others, animator2, otherAnimators,() =>{}));

            //TODO: MoveToMap()
        });
        skill3.GetComponent<Button>().onClick.AddListener(() =>
        {
            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.Add(skill1);
            others.Add(skill2);
            otherAnimators.Add(animator1);
            otherAnimators.Add(animator2);
            
            mainCharacter.AddSkill(skills[2]);
            
            skill1.transform.GetChild(2).gameObject.SetActive(false);
            skill1.transform.GetChild(0).gameObject.SetActive(false);
            skill1.transform.GetChild(1).gameObject.SetActive(false);
            
            skill2.transform.GetChild(0).gameObject.SetActive(false);
            skill2.transform.GetChild(1).gameObject.SetActive(false);
            skill2.transform.GetChild(2).gameObject.SetActive(false);
            
            StartCoroutine(OnTimeClickEvent( skill3, others, animator3, otherAnimators,() =>{} ));
            
            //TODO: MoveToMap()

        });
        
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

        List<Items> items = new List<Items>();
        for (int i = 0; i < 3; i++)
        {
            items.Add(Items.GetRandomItem(items));
        }
        
        //expanding the item tabs
        var animator1 = item1.GetComponent<Animator>();
        animator1.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator1.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator1.IsInTransition(0));
        StartCoroutine(InsertTextIntoObject(item1.transform.GetChild(0).GetComponent<TMP_Text>(), items[0].ItemName));
        StartCoroutine(InsertTextIntoObject(item1.transform.GetChild(1).GetComponent<TMP_Text>(), items[0].ItemDescription));
        
        var animator2 = item2.GetComponent<Animator>();
        animator2.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator2.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator2.IsInTransition(0));
        StartCoroutine(InsertTextIntoObject(item2.transform.GetChild(0).GetComponent<TMP_Text>(), items[1].ItemName));
        StartCoroutine(InsertTextIntoObject(item2.transform.GetChild(1).GetComponent<TMP_Text>(), items[1].ItemDescription));
        
        var animator3 = item3.GetComponent<Animator>();
        animator3.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator3.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator3.IsInTransition(0));
        StartCoroutine(InsertTextIntoObject(item3.transform.GetChild(0).GetComponent<TMP_Text>(), items[2].ItemName));
        StartCoroutine(InsertTextIntoObject(item3.transform.GetChild(1).GetComponent<TMP_Text>(), items[2].ItemDescription));
        
        item1.GetComponent<Button>().onClick.AddListener(() =>
        {
            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.Add(item2);
            others.Add(item3);
            otherAnimators.Add(animator2);
            otherAnimators.Add(animator3);
            var item = mainCharacter.items.Find((e) => e.GetType() == items[0].GetType());
            if (item == null)
            {
                item = items[0];
                mainCharacter.items.Add(items[0]);
            }
            var unit = new List<Unit>(BattleSystem.system.playerUnits);
            item.Acquire(unit);
            StartCoroutine(OnTimeClickEvent( item1, others, animator1, otherAnimators,() =>{}));
            
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
            var item = mainCharacter.items.Find((e) => e.GetType() == items[1].GetType());
            if (item == null)
            {
                item = items[1];
                mainCharacter.items.Add(items[1]);
            }
            var unit = new List<Unit>(BattleSystem.system.playerUnits);
            item.Acquire(unit);
            StartCoroutine(OnTimeClickEvent( item2, others, animator2, otherAnimators,() =>{}));

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
            var item = mainCharacter.items.Find((e) => e.GetType() == items[2].GetType());
            if (item == null)
            {
                item = items[2];
                mainCharacter.items.Add(items[2]);
            }
            var unit = new List<Unit>(BattleSystem.system.playerUnits);
            item.Acquire(unit);
            StartCoroutine(OnTimeClickEvent(item3, others, animator3, otherAnimators,() =>{} ));
            
            //TODO: MoveToMap()

        });

        //await animations type shit.

        //make new 3 buttons for each item
    }
    
    //TODO: make a MoveToMap() function

    private IEnumerator OnTimeClickEvent(GameObject target, List<GameObject> others, Animator animator,
        List<Animator> otherAnimators, Action method = null)
    {
        
        
        for (int i = 0; i < others.Count; i++)
        {
            var texts = others[i].GetComponentsInChildren<TMP_Text>(true);
            for (int j = 0; j < texts.Length -1 ; j++)
            {
                StartCoroutine(ClearTextInObject(texts[j]));
            }
            StartCoroutine(ClearTextInObject(texts[^1]));
            if(i != others.Count - 1)
                StartCoroutine(ClearTextInObject(texts[^1]));
            else
                yield return ClearTextInObject(texts[^1]);
        }
        foreach (var ani in otherAnimators)
        {
            ani.SetTrigger(Collapse);
        }
        
        yield return target.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InExpo).WaitForCompletion();
        yield return new WaitForSeconds(1f);
        var lastText = target.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < lastText.Length -1; i++)
        {
            StartCoroutine(ClearTextInObject(lastText[i]));
        }
        yield return ClearTextInObject(lastText[^1]);
        animator.SetTrigger(Collapse);
        yield return new WaitForSeconds(1f);
        method?.Invoke();
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
        for (int i = 0; i < createdObjects.Count; i++)
        {
            createdObjects[i].GetComponent<Animator>().SetTrigger(Exit);
        }

        yield return null;
        var length = createdObjects[0].GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(length + 0.2f);

        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            Destroy(createdObjects[i]);
        }
        createdObjects.Clear();
    }
}
