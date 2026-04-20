using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameButton : MonoBehaviour, ISelectHandler
{
    public Action OnSelectEvent;

    public void OnSelect(BaseEventData eventData)
    {
        OnSelectEvent?.Invoke();
    }
}
