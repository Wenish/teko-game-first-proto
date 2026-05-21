using R3;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

[RequireComponent(typeof(UIDocument))]
public class GameHudView : MonoBehaviour
{
    private CoinService _coinService;
    private WinConditionService _winConditionService;

    private Label _coinLabel;
    private Label _winLabel;

    [Inject]
    public void Construct(
        CoinService coinService,
        WinConditionService winConditionService)
    {
        _coinService = coinService;
        _winConditionService = winConditionService;
    }

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _coinLabel = root.Q<Label>("coin-label");
        _winLabel = root.Q<Label>("win-label");

        _winLabel.style.display = DisplayStyle.None;
    }

    private void Start()
    {
        _coinService.Coins
            .Subscribe(UpdateCoinLabel)
            .AddTo(this);

        _winConditionService.HasWon
            .Subscribe(UpdateWinLabel)
            .AddTo(this);
    }

    private void UpdateCoinLabel(int coins)
    {
        _coinLabel.text = $"Coins: {coins}";
    }

    private void UpdateWinLabel(bool hasWon)
    {
        _winLabel.style.display =
            hasWon
                ? DisplayStyle.Flex
                : DisplayStyle.None;
    }
}