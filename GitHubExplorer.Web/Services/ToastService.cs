namespace GitHubExplorer.Web.Services;

public enum ToastLevel { Success, Error, Info }

public sealed record Toast(Guid Id, ToastLevel Level, string Message);

public sealed class ToastService
{
    private readonly List<Toast> _toasts = new();
    public IReadOnlyList<Toast> Toasts => _toasts;

    public event Action? OnChange;

    public void ShowSuccess(string m) => Show(ToastLevel.Success, m);
    public void ShowError(string m) => Show(ToastLevel.Error, m);
    public void ShowInfo(string m) => Show(ToastLevel.Info, m);

    private void Show(ToastLevel level, string message)
    {
        var toast = new Toast(Guid.NewGuid(), level, message);
        _toasts.Add(toast);
        OnChange?.Invoke();
        _ = RemoveAfterDelay(toast.Id);
    }

    public void Remove(Guid id)
    {
        _toasts.RemoveAll(t => t.Id == id);
        OnChange?.Invoke();
    }

    private async Task RemoveAfterDelay(Guid id)
    {
        await Task.Delay(4000);
        Remove(id);
    }
}