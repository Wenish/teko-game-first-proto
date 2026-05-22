using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

public class GameEntryPoint : IAsyncStartable
{
    private SceneService _sceneService;

    [Inject]
    public void Construct(SceneService sceneService)
    {
        _sceneService = sceneService;
    }

    public async UniTask StartAsync(CancellationToken cancellation = default)
    {
        await _sceneService.LoadMainMenuSceneAsync();
    }
}