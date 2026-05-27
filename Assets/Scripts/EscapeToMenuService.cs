using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class EscapeToMenuService : ITickable
{
    private readonly SceneService _sceneService;

    public EscapeToMenuService(SceneService sceneService)
    {
        _sceneService = sceneService;
    }

    public void Tick()
    {
        if (_sceneService.IsLoading.CurrentValue)
        {
            return;
        }

        if (!IsEscapePressedThisFrame())
        {
            return;
        }

        _sceneService.LoadMainMenuSceneAsync().Forget();
    }

    private static bool IsEscapePressedThisFrame()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }
}