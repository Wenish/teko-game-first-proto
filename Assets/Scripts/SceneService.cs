using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;

public class SceneService
{
    private readonly LoadingScreenService _loadingScreen;
    private readonly ReactiveProperty<bool> _isLoading = new(false);
    private string _currentScene = string.Empty;
    public ReadOnlyReactiveProperty<bool> IsLoading => _isLoading;

    public SceneService(LoadingScreenService loadingScreen)
    {
        _loadingScreen = loadingScreen;
    }

    public UniTask LoadMainMenuSceneAsync()
    {
        return LoadSceneAsync("MainMenuScene");
    }

    public UniTask ReloadCurrentSceneAsync()
    {
        if (string.IsNullOrEmpty(_currentScene))
        {
            return UniTask.CompletedTask;
        }

        return LoadSceneAsync(_currentScene);
    }

    public async UniTask LoadSceneAsync(string sceneName)
    {
        if (_isLoading.CurrentValue)
        {
            return;
        }

        _isLoading.Value = true;
        _loadingScreen.Show();

        try
        {
            if (!string.IsNullOrEmpty(_currentScene))
            {
                var currentScene = SceneManager.GetSceneByName(_currentScene);
                if (currentScene.IsValid() && currentScene.isLoaded)
                {
                    var unloadOperation = SceneManager.UnloadSceneAsync(_currentScene);
                    if (unloadOperation != null)
                    {
                        await unloadOperation.ToUniTask();
                    }
                }
            }

            var loadOperation = SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Additive);

            if (loadOperation != null)
            {
                await loadOperation.ToUniTask();
            }

            _currentScene = sceneName;

            var loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
            }
        }
        finally
        {
            _isLoading.Value = false;
            _loadingScreen.Hide();
        }
    }
}