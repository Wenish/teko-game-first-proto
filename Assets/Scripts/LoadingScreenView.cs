using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class LoadingScreenView : MonoBehaviour
{
    private UIDocumentConfig _config;

    [Inject]
    public void Construct(
        [Key(UIDocumentConfig.UIType.LoadingScreen)]
        UIDocumentConfig config)
    {
        _config = config;
    }

    private void Awake()
    {
        var document = gameObject.AddComponent<UIDocument>();

        document.panelSettings = _config.PanelSettings;
        document.visualTreeAsset = _config.VisualTreeAsset;
    }
}