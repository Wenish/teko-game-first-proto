using UnityEngine;
using VContainer;
using VContainer.Unity;
using MessagePipe;

public class AppLifetimeScope : LifetimeScope
{   
    [SerializeField]
    private UIDocumentConfig _loadingScreenUIDocumentConfig;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_loadingScreenUIDocumentConfig)
            .Keyed(UIDocumentConfig.UIType.LoadingScreen);

        builder.RegisterMessagePipe();
        builder.Register<SceneService>(Lifetime.Singleton);
        builder.Register<LoadingScreenService>(Lifetime.Singleton);

        builder.RegisterEntryPoint<GameEntryPoint>();

        builder.RegisterComponentOnNewGameObject<AudioPlayer>(Lifetime.Singleton);
        builder.RegisterComponentOnNewGameObject<LoadingScreenView>(Lifetime.Singleton);
        
        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<AudioPlayer>();
            container.Resolve<LoadingScreenView>();
        });
    }
}