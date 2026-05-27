using R3;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

[RequireComponent(typeof(UIDocument))]
public class GameHudView : MonoBehaviour
{
    UIDocumentConfig _uiDocumentConfig;
    private UIDocument _uiDocument;
    private const float CoinAnchorTop = 16f;
    private const float CoinAnchorRight = 18f;

    private CoinService _coinService;
    private CoinStatisticsService _coinStatisticsService;
    private GameTimerService _gameTimerService;
    private WinConditionService _winConditionService;
    private IFrogChargeStateReader _frogChargeStateReader;

    private VisualElement _coinAnchor;
    private VisualElement _chargeAnchor;
    private VisualElement _chargeFill;
    private Label _coinLabel;
    private Label _lifetimeCoinLabel;
    private Label _timerLabel;
    private Label _bestTimeLabel;
    private Label _winLabel;

    private float _latestBestTimeSeconds = -1f;
    private float _bestTimeAtRunStart = -1f;
    private bool _hasRunCompletionResult;
    private RunCompletionResult _lastRunCompletionResult;

    [Inject]
    public void Construct(
        [Key(UIDocumentConfig.UIType.GameHud)] UIDocumentConfig uiDocumentConfig,
        CoinService coinService,
        CoinStatisticsService coinStatisticsService,
        GameTimerService gameTimerService,
        WinConditionService winConditionService,
        IFrogChargeStateReader frogChargeStateReader)
    {
        _coinService = coinService;
        _coinStatisticsService = coinStatisticsService;
        _gameTimerService = gameTimerService;
        _winConditionService = winConditionService;
        _frogChargeStateReader = frogChargeStateReader;
        _uiDocumentConfig = uiDocumentConfig;   
    }

    private void Awake()
    {
        _uiDocument = gameObject.GetComponent<UIDocument>();
        _uiDocument.panelSettings = _uiDocumentConfig.PanelSettings;
        _uiDocument.visualTreeAsset = _uiDocumentConfig.VisualTreeAsset;
        
        var root = _uiDocument.rootVisualElement;

        _coinAnchor = root.Q<VisualElement>("coin-anchor");
        _chargeAnchor = root.Q<VisualElement>("charge-anchor");
        _chargeFill = root.Q<VisualElement>("frog-charge-fill");
        _coinLabel = root.Q<Label>("coin-label");
        _lifetimeCoinLabel = root.Q<Label>("lifetime-coin-label");
        _timerLabel = root.Q<Label>("timer-label");
        _bestTimeLabel = root.Q<Label>("best-time-label");
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

        if (_gameTimerService != null)
        {
            _gameTimerService.ElapsedSeconds
                .Subscribe(UpdateTimerLabel)
                .AddTo(this);

            _gameTimerService.BestTimeSeconds
                .Subscribe(UpdateBestTimeLabel)
                .AddTo(this);

            _gameTimerService.RunStarted += OnRunStarted;
            _gameTimerService.RunCompleted += OnRunCompleted;
        }
        else
        {
            Debug.LogWarning("GameHudView missing GameTimerService injection.");
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

    private void OnDestroy()
    {
        if (_gameTimerService == null)
        {
            return;
        }

        _gameTimerService.RunStarted -= OnRunStarted;
        _gameTimerService.RunCompleted -= OnRunCompleted;
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

            _winLabel.text = hasWon
                ? BuildWinMessage()
                : string.Empty;
        }
    }

    private void UpdateTimerLabel(float elapsedSeconds)
    {
        if (_timerLabel != null)
        {
            _timerLabel.text = $"Time: {FormatTime(elapsedSeconds)}";
        }
    }

    private void UpdateBestTimeLabel(float bestTimeSeconds)
    {
        _latestBestTimeSeconds = bestTimeSeconds;

        if (_bestTimeLabel == null)
        {
            return;
        }

        _bestTimeLabel.text =
            bestTimeSeconds >= 0f
                ? $"Best: {FormatTime(bestTimeSeconds)}"
                : "Best: --:---.---";
    }

    private void OnRunStarted()
    {
        _bestTimeAtRunStart = _latestBestTimeSeconds;
        _hasRunCompletionResult = false;
    }

    private void OnRunCompleted(RunCompletionResult result)
    {
        _lastRunCompletionResult = result;
        _hasRunCompletionResult = true;

        if (_winConditionService != null && _winConditionService.HasWon.CurrentValue)
        {
            UpdateWinLabel(true);
        }
    }

    private string BuildWinMessage()
    {
        if (!_hasRunCompletionResult)
        {
            return $"You win!\nRun time {GetRunTimeText()}";
        }

        if (!_lastRunCompletionResult.IsNewBest)
        {
            return $"You win!\nRun time {GetRunTimeText()}";
        }

        if (_bestTimeAtRunStart < 0f)
        {
            return $"First finish {FormatTime(_lastRunCompletionResult.FinalTimeSeconds)}!";
        }

        float improvementSeconds = Mathf.Max(0f, _bestTimeAtRunStart - _lastRunCompletionResult.FinalTimeSeconds);
        return $"New record {FormatTime(_lastRunCompletionResult.FinalTimeSeconds)}!\nBeat old record by {FormatTime(improvementSeconds)}";
    }

    private string GetRunTimeText()
    {
        if (_hasRunCompletionResult)
        {
            return FormatTime(_lastRunCompletionResult.FinalTimeSeconds);
        }

        if (_gameTimerService != null)
        {
            return FormatTime(_gameTimerService.ElapsedSeconds.CurrentValue);
        }

        return FormatTime(0f);
    }

    private static string FormatTime(float seconds)
    {
        var clampedSeconds = Mathf.Max(0f, seconds);
        var span = TimeSpan.FromSeconds(clampedSeconds);
        return $"{(int)span.TotalMinutes:00}:{span.Seconds:00}.{span.Milliseconds:000}";
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