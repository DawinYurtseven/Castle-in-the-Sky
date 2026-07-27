using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MerchantSystem : MonoBehaviour
{
    public static MerchantSystem Manager;

    [SerializeField] private List<Button> itemSlots, skillSots;
    [SerializeField] private Button reroll;

    [SerializeField] private TMP_Text title, description;

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
            });
        }

        var nextBatch = new List<Skill>();
        foreach (var skill in skillSots)
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
            
            //replace skill window needed for this one
        }

        SetCurrentSelectButton(itemSlots[0]);
        reroll.transform.Find("reroll cost").GetComponent<TMP_Text>().text = $"{ rerolls * 100 + 100 * level / 3}";
        CheckAllButtons();
    }


    private void CheckAllButtons()
    {
        foreach (var item in itemSlots)
        {
            item.interactable = Map.Manager.money >= level * 10;
        }

        foreach (var skill in skillSots)
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
        if(itemSlots.Contains(currentButton) || skillSots.Contains(currentButton))
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
