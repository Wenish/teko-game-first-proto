using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class MainMenuView : MonoBehaviour
{
    private UIDocumentConfig _uiDocumentConfig;
    private SceneService _sceneService;

    private UIDocument _uiDocument;
    private Button _playButton;
    private Button _quitButton;

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

        _playButton = root.Q<Button>("play-button");
        _quitButton = root.Q<Button>("quit-button");

        if (_playButton != null)
        {
            _playButton.clicked += OnPlayButtonClicked;

            // Delay one frame so the element is attached to panel before focusing.
            root.schedule.Execute(() => _playButton.Focus()).ExecuteLater(0);
        }

        if (_quitButton != null)
        {
            _quitButton.clicked += OnQuitButtonClicked;
        }
    }

    private void OnDestroy()
    {
        if (_playButton != null)
        {
            _playButton.clicked -= OnPlayButtonClicked;
        }

        if (_quitButton != null)
        {
            _quitButton.clicked -= OnQuitButtonClicked;
        }
    }

    public void OnPlayButtonClicked()
    {
        _sceneService.LoadSceneAsync("GameplayScene").Forget();
    }

    public void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}