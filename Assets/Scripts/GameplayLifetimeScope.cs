using VContainer;
using VContainer.Unity;
using UnityEngine;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField]
    private FrogPlayerSettings _frogPlayerSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        if (_frogPlayerSettings == null)
        {
            _frogPlayerSettings = ScriptableObject.CreateInstance<FrogPlayerSettings>();
        }

        builder.RegisterInstance(_frogPlayerSettings);

        builder.Register<CoinService>(Lifetime.Scoped);
        builder.Register<CoinStatisticsService>(Lifetime.Scoped);
        builder.Register<WinConditionService>(Lifetime.Scoped);
        builder.Register<FrogInputStateService>(Lifetime.Scoped);
        builder.Register<FrogGroundStateService>(Lifetime.Scoped);
        builder.RegisterEntryPoint<FrogJumpChargeService>(Lifetime.Scoped).AsImplementedInterfaces();

        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<CoinStatisticsService>();
        });

        builder.RegisterComponentInHierarchy<PlayerInputManager>();
        builder.RegisterComponentInHierarchy<PlayerFrogMovement>();
        builder.RegisterComponentInHierarchy<DebugCoinView>();
        builder.RegisterComponentInHierarchy<GameHudView>();
    }
}