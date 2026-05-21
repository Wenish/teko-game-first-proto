using VContainer;
using VContainer.Unity;

public class AppLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<SceneService>(Lifetime.Singleton);

        builder.RegisterEntryPoint<GameController>();

        builder.RegisterComponentOnNewGameObject<AudioPlayer>(Lifetime.Singleton, "AudioPlayer");

        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<AudioPlayer>();
        });
    }
}