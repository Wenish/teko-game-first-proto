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
    private IFrogChargeStateReader _frogChargeStateReader;

    private VisualElement _coinAnchor;
    private VisualElement _chargeAnchor;
    private VisualElement _chargeFill;
    private Label _coinLabel;
    private Label _lifetimeCoinLabel;
    private Label _winLabel;

    [Inject]
    public void Construct(
        CoinService coinService,
        CoinStatisticsService coinStatisticsService,
        WinConditionService winConditionService,
        IFrogChargeStateReader frogChargeStateReader)
    {
        _coinService = coinService;
        _coinStatisticsService = coinStatisticsService;
        _winConditionService = winConditionService;
        _frogChargeStateReader = frogChargeStateReader;
    }

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _coinAnchor = root.Q<VisualElement>("coin-anchor");
        _chargeAnchor = root.Q<VisualElement>("charge-anchor");
        _chargeFill = root.Q<VisualElement>("frog-charge-fill");
        _coinLabel = root.Q<Label>("coin-label");
        _lifetimeCoinLabel = root.Q<Label>("lifetime-coin-label");
        _winLabel = root.Q<Label>("win-label");

        ApplySafeAreaToHud();
        if (_winLabel != null)
        {
            _winLabel.style.display = DisplayStyle.None;
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplySafeAreaToHud();
    }

    private void Start()
    {
        if (_coinService != null)
        {
            _coinService.Coins
                .Subscribe(UpdateCoinLabel)
                .AddTo(this);
        }
        else
        {
            Debug.LogWarning("GameHudView missing CoinService injection.");
        }

        if (_coinStatisticsService != null)
        {
            _coinStatisticsService.TotalCoinsCollected
                .Subscribe(UpdateLifetimeCoinLabel)
                .AddTo(this);
        }
        else
        {
            Debug.LogWarning("GameHudView missing CoinStatisticsService injection.");
        }

        if (_winConditionService != null)
        {
            _winConditionService.HasWon
                .Subscribe(UpdateWinLabel)
                .AddTo(this);
        }
        else
        {
            Debug.LogWarning("GameHudView missing WinConditionService injection.");
        }

        if (_frogChargeStateReader == null)
        {
            Debug.LogError("GameHudView missing IFrogChargeStateReader injection.");
            return;
        }

        _frogChargeStateReader.ChargeNormalized
            .Subscribe(UpdateChargeFill)
            .AddTo(this);

        _frogChargeStateReader.IsCharging
            .Subscribe(UpdateChargeVisibility)
            .AddTo(this);
    }

    private void UpdateCoinLabel(int coins)
    {
        if (_coinLabel != null)
        {
            _coinLabel.text = $"Coins: {coins}";
        }
    }

    private void UpdateLifetimeCoinLabel(int lifetimeCoins)
    {
        if (_lifetimeCoinLabel != null)
        {
            _lifetimeCoinLabel.text = $"Lifetime: {lifetimeCoins}";
        }
    }

    private void UpdateWinLabel(bool hasWon)
    {
        if (_winLabel != null)
        {
            _winLabel.style.display =
                hasWon
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }
    }

    private void UpdateChargeFill(float chargeNormalized)
    {
        if (_chargeFill == null)
        {
            return;
        }

        float percent = Mathf.Clamp01(chargeNormalized) * 100f;
        _chargeFill.style.width = new Length(percent, LengthUnit.Percent);
    }

    private void UpdateChargeVisibility(bool isCharging)
    {
        if (_chargeAnchor == null)
        {
            return;
        }

        _chargeAnchor.style.display = isCharging ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ApplySafeAreaToHud()
    {
        var safeArea = Screen.safeArea;
        var rightInset = Screen.width - safeArea.xMax;
        var topInset = Screen.height - safeArea.yMax;

        if (_coinAnchor != null)
        {
            _coinAnchor.style.right = CoinAnchorRight + rightInset;
            _coinAnchor.style.top = CoinAnchorTop + topInset;
        }

        if (_chargeAnchor != null)
        {
            _chargeAnchor.style.left = 16f + safeArea.x;
            _chargeAnchor.style.bottom = 16f + safeArea.y;
        }
    }
}