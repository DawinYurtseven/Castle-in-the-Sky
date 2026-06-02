using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEditor;
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
        //TODO: make sure to reset all skills, stat icons and so on
        yield return ClearAllButtons(false);
         
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
                gameObject.SetActive(false);
            }
           
        });
    }
    
    private IEnumerator OnSkills()
    {
        yield return ClearAllButtons();
        
        
        SkillButtons[0].transform.localPosition = new Vector3(-rectTransform.rect.width * 0.3f, 0);
        SkillButtons[0].SetActive(true);
        
        SkillButtons[1].transform.localPosition = new Vector3(0, 0);
        SkillButtons[1].SetActive(true);
        
        SkillButtons[2].transform.localPosition = new Vector3(rectTransform.rect.width * 0.3f, 0);
        SkillButtons[2].SetActive(true);
        
        List<Skill> skills = new List<Skill>();
        List<Animator> anims = new List<Animator>();
        for (int i = 0; i < 3; i++)
        {
            skills.Add(Skill.GetRandomSkill(skills));
            anims.Add(SkillButtons[i].GetComponent<Animator>());
        }

        for (int i = 0; i < 3; i++)
        {
            var animator = SkillButtons[i].GetComponent<Animator>();
            animator.SetTrigger(Expand);
            yield return null; // otherwise too quick
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
            SkillButtons[i].transform.GetChild(0).gameObject.SetActive(true);
            SkillButtons[i].transform.GetChild(1).gameObject.SetActive(true);
            SkillButtons[i].transform.GetChild(2).gameObject.SetActive(true);
            StartCoroutine(InsertTextIntoObject(SkillButtons[i].transform.GetChild(0).GetComponentInChildren<TMP_Text>(), skills[i].skillDescription));
            StartCoroutine(InsertTextIntoObject(SkillButtons[i].transform.GetChild(1).GetComponentInChildren<TMP_Text>(), skills[i].skillName));
            StartCoroutine(InsertTextIntoObject(SkillButtons[i].transform.GetChild(2).GetComponentInChildren<TMP_Text>(),
                skills[i].skillCost.ToString()));
            
            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.AddRange(SkillButtons);
            otherAnimators.AddRange(anims);
            others.Remove(SkillButtons[i]);
            otherAnimators.Remove(animator);
            
            SkillButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            var i1 = i;
            SkillButtons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                mainCharacter.AddSkill(skills[i1]);
                
                /*others[0].transform.GetChild(0).gameObject.SetActive(false);
                others[0].transform.GetChild(1).gameObject.SetActive(false); 
                others[0].transform.GetChild(2).gameObject.SetActive(false);
            
                others[1].transform.GetChild(0).gameObject.SetActive(false);
                others[1].transform.GetChild(1).gameObject.SetActive(false);
                others[1].transform.GetChild(2).gameObject.SetActive(false);*/

                StartCoroutine(OnTimeClickEvent( SkillButtons[i1], others, animator, otherAnimators, () =>
                {
                    Map.System.ReturnToMap();
                    gameObject.SetActive(false);
                }));
            });
        }
        
        currentSelectButton = SkillButtons[0].GetComponent<Button>();
        currentSelectButton.Select();
    }

    private IEnumerator OnItems()
    {
        yield return ClearAllButtons();


        ItemButtons[0].transform.localPosition = new Vector3(-rectTransform.rect.width * 0.3f, 0);
        ItemButtons[0].SetActive(true);

        ItemButtons[1].transform.localPosition = new Vector3(0, 0);
        ItemButtons[1].SetActive(true);

        ItemButtons[2].transform.localPosition = new Vector3(rectTransform.rect.width * 0.3f, 0);
        ItemButtons[2].SetActive(true);

        List<Items> items = new List<Items>();
        List<Animator> anims = new List<Animator>();
        for (int i = 0; i < 3; i++)
        {
            items.Add(Items.GetRandomItem(items));
            anims.Add(ItemButtons[i].GetComponent<Animator>());
        }

        for (int i = 0; i < 3; i++)
        {
            var animator = ItemButtons[i].GetComponent<Animator>();
            animator.SetTrigger(Expand);
            yield return null; // otherwise too quick
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
            StartCoroutine(InsertTextIntoObject(ItemButtons[i].transform.GetChild(0).GetComponent<TMP_Text>(),
                items[i].ItemName));
            StartCoroutine(InsertTextIntoObject(ItemButtons[i].transform.GetChild(1).GetComponent<TMP_Text>(),
                items[i].ItemDescription));

            List<GameObject> others = new List<GameObject>();
            List<Animator> otherAnimators = new List<Animator>();
            others.AddRange(ItemButtons);
            otherAnimators.AddRange(anims);
            others.Remove(ItemButtons[i]);
            otherAnimators.Remove(animator);

            ItemButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            var i1 = i;
            ItemButtons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                var item = mainCharacter.items.Find((e) => e.GetType() == items[i1].GetType());
                if (item == null)
                {
                    item = items[i1];
                    mainCharacter.items.Add(items[i1]);
                }

                var unit = new List<Unit>(BattleSystem.system.playerUnits);
                item.Acquire(unit);
                StartCoroutine(OnTimeClickEvent(ItemButtons[i1], others, animator, otherAnimators, () =>
                {
                    Map.System.ReturnToMap();
                    gameObject.SetActive(false);
                }));

                
            });

        }


        currentSelectButton = ItemButtons[0].GetComponent<Button>();
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

    private IEnumerator InsertTextIntoObject(TMP_Text textObj, string text)
    {
        textObj.text = "";
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
                var length = rootButtons[i].GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
                yield return new WaitForSeconds(length/2f);
                rootButtons[i].SetActive(false);
            }
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
