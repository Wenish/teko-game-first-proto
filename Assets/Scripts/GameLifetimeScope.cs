using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CoinService>(Lifetime.Singleton);
        builder.Register<WinConditionService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<PlayerMovement>();
        builder.RegisterComponentInHierarchy<DebugCoinView>();
        builder.RegisterComponentInHierarchy<GameHudView>();
    }
}