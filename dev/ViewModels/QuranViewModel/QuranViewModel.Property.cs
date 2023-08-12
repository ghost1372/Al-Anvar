﻿using System.Collections.ObjectModel;

namespace AlAnvar.ViewModels;
public partial class QuranViewModel : ObservableRecipient
{
    [ObservableProperty]
    public ObservableCollection<ChapterProperty> chapters;

    [ObservableProperty]
    public AdvancedCollectionView chaptersACV;

    [ObservableProperty]
    public ObservableCollection<QuranTranslation> translationsCollection = new();

    [ObservableProperty]
    public QuranTranslation currentTranslation = Settings.QuranTranslation;

    [ObservableProperty]
    public int translationIndex = -1;

    [ObservableProperty]
    public ObservableCollection<QuranAudio> qarisCollection = new();

    [ObservableProperty]
    public QuranAudio currentQari = Settings.QuranAudio;

    [ObservableProperty]
    public int qariIndex = -1;

    [ObservableProperty]
    public int listViewSelectedIndex;

    [ObservableProperty]
    public object listViewSelectedItem;

    [ObservableProperty]
    public bool isTranslationActive = true;

    [ObservableProperty]
    public bool isOriginalTextActive = true;

    [ObservableProperty]
    public SolidColorBrush ayatNumberForeground;

    [ObservableProperty]
    public FontFamily ayatNumberFontFamily;

    [ObservableProperty]
    public double ayatNumberFontSize;

    [ObservableProperty]
    public SolidColorBrush ayatForeground;

    [ObservableProperty]
    public FontFamily ayatFontFamily;

    [ObservableProperty]
    public double ayatFontSize;

    [ObservableProperty]
    public SolidColorBrush translationForeground;

    [ObservableProperty]
    public FontFamily translationFontFamily;

    [ObservableProperty]
    public double translationFontSize;

    [ObservableProperty]
    public string statusText;

    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private SortDescription currentSortDescription;

    private List<string> suggestListForSurahSearch = new List<string>();

    private TabView tabview;
}