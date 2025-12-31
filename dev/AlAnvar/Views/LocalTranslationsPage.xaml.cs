using System.Diagnostics;
using AlAnvar.Models;
using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class LocalTranslationsPage : Page
{
    public TranslationsViewModel ViewModel { get; }
    public static LocalTranslationsPage Instance { get; private set; }

    public LocalTranslationsPage()
    {
        ViewModel = App.GetService<TranslationsViewModel>();
        InitializeComponent();
        Instance = this;

        TranslationsTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, ViewModel.Filter));

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.GetAvailableLocalTranslations();
    }

    private async void TranslationsTableView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TranslationsTableView.SelectedItems.Count > 0)
        {
            BtnDeleteSelection.IsEnabled = true;
        }
        else
        {
            BtnDeleteSelection.IsEnabled = false;
        }

        if (TranslationsTableView.SelectedItems.Count == 1)
        {
            BtnOpenFolder.IsEnabled = true;
        }
        else
        {
            BtnOpenFolder.IsEnabled = false;
        }
    }

    private async void OnDeleteSelection(object sender, RoutedEventArgs e)
    {
        var items = TranslationsTableView.SelectedItems.OfType<TranslationItem>().ToList();
        if (items != null && items.Count > 0)
        {
            var result = await MessageBox.ShowWarningAsync(Strings.LocalTranslationsPage_MessageBoxConfirmDelete.GetLocalizedResource(), MessageBoxButtons.YesNo);
            if (result == MessageBoxResult.YES)
            {
                await ViewModel.DeleteSelections(items);
                await ViewModel.GetAvailableLocalTranslations();
            }
        }
    }
    private async void OnOpenFolder(object sender, RoutedEventArgs e)
    {
        if (TranslationsTableView.SelectedItem is TranslationItem item)
        {
            var filePath = Path.Combine(Settings.TranslationsPath, item.Id + ".json");
            if (File.Exists(filePath))
            {
                Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
            else
            {
                await MessageBox.ShowErrorAsync(Strings.LocalTranslationsPage_MessageBoxFileNotFound.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
            }
        }
    }

    public TableView GetTableView()
    {
        return TranslationsTableView;
    }
}
