using VContainer;
using VContainer.Unity;
using MessagePipe;

public class AppLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterMessagePipe();
        builder.Register<SceneService>(Lifetime.Singleton);
        builder.Register<LoadingScreenService>(Lifetime.Singleton);

        builder.RegisterEntryPoint<GameEntryPoint>();

        builder.RegisterComponentOnNewGameObject<AudioPlayer>(Lifetime.Singleton);

        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<AudioPlayer>();
        });
    }
}