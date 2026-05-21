using MessagePipe;
using R3;
using System;
using UnityEngine;

public class CoinStatisticsService : IDisposable
{
    private const string TotalCoinsCollectedKey = "total_coins_collected";
    private readonly ISubscriber<CoinCollectedEvent> _coinCollectedSubscriber;
    private readonly IDisposable _coinCollectedSubscription;
    private readonly ReactiveProperty<int> _totalCoinsCollected;

    public ReadOnlyReactiveProperty<int> TotalCoinsCollected => _totalCoinsCollected;

    public CoinStatisticsService(ISubscriber<CoinCollectedEvent> coinCollectedSubscriber)
    {
        _coinCollectedSubscriber = coinCollectedSubscriber;
        var persistedTotal = PlayerPrefs.GetInt(TotalCoinsCollectedKey, 0);
        _totalCoinsCollected = new ReactiveProperty<int>(persistedTotal);
        _coinCollectedSubscription = _coinCollectedSubscriber.Subscribe(OnCoinCollected);
    }

    private void OnCoinCollected(CoinCollectedEvent e)
    {
        var newTotal = _totalCoinsCollected.Value + e.Amount;
        _totalCoinsCollected.Value = newTotal;

        PlayerPrefs.SetInt(TotalCoinsCollectedKey, newTotal);
        PlayerPrefs.Save();

        Debug.Log($"Total coins collected updated: {newTotal}");
    }

    public void Dispose()
    {
        _coinCollectedSubscription?.Dispose();
        _totalCoinsCollected.Dispose();
    }
}