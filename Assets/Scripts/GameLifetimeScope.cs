using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CoinService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<PlayerMovement>();
        builder.RegisterComponentInHierarchy<DebugCoinView>();
    }
}