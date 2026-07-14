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
    private static readonly int FinishedEntering = Animator.StringToHash("FinishedEntering");
    private static readonly int Entered = Animator.StringToHash("Entered");
    [SerializeField] public PlayerUnit mainCharacter;
    [SerializeField] private List<GameObject> rootButtons,statButtons,skillButtons,skillSelectButtons,itemButtons,charGrowth;

    private RectTransform rectTransform;
    private Button currentSelectButton;

    //TODO: do a proper cleanup of the scene with all game objects that got instantiated deleted and the progress of the characters saved.
    //make sure to not save it to the prefab tho
    
    private float screenHeight => transform.parent.GetComponent<CanvasScaler>().referenceResolution.y;
    private float screenWidth => transform.parent.GetComponent<CanvasScaler>().referenceResolution.x;

    private IEnumerator ResetScreen()
    {
        //TODO: make sure to reset all skills, stat icons and so on
        yield return ClearAllButtons(false);
        
        //TODO: make also for currency count

        
        for (var i = 0; i < Map.Manager.currentPlayerUnits.Count; i++)
        {
            var curPlayer = Map.Manager.currentPlayerUnits[i];
            var tempStats = new int[]
            {
                curPlayer.Strength,
                curPlayer.Constitution,
                curPlayer.Speed,
                curPlayer.Intelligence,
                curPlayer.Luck
            };
            curPlayer.AddStatLevel(1);
            charGrowth[i].GetComponent<Animator>().Play($"Panel Open");
            var growth = charGrowth[i].GetComponent<CharacterGrowth>();
            growth.statTexts.Clear();
            growth.statTexts.Add($"Strength: {tempStats[0]} => {curPlayer.Strength}");
            growth.statTexts.Add($"Constitution: {tempStats[1]} => {curPlayer.Constitution}");
            growth.statTexts.Add($"Speed: {tempStats[2]} => {curPlayer.Speed}");
            growth.statTexts.Add($"Intelligence: {tempStats[3]} => {curPlayer.Intelligence}");
            growth.statTexts.Add($"Luck: {tempStats[4]} => {curPlayer.Luck}");
            growth.profile.sprite = curPlayer.hudImage;
            charGrowth[i].SetActive(true);
        }
        
        var anim = charGrowth[Map.Manager.currentPlayerUnits.Count-1].GetComponent<Animator>();
        yield return null;
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0));
 
        
        
        rootButtons[0].transform.localPosition = new Vector3(screenWidth * 1.5f, screenHeight * 0.25f, 0);
        rootButtons[0].SetActive(true);
        rootButtons[0].GetComponent<Animator>().SetTrigger(Enter);
        yield return null;
        
        rootButtons[1].transform.localPosition = new Vector3(screenWidth * 1.5f, 0, 0);
        rootButtons[1].SetActive(true);
        rootButtons[1].GetComponent<Animator>().SetTrigger(Enter);
        
        yield return null;
        
        rootButtons[2].transform.localPosition = new Vector3(screenWidth * 1.5f, -screenHeight * 0.25f, 0);
        rootButtons[2].SetActive(true); 
        rootButtons[2].GetComponent<Animator>().SetTrigger(Enter);
        var itemsAnimator = rootButtons[2].GetComponent<Animator>();
        yield return null;
        yield return new WaitUntil(() => itemsAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !itemsAnimator.IsInTransition(0));
        
        rootButtons[0].GetComponent<Animator>().SetBool(FinishedEntering, true);
        rootButtons[1].GetComponent<Animator>().SetBool(FinishedEntering, true);
        rootButtons[2].GetComponent<Animator>().SetBool(FinishedEntering, true);
        
        currentSelectButton = rootButtons[0].GetComponent<Button>();
        currentSelectButton.Select();
    }
    
    //called when the game is won
    private void OnEnable()
    {
        //TODO: Do a reset
        
        StartCoroutine(ResetScreen());
        
        //TODO: Some form of level up showcase?
        
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }


    public void OnStatsStart()
    {
        rootButtons[0].GetComponent<Animator>().SetBool(FinishedEntering, false);
        rootButtons[1].GetComponent<Animator>().SetBool(FinishedEntering, false);
        rootButtons[2].GetComponent<Animator>().SetBool(FinishedEntering, false);
        StartCoroutine(OnStats());
    }

    public void OnSkillsStart()
    {
        rootButtons[0].GetComponent<Animator>().SetBool(FinishedEntering, false);
        rootButtons[1].GetComponent<Animator>().SetBool(FinishedEntering, false);
        rootButtons[2].GetComponent<Animator>().SetBool(FinishedEntering, false);
        StartCoroutine(OnSkills());
    }

    public void OnItemsStart()
    {
        rootButtons[0].GetComponent<Animator>().SetBool(FinishedEntering, false);
        rootButtons[1].GetComponent<Animator>().SetBool(FinishedEntering, false);
        rootButtons[2].GetComponent<Animator>().SetBool(FinishedEntering, false);
        StartCoroutine(OnItems());
    }
    
    
    private IEnumerator OnStats()
    {
        yield return ClearAllButtons();
        
        
        var stats = statButtons[0];
        stats.transform.localPosition = new Vector3(0, 0);
        stats.SetActive(true);

        var anim = stats.GetComponent<Animator>();
        anim.SetTrigger(Expand);
        yield return null;
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0));

        var statlist = mainCharacter.GetStats();
        int allocatable;
        if (Map.system.currentNode.boost != 1)
            allocatable = 10 + 5 * (Map.system.currentNode.boost - 1);
        else
            allocatable = 10;
        
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
                Map.Manager.ReturnToMap();
                gameObject.SetActive(false);
            }
           
        });
    }
    
    private readonly List<Animator> skillButtonAnims = new();
    private IEnumerator OnSkills()
    {
        yield return ClearAllButtons();
        
        
        skillButtons[0].transform.localPosition = new Vector3(-rectTransform.rect.width * 0.3f, 0);
        skillButtons[0].SetActive(true);
        
        skillButtons[1].transform.localPosition = new Vector3(0, 0);
        skillButtons[1].SetActive(true);
        
        skillButtons[2].transform.localPosition = new Vector3(rectTransform.rect.width * 0.3f, 0);
        skillButtons[2].SetActive(true);
        
        List<Skill> skills = new List<Skill>();
        
        for (int i = 0; i < 3; i++)
        {
            skills.Add(Skill.GetRandomSkill(skills));
            skillButtonAnims.Add(skillButtons[i].GetComponent<Animator>());
            if(Map.system.currentNode.boost != 1) skills[i].boost = Map.system.currentNode.boost;
        }

        for (int i = 0; i < 3; i++)
        {
            var animator = skillButtons[i].GetComponent<Animator>();
            animator.SetTrigger(Expand);
            yield return null; // otherwise too quick
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
            skillButtons[i].transform.GetChild(0).gameObject.SetActive(true);
            skillButtons[i].transform.GetChild(1).gameObject.SetActive(true);
            skillButtons[i].transform.GetChild(2).gameObject.SetActive(true);
            StartCoroutine(InsertTextIntoObject(skillButtons[i].transform.GetChild(0).GetComponentInChildren<TMP_Text>(), skills[i].skillDescription));
            StartCoroutine(InsertTextIntoObject(skillButtons[i].transform.GetChild(1).GetComponentInChildren<TMP_Text>(), skills[i].skillName));
            StartCoroutine(InsertTextIntoObject(skillButtons[i].transform.GetChild(2).GetComponentInChildren<TMP_Text>(),
                skills[i].skillCost.ToString()));
            
            List<GameObject> others = skillButtons.FindAll((e) => e != skillButtons[i]);
            List<Animator> otherAnimators = skillButtonAnims.FindAll((e) => e.gameObject != skillButtons[i]);
            
            skillButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            var i1 = i;
            skillButtons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                
                //TODO: add case that mainCharacter has too many skills and wants to exchange.

                if (mainCharacter.SkillCount > 5)
                    StartCoroutine(OnSkillsReplace(skills[i1], skillButtons[i1], others, animator, otherAnimators));
                else
                {
                    mainCharacter.AddSkill(skills[i1]);
                    StartCoroutine(OnTimeClickEvent( skillButtons[i1], others, animator, otherAnimators, () =>
                    {
                        Map.Manager.ReturnToMap();
                        gameObject.SetActive(false);
                    }));
                }
            });
        }
        
        currentSelectButton = skillButtons[0].GetComponent<Button>();
        currentSelectButton.Select();
    }

    private IEnumerator OnSkillsReplace(Skill skill,GameObject target, List<GameObject> others, Animator animator,
        List<Animator> otherAnimators)
    {
        yield return null;
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
        yield return new WaitForSeconds(0.5f);
        yield return target.transform.DOLocalMove(Vector3.right * (screenWidth * 0.25f),  0.5f).SetEase(Ease.InExpo).WaitForCompletion();
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < skillSelectButtons.Count; i++)
        {
            skillSelectButtons[i].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = mainCharacter.GetSkill(i).skillCost.ToString();
            skillSelectButtons[i].transform.GetChild(1).GetComponent<TMP_Text>().text = mainCharacter.GetSkill(i).skillName;
            skillSelectButtons[i].transform.GetChild(2).GetComponent<TMP_Text>().text = mainCharacter.GetSkill(i).skillDescription;
            skillSelectButtons[i].SetActive(true);
            skillSelectButtons[i].GetComponent<Animator>().SetTrigger(Enter);
            yield return new WaitForSeconds(0.25f);
            skillSelectButtons[i].GetComponent<Animator>().SetBool("Entered", true);
            //Set onClick as well
            skillSelectButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            int i1 = i;
            skillSelectButtons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                mainCharacter.AddSkill(skill, i1);
                skillSelectButtons[i1].transform.DOLocalRotate(new Vector3(1800, 0, 0), 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    skillSelectButtons[i1].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = skill.skillCost.ToString();
                    skillSelectButtons[i1].transform.GetChild(1).GetComponent<TMP_Text>().text = skill.skillName;
                    skillSelectButtons[i1].transform.GetChild(2).GetComponent<TMP_Text>().text = skill.skillDescription;
                    StartCoroutine(ClearAllSelectSkillButtons());
                });
            });
            skillSelectButtons[i].GetComponent<GameButton>().OnSpecificAction = () =>
            {
                var active = skillSelectButtons[i1].transform.Find("Description").gameObject.activeSelf;
                skillSelectButtons[i1].transform.DOLocalRotate(new Vector3(1800, 0, 0), 0.025f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    skillSelectButtons[i1].transform.Find("Description").gameObject.SetActive(!active);
                    skillSelectButtons[i1].transform.Find("Title").gameObject.SetActive(active);
                });
            };
        }

        currentSelectButton = skillSelectButtons[0].GetComponent<Button>();
        currentSelectButton.Select();
    }

    private IEnumerator ClearAllSelectSkillButtons()
    {
        yield return new WaitForSeconds(1f);
        foreach (var t in skillSelectButtons)
        {
            t.GetComponent<Animator>().SetBool(Entered, false);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        //TODO: exit something something
        Map.Manager.ReturnToMap();
        gameObject.SetActive(false);
    }

    private IEnumerator OnItems()
    {
        yield return ClearAllButtons();


        itemButtons[0].transform.localPosition = new Vector3(-rectTransform.rect.width * 0.3f, 0);
        itemButtons[0].SetActive(true);

        itemButtons[1].transform.localPosition = new Vector3(0, 0);
        itemButtons[1].SetActive(true);

        itemButtons[2].transform.localPosition = new Vector3(rectTransform.rect.width * 0.3f, 0);
        itemButtons[2].SetActive(true);

        List<Items> items = new List<Items>();
        List<Animator> anims = new List<Animator>();
        for (int i = 0; i < 3; i++)
        {
            items.Add(Items.GetRandomItem(items));
            anims.Add(itemButtons[i].GetComponent<Animator>());
        }

        //TODO: change if item object changes. maybe into standalone a script?
        for (int i = 0; i < 3; i++)
        {
            var animator = itemButtons[i].GetComponent<Animator>();
            animator.SetTrigger(Expand);
            yield return null; // otherwise too quick
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
            StartCoroutine(InsertTextIntoObject(itemButtons[i].transform.GetChild(0).GetComponent<TMP_Text>(),
                items[i].ItemName));
            StartCoroutine(InsertTextIntoObject(itemButtons[i].transform.GetChild(1).GetComponent<TMP_Text>(),
                items[i].ItemDescription));

            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.AddRange(itemButtons);
            otherAnimators.AddRange(anims);
            others.Remove(itemButtons[i]);
            otherAnimators.Remove(animator);

            itemButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            var i1 = i;
            itemButtons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                var item = mainCharacter.Items.Find((e) => e.GetType() == items[i1].GetType());
                if (item == null)
                {
                    item = items[i1];
                    mainCharacter.Items.Add(items[i1]);
                }

                var unit = new List<Unit>(Map.Manager.currentPlayerUnits);
                item.Acquire(unit);
                StartCoroutine(OnTimeClickEvent(itemButtons[i1], others, animator, otherAnimators, () =>
                {
                    Map.Manager.ReturnToMap();
                    gameObject.SetActive(false);
                }));

                
            });

        }


        currentSelectButton = itemButtons[0].GetComponent<Button>();
        currentSelectButton.Select();
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
        yield return new WaitForSeconds(0.5f);
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

    private static IEnumerator InsertTextIntoObject(TMP_Text textObj, string text)
    {
        textObj.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            textObj.text += text[i];
            yield return null;
        }
    }

    private static IEnumerator ClearTextInObject(TMP_Text textObj)
    {
        var amount = textObj.text.Length * Time.deltaTime;
        while(textObj.text.Length > 0)
        {
            textObj.text = textObj.text[..Mathf.Max(0,textObj.text.Length - (int)Mathf.Ceil(amount))];
            yield return null;
        }
    }

    
    //TODO: Why are you doing this with code when you can do this with animations.
    private IEnumerator ClearAllButtons(bool withAnimation = true)
    {
        foreach (var g in charGrowth)
        {
            g.SetActive(false);
        }
        if (withAnimation)
        {
            foreach (var t in rootButtons)
            {
                t.GetComponent<Animator>().SetTrigger(Exit);
                yield return null;
                var length = t.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
                yield return new WaitForSeconds(length/4f);
                t.SetActive(false);
            }
        }
        
        statButtons[0].SetActive(false);
        skillButtons[0].SetActive(false);
        skillButtons[1].SetActive(false);
        skillButtons[2].SetActive(false);
        itemButtons[0].SetActive(false);
        itemButtons[1].SetActive(false);
        itemButtons[2].SetActive(false);
        skillSelectButtons[0].SetActive(false);
        skillSelectButtons[1].SetActive(false);
        skillSelectButtons[2].SetActive(false);
        skillSelectButtons[3].SetActive(false);
        skillSelectButtons[4].SetActive(false);
        skillSelectButtons[5].SetActive(false);
    }

    public void Navigate(Vector2 normalizedInput)
    {
        if (!currentSelectButton) return;
        if (normalizedInput == Vector2.zero) return;
        var isVertical = Mathf.Abs(normalizedInput.y) > Mathf.Abs(normalizedInput.x);
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

        if (selectable == null) return;
        currentSelectButton = (Button)selectable;
        currentSelectButton?.Select();
        if (currentSelectButton != null)
        {
            //think about what to put here if needed be
        }
    }

    public void Confirm()
    {
        currentSelectButton?.onClick.Invoke();
    }
}
