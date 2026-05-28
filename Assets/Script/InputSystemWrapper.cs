using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemWrapper : MonoBehaviour
{
    public static InputSystemWrapper instance;

    private void Awake()
    {
        instance = this;
    }
    public enum State
    {
        Combat,
        Map,
        Menu,
        Dialogue
    }

    private State state;

    public void SetState(State state)
    {
        this.state = state;
    }

    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private Map map;

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
                break;
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
            bool isVertical = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);
            bool same = false;
            var temp = Vector2.zero;
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
                battleSystem.Navigate(context.ReadValue<Vector2>());
                break;
            case State.Map:
                map.Navigate(context.ReadValue<Vector2>());
                break;
        }
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        //TODO: Universal pause button? 
    }
    
}