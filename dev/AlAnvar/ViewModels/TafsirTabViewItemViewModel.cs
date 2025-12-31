
namespace AlAnvar.ViewModels;

public partial class TafsirTabViewItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Query { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }
}
