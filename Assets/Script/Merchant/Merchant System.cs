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

    private void Awake()
    {
        if (Manager) Destroy(this);
        else Manager = this;
    }

    public void Reroll()
    {
        
        //Add cost text to the slots?
        
        var currentBatch = new List<Items>();
        foreach (var item in itemSlots)
        {
            item.interactable = true;
            var gift = Items.GetRandomItem(currentBatch);
            currentBatch.Add(gift);
            item.GetComponent<GameButton>().OnSelectEvent = () =>
            {
                title.text = gift.ItemName;
                description.text = gift.ItemDescription;
            };
            if(gift.ItemImage)
                item.image.sprite = gift.ItemImage;
            item.onClick.RemoveAllListeners();
            item.onClick.AddListener(() =>
            {
                gift.Acquire(new List<Unit>(Map.Manager.currentPlayerUnits));
            });
        }

        var nextBatch = new List<Skill>();
        foreach (var skill in skillSots)
        {
            skill.interactable = true;
            var gift = Skill.GetRandomSkill(nextBatch);
            nextBatch.Add(gift);
            skill.GetComponent<GameButton>().OnSelectEvent = () =>
            {
                title.text = gift.skillName;
                description.text = gift.skillDescription;
            };
            if(gift.skillImage)
                skill.image.sprite = gift.skillImage;
            
            //replace skill window needed for this one
        }

        SetCurrentSelectButton(itemSlots[0]);
    }


    private Button currentButton;

    #region Input

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
