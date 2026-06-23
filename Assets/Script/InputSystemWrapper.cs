using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemWrapper : MonoBehaviour
{
    public static InputSystemWrapper Instance;

    private void Awake()
    {
        Instance = this;
    }
    public enum State
    {
        Combat,
        Map,
        Menu,
        Dialogue
    }

    private State state;

    public void SetState(State nextState)
    {
        state = nextState;
    }

    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private Map map;
    [SerializeField] private StoryManager storyManager;

    public void Submit(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (state)
        {
            case State.Combat:
                battleSystem.Submit();
                break;
            case State.Map:
                map.Submit();
                break;
            case State.Menu:
                break;
            case State.Dialogue:
                storyManager.Submit();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (state)
        {
            case State.Combat:
                battleSystem.Cancel();
                break;
            case State.Map:
                map.Cancel();
                break;
            case State.Menu:
            case State.Dialogue:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void WestButton(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (state)
        {
            case State.Combat:
                battleSystem.SkillTab();
                break;
            case State.Map:
            case State.Menu:
            case State.Dialogue:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void NorthButton(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (state)
        {
            case State.Combat:
                battleSystem.InspectTab();
                break;
            case State.Map:
            case State.Menu:
            case State.Dialogue:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RightShoulderButton(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (state)
        {
            case State.Combat:
                battleSystem.SwitchTab();
                break;
            case State.Map:
            case State.Menu:
            case State.Dialogue:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //up, down, left, right
    private Vector2 previousDirection = Vector2.zero;
    public void Navigate(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            previousDirection = Vector2.zero;
            return;
        }
        if (!context.performed) return;
        var dir = context.ReadValue<Vector2>();
        if (dir != Vector2.zero)
        {
            var isVertical = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);
            bool same;
            Vector2 temp;
            if (isVertical)
            {
                same = dir.y > 0 ? previousDirection == Vector2.up : previousDirection == Vector2.down;
                temp = dir.y > 0 ? Vector2.up : Vector2.down;
            }
            else
            {
                same = dir.x > 0 ? previousDirection == Vector2.right : previousDirection == Vector2.left;
                temp = dir.x > 0 ? Vector2.right : Vector2.left;
            }
            if(same) return;
            previousDirection = temp;
        }
        switch (state)
        {
            case State.Combat:
                battleSystem.Navigate(dir);
                break;
            case State.Map:
                map.Navigate(dir);
                break;
            case State.Dialogue:
                storyManager.Navigate(dir);
                break;
            case State.Menu:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("yo, what?");
        }
        //TODO: Universal pause button? 
    }
    
}