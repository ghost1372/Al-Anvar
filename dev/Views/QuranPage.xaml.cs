﻿namespace AlAnvar.Views;

public sealed partial class QuranPage : Page
{
    public static QuranPage Instance { get; set; }
    public QuranViewModel ViewModel { get; }
    public QuranPage()
    {
        ViewModel = App.GetService<QuranViewModel>();
        this.InitializeComponent();
        Instance = this;
    }

    public QuranTabViewItem GetTabViewItem()
    {
        return tabView.SelectedItem as QuranTabViewItem;
    }

    private void tabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (tabView.TabItems.Count > 0)
        {
            LogoImage.Visibility = Visibility.Collapsed;
        }
        else
        {
            LogoImage.Visibility = Visibility.Visible;
        }
    }
}
