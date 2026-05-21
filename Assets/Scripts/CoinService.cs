using System;
using MessagePipe;
using R3;

public class CoinService : IDisposable
{
    private readonly IDisposable _coinCollectedSubscription;
    private readonly ReactiveProperty<int> _coins = new(0);

    public ReadOnlyReactiveProperty<int> Coins => _coins;

    public CoinService(ISubscriber<CoinCollectedEvent> coinCollectedSubscriber)
    {
        _coinCollectedSubscription = coinCollectedSubscriber.Subscribe(OnCoinCollected);
    }

    private void OnCoinCollected(CoinCollectedEvent e)
    {
        AddCoin(e.Amount);
    }

    private void AddCoin(int amount)
    {
        _coins.Value += amount;
    }

    public void Dispose()
    {
        _coinCollectedSubscription.Dispose();
        _coins.Dispose();
    }
}