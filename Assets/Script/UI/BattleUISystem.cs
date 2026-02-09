using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleUISystem : MonoBehaviour
{
    public static BattleUISystem system;
    
    Stack<BattleUIPanel> uiStack = new();

    private void Awake()
    {
        if(system == null) system = this;
        else Destroy(gameObject);
    }

    public void OnCancel()
    {
        if (uiStack.Count > 1)
        {
            uiStack.Pop().gameObject.SetActive(false);
            uiStack.Peek().gameObject.SetActive(true);
        }
    }

    public void OnNext(BattleUIPanel canvas)
    {
        if (uiStack.Count > 0)
        {
            canvas.gameObject.SetActive(true);
            uiStack.Peek().gameObject.SetActive(false);
            uiStack.Push(canvas);
        }
        else
        {
            uiStack.Push(canvas);
            canvas.gameObject.SetActive(true);
        }
    }
}
