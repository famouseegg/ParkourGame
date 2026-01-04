using UnityEngine;
using UnityEngine.InputSystem;
public class StarterAssetsInputs : MonoBehaviour
{
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool analogMovement;
    public bool dive;
    public bool attack;

    public float zoom;
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            LookInput(value.Get<Vector2>());
        }
    }

    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
    }

    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }

    public void OnZoom(InputValue value)
    {
        ZoomInput(value.Get<float>());
    }

    public void OnDive(InputValue value)
    {
        DiveInput(value.isPressed);
    }

    public void DiveInput(bool newDiveState)
    {
        dive = newDiveState;
    }

    public void OnAttack(InputValue value)
    {
        AttackInput(value.isPressed);
    }

    public void AttackInput(bool newAttackState)
    {
        attack = newAttackState;
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    public void ZoomInput(float newZoomValue)
    {
        zoom = newZoomValue;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
 