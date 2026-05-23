using R3;
using UnityEngine;

public class FrogInputStateService
{
    private readonly ReactiveProperty<Vector2> _moveInput = new(Vector2.zero);
    private readonly ReactiveProperty<bool> _isJumpPressed = new(false);

    public ReadOnlyReactiveProperty<Vector2> MoveInput => _moveInput;
    public ReadOnlyReactiveProperty<bool> IsJumpPressed => _isJumpPressed;

    public void SetMoveInput(Vector2 moveInput)
    {
        _moveInput.Value = moveInput;
    }

    public void SetJumpPressed(bool isPressed)
    {
        _isJumpPressed.Value = isPressed;
    }
}