using R3;
using UnityEngine;

public class FrogInputStateService
{
    private readonly ReactiveProperty<Vector2> _moveInput = new(Vector2.zero);
    private readonly ReactiveProperty<bool> _isJumpPressed = new(false);
    private readonly ReactiveProperty<float> _mouseTurnInput = new(0f);
    private float _mouseTurnAccumulator;

    public ReadOnlyReactiveProperty<Vector2> MoveInput => _moveInput;
    public ReadOnlyReactiveProperty<bool> IsJumpPressed => _isJumpPressed;
    public ReadOnlyReactiveProperty<float> MouseTurnInput => _mouseTurnInput;

    public void SetMoveInput(Vector2 moveInput)
    {
        _moveInput.Value = moveInput;
    }

    public void SetJumpPressed(bool isPressed)
    {
        _isJumpPressed.Value = isPressed;
    }

    public void SetMouseTurnInput(float mouseTurnInput)
    {
        _mouseTurnAccumulator = mouseTurnInput;
        _mouseTurnInput.Value = mouseTurnInput;
    }

    public void AddMouseTurnInput(float mouseTurnInput)
    {
        if (Mathf.Approximately(mouseTurnInput, 0f))
        {
            return;
        }

        _mouseTurnAccumulator += mouseTurnInput;
        _mouseTurnInput.Value = mouseTurnInput;
    }

    public float ConsumeMouseTurnInput()
    {
        float consumedInput = _mouseTurnAccumulator;
        _mouseTurnAccumulator = 0f;
        return consumedInput;
    }
}