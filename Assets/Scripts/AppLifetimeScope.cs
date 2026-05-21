using VContainer;
using VContainer.Unity;
using MessagePipe;

public class AppLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterMessagePipe();
        builder.Register<SceneService>(Lifetime.Singleton);

        builder.RegisterEntryPoint<GameController>();

        builder.RegisterComponentOnNewGameObject<AudioPlayer>(Lifetime.Singleton, "AudioPlayer");

        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<AudioPlayer>();
        });
    }
}