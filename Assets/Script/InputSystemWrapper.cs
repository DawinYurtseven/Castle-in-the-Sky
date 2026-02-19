using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemWrapper : MonoBehaviour
{
    private enum State
    {
        Combat,
        Menu,
        Dialogue
    }

    private State state;

    [SerializeField] private BattleSystem battleSystem;

    public void Submit(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (state)
        {
            case State.Combat:
                battleSystem.Submit();
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

    public void RightShoulderButton(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (state)
        {
            case State.Combat:
                break;
        }
    }

    public void Navigate(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        switch (state)
        {
            case State.Combat:
                battleSystem.Navigate(context.ReadValue<Vector2>());
                break;
        }
    }
}