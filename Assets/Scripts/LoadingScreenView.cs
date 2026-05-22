using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using R3;

public class LoadingScreenView : MonoBehaviour
{
    private const float OuterRingDegreesPerSecond = 90f;
    private const float InnerRingDegreesPerSecond = 130f;

    private UIDocumentConfig _config;
    private LoadingScreenService _loadingScreenService;
    private UIDocument _document;
    private VisualElement _outerRing;
    private VisualElement _innerRing;
    private float _outerRingAngle;
    private float _innerRingAngle;
    private bool _isVisible;
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
        var root = _document.rootVisualElement;
        _outerRing = root.Q<VisualElement>("indicator-ring-outer");
        _innerRing = root.Q<VisualElement>("indicator-ring-inner");

        // Apply current state immediately and react to changes.
        SetVisible(_loadingScreenService.IsVisible.CurrentValue);

        _loadingScreenService.IsVisible
            .Subscribe(SetVisible)
            .AddTo(_disposables);
    }

    private void Update()
    {
        if (!_isVisible)
        {
            return;
        }

        var delta = Time.deltaTime;

        // Outer ring rotates counterclockwise, inner ring rotates clockwise.
        _outerRingAngle -= OuterRingDegreesPerSecond * delta;
        _innerRingAngle += InnerRingDegreesPerSecond * delta;

        if (_outerRing != null)
        {
            _outerRing.style.rotate = new Rotate(new Angle(_outerRingAngle));
        }

        if (_innerRing != null)
        {
            _innerRing.style.rotate = new Rotate(new Angle(_innerRingAngle));
        }
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }

    private void SetVisible(bool isVisible)
    {
        _isVisible = isVisible;

        var root = _document.rootVisualElement;
        root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        root.pickingMode = isVisible ? PickingMode.Position : PickingMode.Ignore;
    }
}