using Microsoft.UI.Xaml.Documents;

namespace AlAnvar.UI.Pages;

public sealed partial class PageToPrint : Page
{
    public RichTextBlock TextContentBlock { get; set; }

    public PageToPrint(string title, string[] subTitle, string footer, Paragraph paragraph)
    {
        this.InitializeComponent();
        SetTitle(title, subTitle);
        txtFooter.Text = footer;
        TextContent.Blocks.Add(paragraph);
        TextContentBlock = TextContent;
    }
    private void SetTitle(string title, string[] subTitle)
    {
        Run titleRun = new Run { Text = title };
        foreach (var item in subTitle)
        {
            Run subTitleRun = new Run { Text = item };
            txtSubTitle.Inlines.Add(subTitleRun);
            txtSubTitle.Inlines.Add(new LineBreak());
        }
        txtTitle.Inlines.Add(titleRun);
    }
}
