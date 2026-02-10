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
        if (context.performed)
        {
            switch (state)
            {
                case State.Combat:
                    battleSystem.Submit();
                    break;
            }
        }
    }

    public void navigate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (state)
            {
                case State.Combat:
                    battleSystem.Navigate(context.ReadValue<Vector2>());
                    break;
            }
        }
    }
}