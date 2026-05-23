using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class EscapeToMenuService : ITickable
{
    private readonly SceneService _sceneService;
    private bool _isLoading;

    public EscapeToMenuService(SceneService sceneService)
    {
        _sceneService = sceneService;
    }

    public void Tick()
    {
        if (_isLoading)
        {
            return;
        }

        if (!IsEscapePressedThisFrame())
        {
            return;
        }

        _isLoading = true;
        _sceneService.LoadMainMenuSceneAsync().Forget();
    }

    private static bool IsEscapePressedThisFrame()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }
}