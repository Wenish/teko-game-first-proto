using MessagePipe;
using R3;
using UnityEngine;
using VContainer.Unity;

public class FrogJumpChargeService : ITickable, IFrogChargeStateReader
{
    private const float MaxCharge = 1f;
    private const float MinCharge = 0f;

    private readonly FrogPlayerSettings _settings;
    private readonly FrogInputStateService _inputStateService;
    private readonly FrogGroundStateService _groundStateService;
    private readonly IPublisher<FrogJumpReleasedEvent> _jumpReleasedPublisher;

    private readonly ReactiveProperty<float> _chargeNormalized = new(0f);
    private readonly ReactiveProperty<float> _chargePercent = new(0f);
    private readonly ReactiveProperty<bool> _isCharging = new(false);
    private readonly ReactiveProperty<bool> _isChargeAscending = new(true);

    private bool _wasJumpPressed;

    public ReadOnlyReactiveProperty<float> ChargeNormalized => _chargeNormalized;
    public ReadOnlyReactiveProperty<float> ChargePercent => _chargePercent;
    public ReadOnlyReactiveProperty<bool> IsCharging => _isCharging;
    public ReadOnlyReactiveProperty<bool> IsChargeAscending => _isChargeAscending;

    public FrogJumpChargeService(
        FrogPlayerSettings settings,
        FrogInputStateService inputStateService,
        FrogGroundStateService groundStateService,
        IPublisher<FrogJumpReleasedEvent> jumpReleasedPublisher)
    {
        _settings = settings;
        _inputStateService = inputStateService;
        _groundStateService = groundStateService;
        _jumpReleasedPublisher = jumpReleasedPublisher;
    }

    public void Tick()
    {
        bool isJumpPressed = _inputStateService.IsJumpPressed.CurrentValue;
        bool isGrounded = _groundStateService.IsGrounded.CurrentValue;

        if (isJumpPressed && (isGrounded || _isCharging.CurrentValue))
        {
            TickCharge();
        }
        else if (_wasJumpPressed && _isCharging.CurrentValue)
        {
            PublishJumpRelease();
            ResetCharge();
        }
        else if (_isCharging.CurrentValue)
        {
            ResetCharge();
        }

        _wasJumpPressed = isJumpPressed;
    }

    private void TickCharge()
    {
        _isCharging.Value = true;

        float cycleSeconds = Mathf.Max(0.05f, _settings.chargeCycleSeconds);
        float deltaCharge = Time.deltaTime / cycleSeconds;

        float direction = _isChargeAscending.CurrentValue ? 1f : -1f;
        float nextCharge = _chargeNormalized.CurrentValue + (direction * deltaCharge);

        if (nextCharge >= MaxCharge)
        {
            nextCharge = MaxCharge;
            _isChargeAscending.Value = false;
        }
        else if (nextCharge <= MinCharge)
        {
            nextCharge = MinCharge;
            _isChargeAscending.Value = true;
        }

        _chargeNormalized.Value = nextCharge;
        _chargePercent.Value = nextCharge * 100f;
    }

    private void PublishJumpRelease()
    {
        _jumpReleasedPublisher.Publish(
            new FrogJumpReleasedEvent(
                _chargeNormalized.CurrentValue,
                _inputStateService.MoveInput.CurrentValue));
    }

    private void ResetCharge()
    {
        _isCharging.Value = false;
        _isChargeAscending.Value = true;
        _chargeNormalized.Value = 0f;
        _chargePercent.Value = 0f;
    }
}
