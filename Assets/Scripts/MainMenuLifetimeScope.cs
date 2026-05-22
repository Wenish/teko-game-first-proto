using VContainer;
using VContainer.Unity;
using UnityEngine;
public class MainMenuLifetimeScope : LifetimeScope
{
    [SerializeField]
    private UIDocumentConfig _mainMenuUIDocumentConfig;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_mainMenuUIDocumentConfig);
        builder.RegisterComponentOnNewGameObject<MainMenuView>(Lifetime.Singleton);

        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<MainMenuView>();
        });
    }
}
