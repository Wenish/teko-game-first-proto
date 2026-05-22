using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneService
{
    private readonly LoadingScreenService _loadingScreen;
    private string _currentScene = string.Empty;

    public SceneService(LoadingScreenService loadingScreen)
    {
        _loadingScreen = loadingScreen;
    }

    public UniTask LoadMainMenuSceneAsync()
    {
        return LoadSceneAsync("MainMenuScene");
    }

    public async UniTask LoadSceneAsync(string sceneName)
    {
        _loadingScreen.Show();

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

        _loadingScreen.Hide();
    }
}