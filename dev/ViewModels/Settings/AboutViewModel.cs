namespace AlAnvar.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    public string alAnvarVersion = $"الانوار v{App.Current.AlAnvarVersion}";
}
