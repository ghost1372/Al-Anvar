using Microsoft.UI.Xaml.Printing;
using Windows.Graphics.Printing;

namespace AlAnvar.Common;
public class PrintHelper
{
    public event Action OnPrintSucceeded;
    public event Action OnPrintFailed;
    public event Action OnPrintCanceled;

    /// <summary>
    /// The percent of app's margin width, content is set at 85% (0.85) of the area's width
    /// </summary>
    protected double ApplicationContentMarginLeft = 0.075;

    /// <summary>
    /// The percent of app's margin height, content is set at 94% (0.94) of tha area's height
    /// </summary>
    protected double ApplicationContentMarginTop = 0.03;

    /// <summary>
    /// PrintDocument is used to prepare the pages for printing.
    /// Prepare the pages to print in the handlers for the Paginate, GetPreviewPage, and AddPages events.
    /// </summary>
    protected PrintDocument printDocument;

    /// <summary>
    /// Marker interface for document source
    /// </summary>
    protected IPrintDocumentSource printDocumentSource;

    /// <summary>
    /// A list of UIElements used to store the print preview pages.  This gives easy access
    /// to any desired preview page.
    /// </summary>
    internal List<UIElement> printPreviewPages;

    // Event callback which is called after print preview pages are generated.  Photos scenario uses this to do filtering of preview pages
    protected event EventHandler PreviewPagesCreated;

    /// <summary>
    /// First page in the printing-content series
    /// From this "virtual sized" paged content is split(text is flowing) to "printing pages"
    /// </summary>
    protected FrameworkElement firstPage;

    /// <summary>
    ///  A reference back to the scenario page used to access XAML elements on the scenario page
    /// </summary>
    protected FrameworkElement scenarioElement;

    /// <summary>
    ///  A hidden canvas used to hold pages we wish to print
    /// </summary>
    protected Canvas PrintCanvas
    {
        get
        {
            return scenarioElement.FindName("PrintCanvas") as Canvas;
        }
    }

    private IntPtr hWnd;
    private string printTaskName;
    private string footer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="scenarioElement">The scenario page constructing us</param>
    public PrintHelper(IntPtr hWnd, FrameworkElement scenarioElement, string footer, string printTaskName)
    {
        this.hWnd = hWnd;
        this.footer = footer;
        this.printTaskName = printTaskName;
        this.scenarioElement = scenarioElement;
        printPreviewPages = new List<UIElement>();
    }

    /// <summary>
    /// This function registers the app for printing with Windows and sets up the necessary event handlers for the print process.
    /// </summary>
    public virtual void RegisterForPrinting()
    {
        printDocument = new PrintDocument();
        printDocumentSource = printDocument.DocumentSource;
        printDocument.Paginate += CreatePrintPreviewPages;
        printDocument.GetPreviewPage += GetPrintPreviewPage;
        printDocument.AddPages += AddPrintPages;

        PrintManager printMan = PrintManagerInterop.GetForWindow(hWnd);
        printMan.PrintTaskRequested += PrintTaskRequested;
    }

    /// <summary>
    /// This function unregisters the app for printing with Windows.
    /// </summary>
    public virtual void UnregisterForPrinting()
    {
        if (printDocument == null)
        {
            return;
        }

        printDocument.Paginate -= CreatePrintPreviewPages;
        printDocument.GetPreviewPage -= GetPrintPreviewPage;
        printDocument.AddPages -= AddPrintPages;
        printDocument = null;

        // Remove the handler for printing initialization.
        PrintManager printMan = PrintManagerInterop.GetForWindow(hWnd);
        printMan.PrintTaskRequested -= PrintTaskRequested;

        PrintCanvas.Children.Clear();
    }

    public async Task ShowPrintUIAsync()
    {
        await PrintManagerInterop.ShowPrintUIForWindowAsync(hWnd);
    }

    /// <summary>
    /// Method that will generate print content for the scenario
    /// For scenarios 1-4: it will create the first page from which content will flow
    /// Scenario 5 uses a different approach
    /// </summary>
    /// <param name="page">The page to print</param>
    public virtual void PreparePrintContent(Page page)
    {
        if (firstPage == null)
        {
            firstPage = page;
            StackPanel header = (StackPanel) firstPage.FindName("Header");
            header.Visibility = Visibility.Visible;
        }

        // Add the (newly created) page to the print canvas which is part of the visual tree and force it to go
        // through layout so that the linked containers correctly distribute the content inside them.
        PrintCanvas.Children.Add(firstPage);
        PrintCanvas.InvalidateMeasure();
        PrintCanvas.UpdateLayout();
    }

    /// <summary>
    /// This is the event handler for PrintManager.PrintTaskRequested.
    /// </summary>
    /// <param name="sender">PrintManager</param>
    /// <param name="e">PrintTaskRequestedEventArgs </param>
    protected virtual void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
    {
        PrintTask printTask = null;
        printTask = e.Request.CreatePrintTask(printTaskName, sourceRequested =>
        {
            // Print Task event handler is invoked when the print job is completed.
            printTask.Completed += (s, args) =>
            {
                scenarioElement.DispatcherQueue.TryEnqueue(() =>
                {
                    switch (args.Completion)
                    {
                        case PrintTaskCompletion.Failed:
                            OnPrintFailed?.Invoke();
                            break;

                        case PrintTaskCompletion.Canceled:
                            OnPrintCanceled?.Invoke();
                            break;

                        case PrintTaskCompletion.Submitted:
                            OnPrintSucceeded?.Invoke();
                            break;
                    }
                });
            };

            sourceRequested.SetSource(printDocumentSource);
        });
    }

    /// <summary>
    /// This is the event handler for PrintDocument.Paginate. It creates print preview pages for the app.
    /// </summary>
    /// <param name="sender">PrintDocument</param>
    /// <param name="e">Paginate Event Arguments</param>
    protected virtual void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
    {
        lock (printPreviewPages)
        {
            // Clear the cache of preview pages
            printPreviewPages.Clear();

            // Clear the print canvas of preview pages
            PrintCanvas.Children.Clear();

            // This variable keeps track of the last RichTextBlockOverflow element that was added to a page which will be printed
            RichTextBlockOverflow lastRTBOOnPage;

            // Get the PrintTaskOptions
            PrintTaskOptions printingOptions = ((PrintTaskOptions) e.PrintTaskOptions);

            // Get the page description to deterimine how big the page is
            PrintPageDescription pageDescription = printingOptions.GetPageDescription(0);

            // We know there is at least one page to be printed. passing null as the first parameter to
            // AddOnePrintPreviewPage tells the function to add the first page.
            lastRTBOOnPage = AddOnePrintPreviewPage(null, pageDescription);

            // We know there are more pages to be added as long as the last RichTextBoxOverflow added to a print preview
            // page has extra content
            while (lastRTBOOnPage.HasOverflowContent && lastRTBOOnPage.Visibility == Visibility.Visible)
            {
                lastRTBOOnPage = AddOnePrintPreviewPage(lastRTBOOnPage, pageDescription);
            }

            if (PreviewPagesCreated != null)
            {
                PreviewPagesCreated.Invoke(printPreviewPages, null);
            }

            PrintDocument printDoc = (PrintDocument) sender;

            // Report the number of preview pages created
            printDoc.SetPreviewPageCount(printPreviewPages.Count, PreviewPageCountType.Intermediate);
        }
    }

    /// <summary>
    /// This is the event handler for PrintDocument.GetPrintPreviewPage. It provides a specific print preview page,
    /// in the form of an UIElement, to an instance of PrintDocument. PrintDocument subsequently converts the UIElement
    /// into a page that the Windows print system can deal with.
    /// </summary>
    /// <param name="sender">PrintDocument</param>
    /// <param name="e">Arguments containing the preview requested page</param>
    protected virtual void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
    {
        PrintDocument printDoc = (PrintDocument) sender;
        printDoc.SetPreviewPage(e.PageNumber, printPreviewPages[e.PageNumber - 1]);
    }

    /// <summary>
    /// This is the event handler for PrintDocument.AddPages. It provides all pages to be printed, in the form of
    /// UIElements, to an instance of PrintDocument. PrintDocument subsequently converts the UIElements
    /// into a pages that the Windows print system can deal with.
    /// </summary>
    /// <param name="sender">PrintDocument</param>
    /// <param name="e">Add page event arguments containing a print task options reference</param>
    protected virtual void AddPrintPages(object sender, AddPagesEventArgs e)
    {
        // Loop over all of the preview pages and add each one to  add each page to be printied
        for (int i = 0; i < printPreviewPages.Count; i++)
        {
            // We should have all pages ready at this point...
            printDocument.AddPage(printPreviewPages[i]);
        }

        PrintDocument printDoc = (PrintDocument) sender;

        // Indicate that all of the print pages have been provided
        printDoc.AddPagesComplete();
    }

    /// <summary>
    /// This function creates and adds one print preview page to the internal cache of print preview
    /// pages stored in printPreviewPages.
    /// </summary>
    /// <param name="lastRTBOAdded">Last RichTextBlockOverflow element added in the current content</param>
    /// <param name="printPageDescription">Printer's page description</param>
    protected virtual RichTextBlockOverflow AddOnePrintPreviewPage(RichTextBlockOverflow lastRTBOAdded, PrintPageDescription printPageDescription)
    {
        // XAML element that is used to represent to "printing page"
        FrameworkElement page;

        // The link container for text overflowing in this page
        RichTextBlockOverflow textLink;

        // Check if this is the first page ( no previous RichTextBlockOverflow)
        if (lastRTBOAdded == null)
        {
            // If this is the first page add the specific scenario content
            page = firstPage;

            // Hide footer since we don't know yet if it will be displayed (this might not be the last page) - wait for layout
            StackPanel footer = (StackPanel) page.FindName("Footer");
            footer.Visibility = Visibility.Collapsed;
        }
        else
        {
            // Flow content (text) from previous pages
            page = new ContinuationPage(lastRTBOAdded, footer);
        }

        // Set "paper" width
        page.Width = printPageDescription.PageSize.Width;
        page.Height = printPageDescription.PageSize.Height;

        Grid printableArea = (Grid) page.FindName("PrintableArea");

        // Get the margins size
        // If the ImageableRect is smaller than the app provided margins use the ImageableRect
        double marginWidth = Math.Max(printPageDescription.PageSize.Width - printPageDescription.ImageableRect.Width, printPageDescription.PageSize.Width * ApplicationContentMarginLeft * 2);
        double marginHeight = Math.Max(printPageDescription.PageSize.Height - printPageDescription.ImageableRect.Height, printPageDescription.PageSize.Height * ApplicationContentMarginTop * 2);

        // Set-up "printable area" on the "paper"
        printableArea.Width = firstPage.Width - marginWidth;
        printableArea.Height = firstPage.Height - marginHeight;

        // Add the (newley created) page to the print canvas which is part of the visual tree and force it to go
        // through layout so that the linked containers correctly distribute the content inside them.
        PrintCanvas.Children.Add(page);
        PrintCanvas.InvalidateMeasure();
        PrintCanvas.UpdateLayout();

        // Find the last text container and see if the content is overflowing
        textLink = (RichTextBlockOverflow) page.FindName("ContinuationPageLinkedContainer");

        // Check if this is the last page
        if (!textLink.HasOverflowContent && textLink.Visibility == Visibility.Visible)
        {
            StackPanel footer = (StackPanel) page.FindName("Footer");
            footer.Visibility = Visibility.Visible;
            PrintCanvas.UpdateLayout();
        }

        // Add the page to the page preview collection
        printPreviewPages.Add(page);

        return textLink;
    }
}
