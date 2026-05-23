using R3;

public interface IFrogChargeStateReader
{
    ReadOnlyReactiveProperty<float> ChargeNormalized { get; }
    ReadOnlyReactiveProperty<float> ChargePercent { get; }
    ReadOnlyReactiveProperty<bool> IsCharging { get; }
    ReadOnlyReactiveProperty<bool> IsChargeAscending { get; }
}