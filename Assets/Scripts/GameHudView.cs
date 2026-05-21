using R3;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

[RequireComponent(typeof(UIDocument))]
public class GameHudView : MonoBehaviour
{
    private const float CoinAnchorTop = 16f;
    private const float CoinAnchorRight = 18f;

    private CoinService _coinService;
    private CoinStatisticsService _coinStatisticsService;
    private WinConditionService _winConditionService;

    private VisualElement _coinAnchor;
    private Label _coinLabel;
    private Label _lifetimeCoinLabel;
    private Label _winLabel;

    [Inject]
    public void Construct(
        CoinService coinService,
        CoinStatisticsService coinStatisticsService,
        WinConditionService winConditionService)
    {
        _coinService = coinService;
        _coinStatisticsService = coinStatisticsService;
        _winConditionService = winConditionService;
    }

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _coinAnchor = root.Q<VisualElement>("coin-anchor");
        _coinLabel = root.Q<Label>("coin-label");
        _lifetimeCoinLabel = root.Q<Label>("lifetime-coin-label");
        _winLabel = root.Q<Label>("win-label");

        ApplySafeAreaToHud();
        _winLabel.style.display = DisplayStyle.None;
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplySafeAreaToHud();
    }

    private void Start()
    {
        _coinService.Coins
            .Subscribe(UpdateCoinLabel)
            .AddTo(this);

        _coinStatisticsService.TotalCoinsCollected
            .Subscribe(UpdateLifetimeCoinLabel)
            .AddTo(this);

        _winConditionService.HasWon
            .Subscribe(UpdateWinLabel)
            .AddTo(this);
    }

    private void UpdateCoinLabel(int coins)
    {
        _coinLabel.text = $"Coins: {coins}";
    }

    private void UpdateLifetimeCoinLabel(int lifetimeCoins)
    {
        _lifetimeCoinLabel.text = $"Lifetime: {lifetimeCoins}";
    }

    private void UpdateWinLabel(bool hasWon)
    {
        _winLabel.style.display =
            hasWon
                ? DisplayStyle.Flex
                : DisplayStyle.None;
    }

    private void ApplySafeAreaToHud()
    {
        if (_coinAnchor == null)
        {
            return;
        }

        var safeArea = Screen.safeArea;
        var rightInset = Screen.width - safeArea.xMax;
        var topInset = Screen.height - safeArea.yMax;

        _coinAnchor.style.right = CoinAnchorRight + rightInset;
        _coinAnchor.style.top = CoinAnchorTop + topInset;
    }
}