using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MerchantSystem : MonoBehaviour
{
    public static MerchantSystem Manager;
    private static readonly int Enter = Animator.StringToHash("Enter");
    private static readonly int Entered = Animator.StringToHash("Entered");

    [SerializeField] private List<Button> itemSlots, skillSlots;
    [SerializeField] private Button reroll;

    [SerializeField] private TMP_Text title, description, moneyText;

    [SerializeField] private GameObject skillExchangeTab, skillName;
    [SerializeField] private List<Button> skillButtons;

    public int rerolls, level;

    private void Awake()
    {
        if (Manager) Destroy(this);
        else Manager = this;
    }

    private void OnDisable()
    {
        rerolls = 0;
    }

    public void Reroll()
    {
        if (rerolls != 0)
            Map.Manager.money -= rerolls * 100 * level / 3;
        rerolls++;
        moneyText.text = $"Money: {Map.Manager.money}";
        //Add cost text to the slots?
        
        var currentBatch = new List<Items>();
        foreach (var item in itemSlots)
        {
            var gift = Items.GetRandomItem(currentBatch);
            currentBatch.Add(gift);
            item.GetComponent<GameButton>().OnSelectEvent = () =>
            {
                title.text = gift.ItemName;
                description.text = gift.ItemDescription;
            };
            item.transform.GetChild(1).GetComponent<TMP_Text>().text = $"{level * 10}";
            if(gift.ItemImage)
                item.image.sprite = gift.ItemImage;
            item.onClick.RemoveAllListeners();
            item.onClick.AddListener(() =>
            {
                gift.Acquire(new List<Unit>(Map.Manager.currentPlayerUnits));
                Map.Manager.money -= 10 * level;
                moneyText.text = $"Money: {Map.Manager.money}";
                CheckAllButtons();
            });
        }

        var nextBatch = new List<Skill>(Map.Manager.currentPlayerUnits[0].skills);
        foreach (var skill in skillSlots)
        {
            var gift = Skill.GetRandomSkill(nextBatch);
            nextBatch.Add(gift);
            skill.GetComponent<GameButton>().OnSelectEvent = () =>
            {
                title.text = gift.skillName;
                description.text = gift.skillDescription;
            };
            skill.transform.GetChild(1).GetComponent<TMP_Text>().text = $"{10 * level}";
            if(gift.skillImage)
                skill.image.sprite = gift.skillImage;
            
            skill.onClick.RemoveAllListeners();
            skill.onClick.AddListener(() =>
            {
                if (Map.Manager.currentPlayerUnits[0].skills.Count == 6)
                {
                    StartCoroutine(SkillButtonPress(gift));
                    Map.Manager.money -= 10 * level;
                    moneyText.text = $"Money: {Map.Manager.money}";
                    CheckAllButtons();
                }
                else
                {
                    Map.Manager.currentPlayerUnits[0].AddSkill(gift);
                    Map.Manager.money -= 10 * level;
                    moneyText.text = $"Money: {Map.Manager.money}";
                    CheckAllButtons();
                }
            });
        }

        SetCurrentSelectButton(itemSlots[0]);
        reroll.transform.Find("reroll cost").GetComponent<TMP_Text>().text = $"{ rerolls * 100 + 100 * level / 3}";
        CheckAllButtons();
    }

    private IEnumerator SkillButtonPress(Skill skill)
    {
        skillExchangeTab.SetActive(true);
        skillName.SetActive(true);
        skillName.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text =  skill.skillDescription;
        skillName.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text =  skill.skillName;
        skillName.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text =  skill.skillCost.ToString();
        
        for (int i = 0; i < skillButtons.Count; i++)
        {
            skillButtons[i].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Map.Manager.currentPlayerUnits[0].GetSkill(i).skillCost.ToString();
            skillButtons[i].transform.GetChild(1).GetComponent<TMP_Text>().text = Map.Manager.currentPlayerUnits[0].GetSkill(i).skillName;
            skillButtons[i].transform.GetChild(2).GetComponent<TMP_Text>().text = Map.Manager.currentPlayerUnits[0].GetSkill(i).skillDescription;
            skillButtons[i].gameObject.SetActive(true);
            skillButtons[i].GetComponent<Animator>().SetTrigger(Enter);
            yield return new WaitForSeconds(0.25f);
            skillButtons[i].GetComponent<Animator>().SetBool("Entered", true);
            //Set onClick as well
            skillButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            int i1 = i;
            skillButtons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                Map.Manager.currentPlayerUnits[0].AddSkill(skill, i1);
                skillButtons[i1].transform.DOLocalRotate(new Vector3(1800, 0, 0), 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    skillButtons[i1].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = skill.skillCost.ToString();
                    skillButtons[i1].transform.GetChild(1).GetComponent<TMP_Text>().text = skill.skillName;
                    skillButtons[i1].transform.GetChild(2).GetComponent<TMP_Text>().text = skill.skillDescription;
                    StartCoroutine(ClearAllSelectSkillButtons());
                });
            });
            skillButtons[i].GetComponent<GameButton>().OnSpecificAction = () =>
            {
                var active = skillButtons[i1].transform.Find("Description").gameObject.activeSelf;
                skillButtons[i1].transform.DOLocalRotate(new Vector3(1800, 0, 0), 0.025f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    skillButtons[i1].transform.Find("Description").gameObject.SetActive(!active);
                    skillButtons[i1].transform.Find("Title").gameObject.SetActive(active);
                });
            };
        }
    }
    
    private IEnumerator ClearAllSelectSkillButtons()
    {
        yield return new WaitForSeconds(1f);
        foreach (var t in skillButtons)
        {
            t.GetComponent<Animator>().SetBool(Entered, false);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        //TODO: exit something something
        Map.Manager.ReturnToMap();
        gameObject.SetActive(false);
    }


    private void CheckAllButtons()
    {
        foreach (var item in itemSlots)
        {
            item.interactable = Map.Manager.money >= level * 10;
        }

        foreach (var skill in skillSlots)
        {
            skill.interactable = Map.Manager.money >= level * 10;
        }
        
        reroll.interactable = Map.Manager.money >= 100 * level / 3;
    }

    #region Input

    private Button currentButton;

    public void Submit()
    {
        if (!currentButton) return;
        if(itemSlots.Contains(currentButton) || skillSlots.Contains(currentButton))
            currentButton.interactable = false;
        currentButton.onClick.Invoke();
    }

    public void Cancel()
    {
        Map.Manager.ReturnToMap();
    }

    public void Navigate(Vector2 normalizedInput)
    {
        if (!currentButton) return;
        if (normalizedInput == Vector2.zero) return;
        if (!currentButton || normalizedInput == Vector2.zero) return;
        var isVertical = Mathf.Abs(normalizedInput.y) > Mathf.Abs(normalizedInput.x);
        Selectable selectable;
        if (isVertical)
        {
            selectable = normalizedInput.y > 0
                ? currentButton.navigation.selectOnUp
                : currentButton.navigation.selectOnDown;
        }
        else
        {
            selectable = normalizedInput.x > 0
                ? currentButton.navigation.selectOnRight
                : currentButton.navigation.selectOnLeft;
        }

        if (!selectable) return;
        SetCurrentSelectButton((Button)selectable);
    }
    
    private void SetCurrentSelectButton(Button button)
    {
        if (currentButton && currentButton.TryGetComponent(typeof(GameButton), out var component))
        {
            ( component as GameButton)?.OnDeselectEvent?.Invoke();
        }
        currentButton = button;
        currentButton?.Select();
        if (!currentButton || !currentButton.TryGetComponent(typeof(GameButton), out component)) return;
        {
            (component as GameButton)?.OnSelectEvent?.Invoke();
        }
    }

    #endregion
}
