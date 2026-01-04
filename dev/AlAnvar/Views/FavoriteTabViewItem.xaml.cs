using System.Collections.ObjectModel;
using System.Text;
using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class FavoriteTabViewItem : TabViewItem
{
    public ObservableCollection<QuranMetadataTable> Metadata
    {
        get { return (ObservableCollection<QuranMetadataTable>)GetValue(MetadataProperty); }
        set { SetValue(MetadataProperty, value); }
    }

    public static readonly DependencyProperty MetadataProperty =
        DependencyProperty.Register(nameof(Metadata), typeof(ObservableCollection<QuranMetadataTable>), typeof(FavoriteTabViewItem), new PropertyMetadata(null));

    public QuranViewModel QuranViewModel
    {
        get { return (QuranViewModel)GetValue(QuranViewModelProperty); }
        set { SetValue(QuranViewModelProperty, value); }
    }

    public static readonly DependencyProperty QuranViewModelProperty =
        DependencyProperty.Register(nameof(QuranViewModel), typeof(QuranViewModel), typeof(FavoriteTabViewItem), new PropertyMetadata(null));

    public FavoriteTabViewItemViewModel ViewModel { get; }
    public FavoriteTabViewItem()
    {
        ViewModel = App.GetService<FavoriteTabViewItemViewModel>();
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        FavoriteTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, QuranViewModel.Filter));

        await ViewModel.OnPageLoaded(Metadata);
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null && button.DataContext is FinalQuran finalQuran)
        {
            var dialogResult = await MessageBox.ShowWarningAsync(Strings.FavoriteTabItem_MessageBoxConfirmDeleteMessage.GetLocalizedResource(), Strings.FavoriteTabItem_MessageBoxConfirmDeleteTitle.GetLocalizedResource(), MessageBoxButtons.YesNo);
            if (dialogResult == MessageBoxResult.YES)
            {
                using var db = new AlAnvarDBContext();
                var result = await Queries.GetFavoriteByIdsQueryAsync(db, finalQuran.SuraId, finalQuran.AyaId).FirstOrDefaultAsync();
                if (result != null)
                {
                    db.Favorites.Remove(result);
                    await db.SaveChangesAsync();
                }

                FavoriteTableView.CollectionView.Remove(button.DataContext);
            }
        }
    }

    private void BtnGoToTab_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null && button.DataContext is FinalQuran finalQuran)
        {
            QuranPage.Instance.GoToVerse(finalQuran.SuraId, finalQuran.AyaId);
        }
    }
    private void BtnCopy_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null && button.DataContext is FinalQuran finalQuran)
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(finalQuran.Aya);
            if (!string.IsNullOrEmpty(finalQuran.Translation))
            {
                strBuilder.AppendLine(finalQuran.Translation);
            }

            CopyToClipboard(strBuilder.ToString());
        }
    }

    public async void Refresh()
    {
        await ViewModel.OnPageLoaded(Metadata);
    }

    public TableView GetTableView()
    {
        return FavoriteTableView;
    }

    public async void OnMenuClicked(object sender, RoutedEventArgs e)
    {
        var menu = sender as MenuFlyoutItem;
        if (menu != null && menu.Tag != null && menu.DataContext is FinalQuran finalQuran)
        {
            switch (menu.Tag.ToString())
            {
                case "CopyVerse":
                    CopyToClipboard(finalQuran.Aya);
                    break;
                case "CopyTranslation":
                    CopyToClipboard(finalQuran.Translation);
                    break;
                case "CopyAll":
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(finalQuran.Aya);
                    stringBuilder.AppendLine(finalQuran.Translation);

                    CopyToClipboard(stringBuilder.ToString());
                    break;
                case "GoToSura":
                    if (menu.CommandParameter is Button goToTabButton)
                    {
                        BtnGoToTab_Click(goToTabButton, null);
                    }
                    break;
                case "Delete":
                    if (menu.CommandParameter is Button deleteButton)
                    {
                        BtnDelete_Click(deleteButton, null);
                    }
                    break;
            }
        }
    }
}
