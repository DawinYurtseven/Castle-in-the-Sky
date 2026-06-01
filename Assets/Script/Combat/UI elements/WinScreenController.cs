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
    private static readonly int FinishedExpanding = Animator.StringToHash("FinishedExpanding");
    private static readonly int FinishedEntering = Animator.StringToHash("FinishedEntering");
    [SerializeField] public PlayerUnit mainCharacter;
    [SerializeField] private List<GameObject> rootButtons,StatButtons, SkillButtons, ItemButtons;
    
    private RectTransform rectTransform;
    private Button currentSelectButton;

    //TODO: do a proper cleanup of the scene with all game objects that got instantiated deleted and the progress of the characters saved.
    //make sure to not save it to the prefab tho
    
    private float screenHeight => transform.parent.GetComponent<CanvasScaler>().referenceResolution.y;
    private float screenWidth => transform.parent.GetComponent<CanvasScaler>().referenceResolution.x;

    private IEnumerator ResetScreen()
    {
        yield return ClearAllButtons(false);
        
        rectTransform = GetComponent<RectTransform>();
        

        var stats = rootButtons[0];
        stats.transform.localPosition = new Vector3(screenWidth * 1.5f, screenHeight * 0.25f, 0);
        stats.SetActive(true);
        var statAnimator = stats.GetComponent<Animator>();
        statAnimator.SetTrigger(Enter);
        stats.GetComponent<Button>().onClick.AddListener(() =>
        {
            statAnimator.SetBool(FinishedEntering, false);
            StartCoroutine(OnStats());
        });
        yield return null;
        
        var skills = rootButtons[1];
        skills.transform.localPosition = new Vector3(screenWidth * 1.5f, 0, 0);
        skills.SetActive(true);
        var skillAnimator = skills.GetComponent<Animator>();
        skillAnimator.SetTrigger(Enter);
        skills.GetComponent<Button>().onClick.AddListener(() =>
        {
            skillAnimator.SetBool(FinishedEntering, false);
            StartCoroutine(OnSkills());
        });
        yield return null;
        
        var items = rootButtons[2];
        items.transform.localPosition = new Vector3(screenWidth * 1.5f, -screenHeight * 0.25f, 0);
        items.SetActive(true);
        var itemsAnimator = items.GetComponent<Animator>();
        itemsAnimator.SetTrigger(Enter);
        items.GetComponent<Button>().onClick.AddListener(() =>
        {
            itemsAnimator.SetBool(FinishedEntering, false);
            StartCoroutine(OnItems());
        });

        yield return null;
        yield return new WaitUntil(() => itemsAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !statAnimator.IsInTransition(0));

        
        statAnimator.SetBool(FinishedEntering, true);
        skillAnimator.SetBool(FinishedEntering, true);
        itemsAnimator.SetBool(FinishedEntering, true);
        
        currentSelectButton = stats.GetComponent<Button>();
        currentSelectButton.Select();
    }
    
    //called when the game is won
    private void OnEnable()
    {
        //TODO: Do a reset
        
        StartCoroutine(ResetScreen());
        
        //TODO: Some form of level up showcase?
        
    }


    private IEnumerator OnStats()
    {
        yield return ClearAllButtons();
        var stats = StatButtons[0];
        stats.transform.localPosition = new Vector3(0, 0);
        stats.SetActive(true);

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
            minus.GetComponent<Button>().onClick.RemoveAllListeners();
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
            plus.GetComponent<Button>().onClick.RemoveAllListeners();
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
        stats.transform.GetChild(0).GetChild(1).GetComponent<Button>().Select();
        currentSelectButton = stats.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        stats.transform.GetChild(6).gameObject.SetActive(true);
        stats.transform.GetChild(6).GetComponent<Button>().onClick.RemoveAllListeners();
        stats.transform.GetChild(6).GetComponent<Button>().onClick.AddListener(() =>
        {
            if (allocatable == 0) 
            {
                Map.System.ReturnToMap();
                //TODO: end the screen and go to menu
                gameObject.SetActive(false);
            }
           
        });
    }
    
    private IEnumerator OnSkills()
    {
        yield return ClearAllButtons();
        
        var skill1 = SkillButtons[0];
        skill1.transform.localPosition = new Vector3(-rectTransform.rect.width * 0.3f, 0);
        skill1.SetActive(true);
        var skill2 = SkillButtons[1];
        skill2.transform.localPosition = new Vector3(0, 0);
        skill2.SetActive(true);
        var skill3 = SkillButtons[2];
        skill3.transform.localPosition = new Vector3(rectTransform.rect.width * 0.3f, 0);
        skill3.SetActive(true);
        
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
        animator1.SetBool(FinishedExpanding, true);
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
        animator2.SetBool(FinishedExpanding, true);
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
        animator3.SetBool(FinishedExpanding, true);
        skill3.transform.GetChild(0).gameObject.SetActive(true);
        skill3.transform.GetChild(1).gameObject.SetActive(true);
        skill3.transform.GetChild(2).gameObject.SetActive(true);
        StartCoroutine(InsertTextIntoObject(skill3.transform.GetChild(1).GetComponentInChildren<TMP_Text>(), skills[2].skillName));
        StartCoroutine(InsertTextIntoObject(skill3.transform.GetChild(0).GetComponentInChildren<TMP_Text>(), skills[2].skillDescription));
        StartCoroutine(InsertTextIntoObject(skill3.transform.GetChild(2).GetComponentInChildren<TMP_Text>(), skills[2].skillCost.ToString()));

        skill1.GetComponent<Button>().onClick.RemoveAllListeners();
        skill1.GetComponent<Button>().onClick.AddListener(() =>
        {
            animator1.SetBool(FinishedExpanding, false);
            animator2.SetBool(FinishedExpanding, false);
            animator3.SetBool(FinishedExpanding, false);
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
            Map.System.ReturnToMap();
            gameObject.SetActive(false);
        });
        
        skill2.GetComponent<Button>().onClick.RemoveAllListeners();
        skill2.GetComponent<Button>().onClick.AddListener(() =>
        {
            animator1.SetBool(FinishedExpanding, false);
            animator2.SetBool(FinishedExpanding, false);
            animator3.SetBool(FinishedExpanding, false);
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
            Map.System.ReturnToMap();
            gameObject.SetActive(false);
        });
        
        skill3.GetComponent<Button>().onClick.RemoveAllListeners();
        skill3.GetComponent<Button>().onClick.AddListener(() =>
        {
            animator1.SetBool(FinishedExpanding, false);
            animator2.SetBool(FinishedExpanding, false);
            animator3.SetBool(FinishedExpanding, false);
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
            Map.System.ReturnToMap();
            gameObject.SetActive(false);
        });
        currentSelectButton = skill1.GetComponent<Button>();
        currentSelectButton.Select();
    }
    
    private IEnumerator OnItems()
    {
        yield return ClearAllButtons();
        
        var item1 = ItemButtons[0];
        item1.transform.localPosition = new Vector3(-rectTransform.rect.width * 0.3f, 0);
        item1.SetActive(true);
        var item2 = ItemButtons[1];
        item2.transform.localPosition = new Vector3(0, 0);
        item2.SetActive(true);
        var item3 = ItemButtons[2];
        item3.transform.localPosition = new Vector3(rectTransform.rect.width * 0.3f, 0);
        item3.SetActive(true);
        
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
        animator1.SetBool(FinishedExpanding, true);
        StartCoroutine(InsertTextIntoObject(item1.transform.GetChild(0).GetComponent<TMP_Text>(), items[0].ItemName));
        StartCoroutine(InsertTextIntoObject(item1.transform.GetChild(1).GetComponent<TMP_Text>(), items[0].ItemDescription));
        
        var animator2 = item2.GetComponent<Animator>();
        animator2.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator2.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator2.IsInTransition(0));
        animator2.SetBool(FinishedExpanding, true);
        StartCoroutine(InsertTextIntoObject(item2.transform.GetChild(0).GetComponent<TMP_Text>(), items[1].ItemName));
        StartCoroutine(InsertTextIntoObject(item2.transform.GetChild(1).GetComponent<TMP_Text>(), items[1].ItemDescription));
        
        var animator3 = item3.GetComponent<Animator>();
        animator3.SetTrigger(Expand);
        yield return null; // otherwise too quick
        yield return new WaitUntil(() => animator3.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator3.IsInTransition(0));
        animator3.SetBool(FinishedExpanding, true);
        StartCoroutine(InsertTextIntoObject(item3.transform.GetChild(0).GetComponent<TMP_Text>(), items[2].ItemName));
        StartCoroutine(InsertTextIntoObject(item3.transform.GetChild(1).GetComponent<TMP_Text>(), items[2].ItemDescription));
        
        item1.GetComponent<Button>().onClick.RemoveAllListeners();
        item1.GetComponent<Button>().onClick.AddListener(() =>
        {
            animator1.SetBool(FinishedExpanding, false);
            animator2.SetBool(FinishedExpanding, false);
            animator3.SetBool(FinishedExpanding, false);
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
            Map.System.ReturnToMap();
            gameObject.SetActive(false);
        });
        
        item2.GetComponent<Button>().onClick.RemoveAllListeners();
        item2.GetComponent<Button>().onClick.AddListener(() =>
        {
            animator1.SetBool(FinishedExpanding, false);
            animator2.SetBool(FinishedExpanding, false);
            animator3.SetBool(FinishedExpanding, false);
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
            Map.System.ReturnToMap();
            gameObject.SetActive(false);
        });
        
        item3.GetComponent<Button>().onClick.RemoveAllListeners();
        item3.GetComponent<Button>().onClick.AddListener(() =>
        {
            animator1.SetBool(FinishedExpanding, false);
            animator2.SetBool(FinishedExpanding, false);
            animator3.SetBool(FinishedExpanding, false);
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
            Map.System.ReturnToMap();
            gameObject.SetActive(false);
        });

        currentSelectButton = item1.GetComponent<Button>();
        currentSelectButton.Select();
        //await animations type shit.

        //make new 3 buttons for each item
    }

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
    private IEnumerator ClearAllButtons(bool withAnimation = true)
    {
        if (withAnimation)
        {
            for (int i = 0; i < rootButtons.Count; i++)
            {
                rootButtons[i].GetComponent<Animator>().SetTrigger(Exit);
            }
            var length = rootButtons[0].GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
            yield return new WaitForSeconds(length + 0.2f);
        }
        
        for (int i = 0; i < rootButtons.Count; i++)
        {
            rootButtons[i].transform.localPosition = new  Vector3(screenWidth * 1.5f, screenHeight * (0.25f - 0.25f * i), 0);
            rootButtons[i].SetActive(false);
        }
        
        StatButtons[0].SetActive(false);
        SkillButtons[0].SetActive(false);
        SkillButtons[1].SetActive(false);
        SkillButtons[2].SetActive(false);
        ItemButtons[0].SetActive(false);
        ItemButtons[1].SetActive(false);
        ItemButtons[2].SetActive(false);
    }

    public void Navigate(Vector2 normalizedInput)
    {
        if (currentSelectButton == null) return;
        if (normalizedInput != Vector2.zero)
        {
            bool isVertical = Mathf.Abs(normalizedInput.y) > Mathf.Abs(normalizedInput.x);
            Selectable selectable;
            if (isVertical)
            {
                selectable = normalizedInput.y > 0
                    ? currentSelectButton.navigation.selectOnUp
                    : currentSelectButton.navigation.selectOnDown;
            }
            else
            {
                selectable = normalizedInput.x > 0
                    ? currentSelectButton.navigation.selectOnRight
                    : currentSelectButton.navigation.selectOnLeft;
            }

            if (selectable != null)
            {
                currentSelectButton = (Button)selectable;
                currentSelectButton?.Select();
                if (currentSelectButton != null)
                {
                    //think about what to put here if needed be
                }
            }
        }
    }

    public void Confirm()
    {
        currentSelectButton?.onClick.Invoke();
    }
}
