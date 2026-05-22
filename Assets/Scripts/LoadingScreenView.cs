using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using R3;

public class LoadingScreenView : MonoBehaviour
{
    private UIDocumentConfig _config;
    private LoadingScreenService _loadingScreenService;
    private UIDocument _document;
    private readonly CompositeDisposable _disposables = new();

    [Inject]
    public void Construct(
        [Key(UIDocumentConfig.UIType.LoadingScreen)] UIDocumentConfig config,
        LoadingScreenService loadingScreenService)
    {
        _config = config;
        _loadingScreenService = loadingScreenService;
    }

    private void Awake()
    {
        _document = gameObject.AddComponent<UIDocument>();

        _document.panelSettings = _config.PanelSettings;
        _document.visualTreeAsset = _config.VisualTreeAsset;
        _document.sortingOrder = 100; // Ensure the loading screen is on top of other UI.
    }

    private void Start()
    {
        // Apply current state immediately and react to changes.
        SetVisible(_loadingScreenService.IsVisible.CurrentValue);

        _loadingScreenService.IsVisible
            .Subscribe(SetVisible)
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }

    private void SetVisible(bool isVisible)
    {
        var root = _document.rootVisualElement;
        root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        root.pickingMode = isVisible ? PickingMode.Position : PickingMode.Ignore;
    }
}