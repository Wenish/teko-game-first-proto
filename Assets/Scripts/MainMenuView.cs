using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class MainMenuView : MonoBehaviour
{
    private UIDocumentConfig _uiDocumentConfig;
    private SceneService _sceneService;

    private UIDocument _uiDocument;

    [Inject]
    public void Construct([Key(UIDocumentConfig.UIType.MainMenu)] UIDocumentConfig uiDocumentConfig, SceneService sceneService)
    {
        _uiDocumentConfig = uiDocumentConfig;
        _sceneService = sceneService;
    }

    private void Awake()
    {
        _uiDocument = gameObject.AddComponent<UIDocument>();
        _uiDocument.panelSettings = _uiDocumentConfig.PanelSettings;
        _uiDocument.visualTreeAsset = _uiDocumentConfig.VisualTreeAsset;
    }

    private void Start()
    {
        var root = _uiDocument.rootVisualElement;

        root.Q<Button>("play-button").clicked += OnPlayButtonClicked;
    }

    public void OnPlayButtonClicked()
    {
        _sceneService.LoadSceneAsync("GameplayScene").Forget();
    }
}