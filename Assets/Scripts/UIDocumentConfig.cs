using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Game/UI Document Config", fileName = "UIDocumentConfig")]
public class UIDocumentConfig : ScriptableObject
{
    public PanelSettings PanelSettings;
    public VisualTreeAsset VisualTreeAsset;
}