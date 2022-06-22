using Microsoft.UI.Xaml.Documents;

namespace AlAnvar.UI.Pages;

/// <summary>
/// A paged used to flow text from a given text container
/// </summary>
public sealed partial class ContinuationPage : Page
{
    /// <summary>
    /// Creates a continuation page and links text-flow to a text flow container
    /// </summary>
    /// <param name="textLinkContainer">Text link container which will flow text into this page</param>
    public ContinuationPage(RichTextBlockOverflow textLinkContainer, string footer)
    {
        InitializeComponent();
        Run run = new Run();
        run.Text = footer;
        txtFooter.Inlines.Add(run);
        textLinkContainer.OverflowContentTarget = ContinuationPageLinkedContainer;
    }
}
