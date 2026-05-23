using R3;

public class FrogGroundStateService
{
    private readonly ReactiveProperty<bool> _isGrounded = new(false);

    public ReadOnlyReactiveProperty<bool> IsGrounded => _isGrounded;

    public void SetIsGrounded(bool isGrounded)
    {
        _isGrounded.Value = isGrounded;
    }
}