using R3;
using UnityEngine;
using VContainer;

public class DebugCoinView : MonoBehaviour
{
    private CoinService _coinService;

    [Inject]
    public void Construct(CoinService coinService)
    {
        _coinService = coinService;
    }

    private void Start()
    {
        _coinService.Coins
            .Subscribe(coins => Debug.Log($"Coins: {coins}"))
            .AddTo(this);
    }
}