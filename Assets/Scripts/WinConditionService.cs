using R3;

public class WinConditionService
{
    private readonly ReactiveProperty<bool> _hasWon = new(false);

    public ReadOnlyReactiveProperty<bool> HasWon => _hasWon;

    public WinConditionService(CoinService coinService)
    {
        coinService.Coins
            .Subscribe(CheckWinCondition);
    }

    private void CheckWinCondition(int coins)
    {
        if (coins >= 3)
        {
            _hasWon.Value = true;
        }
    }
}