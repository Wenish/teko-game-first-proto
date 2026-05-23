using UnityEngine;

public readonly struct FrogJumpReleasedEvent
{
    public float ChargeNormalized { get; }
    public Vector2 MoveInput { get; }

    public FrogJumpReleasedEvent(float chargeNormalized, Vector2 moveInput)
    {
        ChargeNormalized = chargeNormalized;
        MoveInput = moveInput;
    }
}