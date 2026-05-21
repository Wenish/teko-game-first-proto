using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneService
{
    private string _currentScene = string.Empty;

    public async UniTask LoadSceneAsync(string sceneName)
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
}