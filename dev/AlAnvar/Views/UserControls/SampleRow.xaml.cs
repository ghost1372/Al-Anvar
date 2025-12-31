using System.Collections.ObjectModel;

namespace AlAnvar.Views;

internal partial class SampleRow : UserControl
{
    public static readonly DependencyProperty ShowCategoryProperty = DependencyProperty.Register(
    nameof(ShowCategory),
    typeof(bool),
    typeof(SampleRow),
    new PropertyMetadata(defaultValue: true));

    public bool ShowCategory
    {
        get => (bool)GetValue(ShowCategoryProperty);
        set => SetValue(ShowCategoryProperty, value);
    }

    public static readonly DependencyProperty CategoryImageUrlProperty = DependencyProperty.Register(
      nameof(CategoryImageUrl),
      typeof(Uri),
      typeof(SampleRow),
      new PropertyMetadata(defaultValue: null));

    public Uri CategoryImageUrl
    {
        get => (Uri)GetValue(CategoryImageUrlProperty);
        set => SetValue(CategoryImageUrlProperty, value);
    }

    public static readonly DependencyProperty CategoryHeaderProperty = DependencyProperty.Register(nameof(CategoryHeader), typeof(string), typeof(SampleRow), new PropertyMetadata(defaultValue: null));

    public string CategoryHeader
    {
        get => (string)GetValue(CategoryHeaderProperty);
        set => SetValue(CategoryHeaderProperty, value);
    }

    public static readonly DependencyProperty CategoryDescriptionProperty = DependencyProperty.Register(nameof(CategoryDescription), typeof(string), typeof(SampleRow), new PropertyMetadata(defaultValue: null));

    public string CategoryDescription
    {
        get => (string)GetValue(CategoryDescriptionProperty);
        set => SetValue(CategoryDescriptionProperty, value);
    }

    public static readonly DependencyProperty SampleCardsProperty = DependencyProperty.Register(nameof(SampleCards), typeof(ObservableCollection<RowSample>), typeof(SampleRow), new PropertyMetadata(null));

    public ObservableCollection<RowSample> SampleCards
    {
        get => (ObservableCollection<RowSample>)GetValue(SampleCardsProperty);
        set => SetValue(SampleCardsProperty, value);
    }

    public SampleRow()
    {
        this.InitializeComponent();
        SampleCards = [];
    }

    private void OnExplore(object sender, RoutedEventArgs e)
    {
        EnsureNavigationSelection(typeof(QuranPage));
    }
    private void OnSearch(object sender, RoutedEventArgs e)
    {
        EnsureNavigationSelection(typeof(SearchPage));
    }
}

internal class RowSample
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public IconElement? Icon { get; set; }
    public string? Id { get; set; }
}
