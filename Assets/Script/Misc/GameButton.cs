using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Action OnSelectEvent;
    public Action OnSpecificAction;
    public Action OnDeselectEvent;

    public void OnSelect(BaseEventData eventData)
    {
        OnSelectEvent?.Invoke();
    }
    
    //TODO: make something for animation events

    public void OnDeselect(BaseEventData eventData)
    {
        OnDeselectEvent?.Invoke();
    }
}
