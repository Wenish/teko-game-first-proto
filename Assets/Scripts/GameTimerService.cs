using System;
using R3;
using UnityEngine;
using VContainer.Unity;

public class GameTimerService : IDisposable
{
    private const string BestTimeSecondsKey = "best_time_seconds";
    private const float NoBestTimeSentinel = -1f;

    private readonly IDisposable _moveInputSubscription;
    private readonly IDisposable _jumpInputSubscription;
    private readonly IDisposable _winConditionSubscription;

    private readonly ReactiveProperty<float> _elapsedSeconds = new(0f);
    private readonly ReactiveProperty<float> _bestTimeSeconds;

    private bool _isRunning;

    public ReadOnlyReactiveProperty<float> ElapsedSeconds => _elapsedSeconds;
    public ReadOnlyReactiveProperty<float> BestTimeSeconds => _bestTimeSeconds;

    public GameTimerService(
        FrogInputStateService frogInputStateService,
        WinConditionService winConditionService)
    {
        var persistedBestTime =
            PlayerPrefs.HasKey(BestTimeSecondsKey)
                ? PlayerPrefs.GetFloat(BestTimeSecondsKey)
                : NoBestTimeSentinel;

        _bestTimeSeconds = new ReactiveProperty<float>(persistedBestTime);

        _moveInputSubscription = frogInputStateService.MoveInput.Subscribe(OnMoveInputChanged);
        _jumpInputSubscription = frogInputStateService.IsJumpPressed.Subscribe(OnJumpInputChanged);
        _winConditionSubscription = winConditionService.HasWon.Subscribe(OnWinConditionChanged);
    }

    public void Tick()
    {
        if (!_isRunning)
        {
            return;
        }

        _elapsedSeconds.Value += Time.deltaTime;
    }

    public void Dispose()
    {
        _moveInputSubscription.Dispose();
        _jumpInputSubscription.Dispose();
        _winConditionSubscription.Dispose();
        _elapsedSeconds.Dispose();
        _bestTimeSeconds.Dispose();
    }

    private void OnMoveInputChanged(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude > 0f)
        {
            StartTimerIfNeeded();
        }
    }

    private void OnJumpInputChanged(bool isJumpPressed)
    {
        if (isJumpPressed)
        {
            StartTimerIfNeeded();
        }
    }

    private void OnWinConditionChanged(bool hasWon)
    {
        if (!hasWon)
        {
            return;
        }

        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;

        var finalTime = _elapsedSeconds.CurrentValue;
        var currentBestTime = _bestTimeSeconds.CurrentValue;
        bool isFirstTime = currentBestTime < 0f;

        if (!isFirstTime && finalTime >= currentBestTime)
        {
            return;
        }

        _bestTimeSeconds.Value = finalTime;
        PlayerPrefs.SetFloat(BestTimeSecondsKey, finalTime);
        PlayerPrefs.Save();
    }

    private void StartTimerIfNeeded()
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
    }
}

public class GameTimerTickEntryPoint : ITickable
{
    private readonly GameTimerService _gameTimerService;

    public GameTimerTickEntryPoint(GameTimerService gameTimerService)
    {
        _gameTimerService = gameTimerService;
    }

    public void Tick()
    {
        _gameTimerService.Tick();
    }
}
