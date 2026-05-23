public readonly struct FrogJumpReleasedEvent
{
    public float ChargeNormalized { get; }

    public FrogJumpReleasedEvent(float chargeNormalized)
    {
        ChargeNormalized = chargeNormalized;
    }
}