using VContainer;
using VContainer.Unity;
using UnityEngine;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField]
    private FrogPlayerSettings _frogPlayerSettings;

    [SerializeField]
    private CameraOrbitSettings _cameraOrbitSettings;

    [SerializeField]
    private UIDocumentConfig _gameHudUIDocumentConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        if (_frogPlayerSettings == null)
        {
            _frogPlayerSettings = ScriptableObject.CreateInstance<FrogPlayerSettings>();
        }

        if (_cameraOrbitSettings == null)
        {
            _cameraOrbitSettings = ScriptableObject.CreateInstance<CameraOrbitSettings>();
        }

        var hasGameHudConfig = _gameHudUIDocumentConfig != null;
        if (hasGameHudConfig)
        {
            builder.RegisterInstance(_gameHudUIDocumentConfig)
                .Keyed(UIDocumentConfig.UIType.GameHud);
        }
        else
        {
            Debug.LogError(
                $"{nameof(GameplayLifetimeScope)} on '{name}' missing {nameof(UIDocumentConfig)} for GameHud. " +
                "Assign _gameHudUIDocumentConfig in inspector.");
        }

        builder.RegisterInstance(_frogPlayerSettings);
        builder.RegisterInstance(_cameraOrbitSettings);

        builder.Register<CoinService>(Lifetime.Scoped);
        builder.Register<CoinStatisticsService>(Lifetime.Scoped);
        builder.Register<GameTimerService>(Lifetime.Scoped);
        builder.Register<WinConditionService>(Lifetime.Scoped);
        builder.Register<FrogInputStateService>(Lifetime.Scoped);
        builder.Register<FrogGroundStateService>(Lifetime.Scoped);
        builder.Register<CameraOrbitInputService>(Lifetime.Scoped);
        builder.RegisterEntryPoint<GameTimerTickEntryPoint>(Lifetime.Scoped).AsImplementedInterfaces();
        builder.RegisterEntryPoint<EscapeToMenuService>(Lifetime.Scoped).AsImplementedInterfaces();
        builder.RegisterEntryPoint<FrogJumpChargeService>(Lifetime.Scoped).AsImplementedInterfaces();
        builder.RegisterEntryPoint<GhostRunService>(Lifetime.Scoped).AsImplementedInterfaces();

        if (hasGameHudConfig)
        {
            builder.RegisterComponentOnNewGameObject<GameHudView>(Lifetime.Singleton);
        }

        builder.RegisterComponentInHierarchy<PlayerInputManager>();
        builder.RegisterComponentInHierarchy<PlayerFrogMovement>();
        builder.RegisterComponentInHierarchy<CameraOrbitView>();
        builder.RegisterComponentInHierarchy<DebugCoinView>();

        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<CoinStatisticsService>();

            if (hasGameHudConfig)
            {
                container.Resolve<GameHudView>();
            }
        });
    }
}