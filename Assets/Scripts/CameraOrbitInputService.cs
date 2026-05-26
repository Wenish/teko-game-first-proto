using UnityEngine;

public class CameraOrbitInputService
{
    private Vector2 _orbitLookAccumulator;
    private bool _isOrbitPressed;

    public bool IsOrbitPressed => _isOrbitPressed;

    public void SetOrbitPressed(bool isPressed)
    {
        _isOrbitPressed = isPressed;
    }

    public void AddOrbitLookInput(Vector2 lookInput)
    {
        if (!_isOrbitPressed)
        {
            return;
        }

        if (lookInput.sqrMagnitude <= 0f)
        {
            return;
        }

        _orbitLookAccumulator += lookInput;
    }

    public Vector2 ConsumeOrbitLookInput()
    {
        Vector2 consumedInput = _orbitLookAccumulator;
        _orbitLookAccumulator = Vector2.zero;
        return consumedInput;
    }
}
