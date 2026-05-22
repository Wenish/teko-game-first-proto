using R3;

public class LoadingScreenService
{
    private readonly ReactiveProperty<bool> _isVisible = new(false);

    public ReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

    public void Show()
    {
        _isVisible.Value = true;
    }

    public void Hide()
    {
        _isVisible.Value = false;
    }
}