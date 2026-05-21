using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CoinService>(Lifetime.Scoped);
        builder.Register<WinConditionService>(Lifetime.Scoped);

        builder.RegisterComponentInHierarchy<PlayerMovement>();
        builder.RegisterComponentInHierarchy<DebugCoinView>();
        builder.RegisterComponentInHierarchy<GameHudView>();
    }
}