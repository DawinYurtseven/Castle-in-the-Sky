using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameButton : MonoBehaviour, ISelectHandler
{
    public Action OnSelectEvent;
    public Action OnSpecificAction;

    public void OnSelect(BaseEventData eventData)
    {
        OnSelectEvent?.Invoke();
    }
    
    //TODO: make something for animation events
    
}
