using R3;

public class CoinService
{
    private readonly ReactiveProperty<int> _coins = new(0);

    public ReadOnlyReactiveProperty<int> Coins => _coins;

    public void AddCoin(int amount)
    {
        _coins.Value += amount;
    }
}