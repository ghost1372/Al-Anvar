using System.ComponentModel;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AlAnvar.Models;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings.Modulation.Recovery;
using Windows.ApplicationModel.DataTransfer;

namespace AlAnvar.Common;
public static partial class AppHelper
{
    [System.Diagnostics.CodeAnalysis.DynamicDependency(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All, typeof(AppConfig))]
    public static AppConfig Settings = JsonSettings.Configure<AppConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow();
    public async static Task<bool> EnsureDatabaseExistsAsync()
    {
        if (!File.Exists(Settings.DBPath))
        {
            var contentDialog = new WindowedContentDialog()
            {
                Header = Strings.AppHelper_DatabaseDialogTitle.GetLocalizedResource(),
                Content = Strings.AppHelper_DatabaseDialogContent.GetLocalizedResource(),
                CloseButtonContent = Strings.AppHelper_DatabaseDialogPrimaryButtonText.GetLocalizedResource(),
                SecondaryButtonContent = Strings.AppHelper_DatabaseDialogSecondaryButtonText.GetLocalizedResource(),
                PrimaryButtonContent = Strings.AppHelper_DatabaseDialogPrimaryButtonText.GetLocalizedResource(),
                FlowDirection = GeneralHelper.GetEnum<FlowDirection>(Strings.Main_FlowDirection_FlowDirection.GetLocalizedResource()),
                DefaultButton = ContentDialogButton.Close,
                Owner = App.MainWindow,
            };
            contentDialog.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
            contentDialog.DefaultButton = ContentDialogButton.Primary;

            contentDialog.PrimaryButtonClick -= OnGoToSettings;
            contentDialog.PrimaryButtonClick += OnGoToSettings;

            contentDialog.SecondaryButtonClick -= OnExit;
            contentDialog.SecondaryButtonClick += OnExit;

            void OnExit(object sender, EventArgs args)
            {
                Environment.Exit(0);
            }

            void OnGoToSettings(object sender, EventArgs args)
            {
                EnsureNavigationSelection(typeof(SettingsPage));
            }

            await contentDialog.ShowAsync();
            return false;
        }
        else
        {
            return true;
        }
    }

    public static async Task GoToTranslationsFolderInExplorerAsync()
    {
        if (Directory.Exists(Settings.TranslationsPath))
        {
            Windows.Storage.StorageFolder folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Settings.TranslationsPath);
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }
    }

    public static async Task GoToAudiosFolderInExplorerAsync()
    {
        if (Directory.Exists(Settings.AudiosPath))
        {
            Windows.Storage.StorageFolder folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Settings.AudiosPath);
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }
    }

    public static void EnsureNavigationSelection(Type page)
    {
        App.Current.NavService.EnsureNavigationSelection(page.FullName);
    }

    public static async Task<JsonTranslationFile> LoadTranslationFileAsync(string translationId)
    {
        var filePath = Path.Combine(Settings.TranslationsPath, $"{translationId}.json");
        if (!File.Exists(filePath)) return null;

        var text = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<JsonTranslationFile>(text, JsonTranslationFileSerializerOption.Default.JsonTranslationFile);
    }

    public static Visual InitTranslationAnimation(FrameworkElement element)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        ElementCompositionPreview.SetIsTranslationEnabled(element, true);
        visual.Opacity = 0;
        visual.Properties.InsertVector3("Translation", new Vector3(0, 200, 0));
        return visual;
    }

    public static void AnimateShowPanel(Visual visual, FrameworkElement element)
    {
        if (element == null)
            return;

        if (element.Visibility == Visibility.Visible)
            return;

        var compositor = visual.Compositor;

        // Cancel previous animations
        visual.StopAnimation("Opacity");
        visual.Properties.StopAnimation("Translation");

        // Ensure initial visual state
        visual.Opacity = 0;
        visual.Properties.InsertVector3("Translation", new Vector3(0, 200, 0));
        element.Visibility = Visibility.Visible;
        element.Opacity = 1;

        // Fade-in animation
        var fadeIn = compositor.CreateScalarKeyFrameAnimation();
        fadeIn.InsertKeyFrame(1f, 1f);
        fadeIn.Duration = TimeSpan.FromMilliseconds(400);
        visual.StartAnimation("Opacity", fadeIn);

        // Slide-in animation
        var slideIn = compositor.CreateVector3KeyFrameAnimation();
        slideIn.InsertKeyFrame(1f, Vector3.Zero);
        slideIn.Duration = TimeSpan.FromMilliseconds(600);
        visual.Properties.StartAnimation("Translation", slideIn);
    }
    public static void AnimateHidePanel(Visual visual, FrameworkElement element)
    {
        if (element == null)
            return;

        if (element.Visibility == Visibility.Collapsed)
            return;

        var compositor = visual.Compositor;

        visual.StopAnimation("Opacity");
        visual.Properties.StopAnimation("Translation");

        var fadeOut = compositor.CreateScalarKeyFrameAnimation();
        fadeOut.InsertKeyFrame(1f, 0f);
        fadeOut.Duration = TimeSpan.FromMilliseconds(350);

        var slideOut = compositor.CreateVector3KeyFrameAnimation();
        slideOut.InsertKeyFrame(1f, new Vector3(0, 200, 0));
        slideOut.Duration = TimeSpan.FromMilliseconds(600);

        var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
        batch.Completed += (s, e) =>
        {
            if (element != null)
            {
                element.Visibility = Visibility.Collapsed;
            }
        };

        visual.StartAnimation("Opacity", fadeOut);
        visual.Properties.StartAnimation("Translation", slideOut);

        batch.End();
    }

    public static void CopyToClipboard(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        var dataPackage = new DataPackage();
        var strBuilder = new StringBuilder();
        dataPackage.SetText(value.Trim());
        Clipboard.SetContent(dataPackage);
    }

    public static bool VerseContains(string verseIds, int target)
    {
        if (string.IsNullOrWhiteSpace(verseIds))
            return false;

        foreach (Match m in Regex.Matches(verseIds, @"\d+"))
        {
            if (int.Parse(m.Value) == target)
                return true;
        }

        return false;
    }

    public static List<int> ParseVerseIds(string verseIds)
    {
        return System.Text.RegularExpressions.Regex.Matches(verseIds, @"\d+").Select(m => int.Parse(m.Value)).ToList();
    }
}

