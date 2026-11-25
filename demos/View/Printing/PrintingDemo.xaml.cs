/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System;
using System.Globalization;
using System.Printing;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Demo.Option.DataBinding;
using yWorks.Demo.Option.Editor;
using yWorks.Demo.Option.Handler;
using yWorks.Demo.Option.I18N;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo.Printing
{
  /// <summary>
  ///   This demo shows how to use and customize printing functionality.
  /// </summary>
  /// <remarks>For more details, see the description file or run the application.</remarks>
  [Demo("printing", "view", "Printing", Summary = "Shows how to print the whole diagram or a part of it.",
      Tags = ["printing"], Keywords = ["posters", "vector graphics", "handles"])]
  public partial class PrintingDemo
  {
    #region private fields

    // Option handler for print options
    // region that gets printed
    private MutableRectangle exportRect;

    // printable representation of the graph control
    private CanvasPrintDocument printDocument;

    // dialog that holds printer and page settings used for printing
    private PrintDialog printDialog;

    #endregion

    #region Constructors

    /// <summary>
    ///   Instantiates class <see cref="PrintingDemo"/>
    /// </summary>
    public PrintingDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      InitializeInputModes();
      InitializeGraph();

      SetupOptions();
      InitializePrinting();
    }

    #endregion

    #region Properties

    private OptionHandler Handler { get; set; }

    #endregion

    #region Initialization

    private void InitializeInputModes() {
      // Create a GraphEditorInputMode instance
      var editMode = new GraphEditorInputMode();

      // and install the edit mode into the canvas.
      graphControl.InputMode = editMode;

      // create the model for the export rectangle
      exportRect = new MutableRectangle(0, 0, 100, 100);
      // visualize it
      var renderTree = graphControl.RenderTree;
      renderTree.CreateElement(renderTree.InputModeGroup, exportRect,
          new ExportRectangleRenderer((Pen) new Pen(Brushes.Gray, 4).GetAsFrozen()));

      renderTree.GetElements();
      AddExportRectInputModes(graphControl.InputModeContext, editMode);
    }

    /// <summary>
    ///   Adds the view modes that handle the resizing and movement of the export rectangle.
    /// </summary>
    /// <param name="graphControlInputModeContext"></param>
    /// <param name="inputMode"></param>
    private void AddExportRectInputModes(
        IInputModeContext graphControlInputModeContext,
        MultiplexingInputMode inputMode) {
      // create handles for interactively resizing the export rectangle
      var rectangleHandles = new RectangleReshapeHandleProvider(exportRect) { MinimumSize = new SizeD(1, 1) };

      // create a mode that deals with the handles
      var exportHandleInputMode = new HandleInputMode { Priority = 1 };

      // add it to the graph editor mode
      inputMode.Add(exportHandleInputMode);

      // now the handles
      var inputModeContext = graphControlInputModeContext;
      exportHandleInputMode.Handles = new ObservableCollection<IHandle> {
          rectangleHandles.GetHandle(inputModeContext, HandlePositions.TopRight),
          rectangleHandles.GetHandle(inputModeContext, HandlePositions.TopLeft),
          rectangleHandles.GetHandle(inputModeContext, HandlePositions.BottomRight),
          rectangleHandles.GetHandle(inputModeContext, HandlePositions.BottomLeft)
      };

      // create a mode that allows for dragging the export rectangle at the sides
      var moveInputMode = new MoveInputMode {
          PositionHandler = new ExportRectanglePositionHandler(exportRect),
          HitTestable = HitTestables.Create((context, location) => {
            var path = new GeneralPath(5);
            path.AppendRectangle(exportRect, false);
            return path.PathContains(location, context.HitTestRadius + 3 * context.Zoom);
          }),
          Priority = 41
      };

      // add it to the edit mode
      inputMode.Add(moveInputMode);
    }

    private void InitializeGraph() {
      var graph = graphControl.Graph;
      // initialize defaults
      DemoStyles.InitDemoStyles(graph);

      // create sample graph
      graph.AddLabel(graph.CreateNode(new RectD(15, 15, 50, 30)), "Node");
      var node = graph.CreateNode(new PointD(90, 30));
      graph.CreateEdge(node, graph.CreateNode(new PointD(90, 90)));

      graphControl.FitGraphBounds();
      // initially set the export rect to enclose part of the graph's contents
      exportRect.SetShape(graphControl.ContentBounds);

      graph.CreateEdge(node, graph.CreateNode(new PointD(200, 30)));

      graphControl.FitGraphBounds();
    }

    private void InitializePrinting() {
      printDialog = new PrintDialog {
          PageRangeSelection = PageRangeSelection.AllPages,
          UserPageRangeEnabled = false
      };

      // create new canvas print document
      printDocument = new CanvasPrintDocument { AddOverlay = true };
    }

    #endregion

    #region export option handler

    private void SetupOptions() {
      // create the options
      SetupHandler();
      // create the control to visualize them
      AddEditorControlToForm();
    }

    private void AddEditorControlToForm() {
      // add a new editor control for the option handler
      editorControl.OptionHandler = Handler;
    }


    /// <summary>
    ///   Initializes the option handler for the export
    /// </summary>
    private void SetupHandler() {
      Handler = new OptionHandler(PRINTING);

      var currentGroup = Handler.AddGroup(OUTPUT);
      currentGroup.AddBool(HIDE_DECORATIONS, true);
      currentGroup.AddBool(PRINT_RECTANGLE, true);

      currentGroup = Handler.AddGroup(DOCUMENT_SETTINGS);

      var item = currentGroup.AddDouble(SCALE, 1.0);
      currentGroup.AddBool(CENTER_CONTENT, false);
      currentGroup.AddBool(PAGE_MARK_PRINTING, false);
      currentGroup.AddBool(SCALE_DOWN_TO_FIT_PAGE, false);
      currentGroup.AddBool(SCALE_UP_TO_FIT_PAGE, false);

      // localization
      var rm = new ResourceManager("yWorks.Demo.Printing.Printing", Assembly.GetExecutingAssembly());
      var rmf = new ResourceManagerI18NFactory();
      rmf.AddResourceManager(Handler.Name, rm);
      Handler.I18nFactory = rmf;
    }

    #endregion

    #region eventhandlers

    private void printPreviewButton_Click(object sender, EventArgs e) {
      PreparePrinting();

      // show new PrintPreviewDialog
      printDocument.Print(printDialog, true, true, new InsetsD(50), PrintDialogTitle);
    }

    private void PreparePrinting() {
      var control = graphControl;
      // check if the rectangular region or the whole viewport should be printed
      var useRect = (bool) Handler.GetValue(OUTPUT, PRINT_RECTANGLE);
      var bounds = useRect ? exportRect.ToRectD() : graphControl.ContentBounds;

      // check whether decorations (selection, handles, ...) should be hidden
      var hide = (bool) Handler.GetValue(OUTPUT, HIDE_DECORATIONS);
      if (hide) {
        // if so, create a new graph control with the same graph
        control = new GraphControl {
            Graph = graphControl.Graph,
            FlowDirection = graphControl.FlowDirection,
            Projection = graphControl.Projection
        };
      }

      // read CanvasPrintDocument options
      printDocument.Scale = (double) Handler.GetValue(DOCUMENT_SETTINGS, SCALE);
      printDocument.CenterContent = (bool) Handler.GetValue(DOCUMENT_SETTINGS, CENTER_CONTENT);
      printDocument.PageMarkPrinting = (bool) Handler.GetValue(DOCUMENT_SETTINGS, PAGE_MARK_PRINTING);
      printDocument.ScaleDownToFitPage = (bool) Handler.GetValue(DOCUMENT_SETTINGS, SCALE_DOWN_TO_FIT_PAGE);
      printDocument.ScaleUpToFitPage = (bool) Handler.GetValue(DOCUMENT_SETTINGS, SCALE_UP_TO_FIT_PAGE);
      // set GraphControl
      // but first detach the graph from the old control if it was a temporary one
      if (printDocument.CanvasControl is GraphControl printDocumentGraphControl &&
          printDocumentGraphControl != graphControl) {
        printDocumentGraphControl.ClearValue(GraphControl.GraphProperty);
      }
      printDocument.CanvasControl = control;
      // set print area
      printDocument.PrintRectangle = bounds;
    }

    private void printerSetupButton_Click(object sender, EventArgs e) {
      // show new PrintDialog
      var showDialog = printDialog.ShowDialog();
      if (showDialog.HasValue && showDialog.Value) {
        PreparePrinting();
        printDocument.Print(printDialog, true, false, new InsetsD(50), PrintDialogTitle);
      }
    }

    private void pageSetupButton_Click(object sender, EventArgs e) {
      var optionHandler = new OptionHandler("PageSettings");

      // We use only a subset of the available settings, since we want to filter some values
      // ("Unknown" can be returned by some properties, but may not be set on a PrintTicket)
      var selectionProvider = new DefaultSelectionProvider<PrintTicketOptionsHelper>([
          new PrintTicketOptionsHelper(printDialog.PrintTicket)
      ]);
      selectionProvider.ContextLookupDelegate = OptionHandlerContextLookup;
      selectionProvider.UpdatePropertyViewsNow();
      // We populate the OptionHandler
      optionHandler.BuildFromSelection(selectionProvider, OptionHandlerContextLookup);

      var form = new EditorForm {
          OptionHandler = optionHandler,
          IsAutoAdopt = true,
          IsAutoCommit = false,
          Title = "Page Setup"
      };
      form.ShowDialog();
    }

    private object OptionHandlerContextLookup(object subject, Type type) {
      if (type == typeof(IOptionBuilder) && (subject is PrintTicketOptionsHelper || subject is PrintTicket)) {
        return new AttributeBasedOptionBuilder();
      }
      if (type == typeof(IPropertyMapBuilder) && (subject is PrintTicketOptionsHelper || subject is PrintTicket)) {
        return new AttributeBasedPropertyMapBuilderAttribute().CreateBuilder(subject.GetType());
      }
      return null;
    }

    /// <summary>
    ///   Helper class that serves as a proxy for the most important settings of a print ticket
    /// </summary>
    public class PrintTicketOptionsHelper
    {
      private readonly PrintTicket ticket;

      public PrintTicketOptionsHelper(PrintTicket ticket) {
        this.ticket = ticket;
      }

      public int? CopyCount {
        get => ticket.CopyCount;
        set => ticket.CopyCount = value;
      }

      [OptionItemAttribute(Name = OptionItem.CustomDialogitemEditor, Value = "ConverterBased.OptionItemPresenter")]
      [OptionItemAttribute(Name = OptionItem.CustomValueConverterAttribute, Value = typeof(PageMediaSizeConverter))]
      public PageMediaSize PageMediaSize {
        get => ticket.PageMediaSize;
        set => ticket.PageMediaSize = value;
      }

      public class PageMediaSizeConverter : IValueConverter
      {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
          var sz = (PageMediaSize) value;
          var pageMediaSizeName = sz.PageMediaSizeName;
          if (pageMediaSizeName.HasValue) {
            return pageMediaSizeName.Value;
          }
          return sz.Height.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) + "x" +
              sz.Width.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
          var strValue = value as string;
          PageMediaSize retval = null;
          if (strValue != null) {
            try {
              var pageMediaSizeName = (PageMediaSizeName) Enum.Parse(typeof(PageMediaSizeName), strValue);
              retval = new PageMediaSize(pageMediaSizeName);
            } catch (Exception) { }
            if (retval == null) {
              var tokens = strValue.Split('x');
              if (tokens.Length == 2) {
                try {
                  var height = double.Parse(tokens[0], CultureInfo.InvariantCulture);
                  var width = double.Parse(tokens[1], CultureInfo.InvariantCulture);
                  retval = new PageMediaSize(width, height);
                } catch (Exception) { }
              }
            }
          }
          return retval ?? DependencyProperty.UnsetValue;
        }
      }

      public MyPageOrder PageOrder {
        get {
          var pageOrder = ticket.PageOrder;
          if (pageOrder.HasValue && pageOrder != System.Printing.PageOrder.Unknown) {
            return (MyPageOrder) pageOrder;
          }
          return MyPageOrder.Unspecified;
        }
        set {
          if (value == MyPageOrder.Unspecified) {
            ticket.PageOrder = null;
          } else {
            ticket.PageOrder = (PageOrder) value;
          }
        }
      }

      public MyPageOrientation PageOrientation {
        get {
          var pageOrientation = ticket.PageOrientation;
          if (pageOrientation.HasValue && pageOrientation != System.Printing.PageOrientation.Unknown) {
            return (MyPageOrientation) pageOrientation;
          }
          return MyPageOrientation.Unspecified;
        }
        set {
          if (value == MyPageOrientation.Unspecified) {
            ticket.PageOrientation = null;
          } else {
            ticket.PageOrientation = (PageOrientation) value;
          }
        }
      }

      public int? PagesPerSheet {
        get => ticket.PagesPerSheet;
        set => ticket.PagesPerSheet = value;
      }

      /// <summary>
      ///   Helper enum that filters out the "Unknown" value which may not be set interactively, and filters the NullableValues
      ///   from the original enum...
      /// </summary>
      public enum MyPageOrientation
      {
        Unspecified,
        Landscape = System.Printing.PageOrientation.Landscape,
        Portrait = System.Printing.PageOrientation.Portrait,
        ReverseLandscape = System.Printing.PageOrientation.ReverseLandscape,
        ReversePortrait = System.Printing.PageOrientation.ReversePortrait
      }

      /// <summary>
      ///   Helper enum that filters out the "Unknown" value which may not be set interactively, and filters the NullableValues
      ///   from the original enum...
      /// </summary>
      public enum MyPageOrder
      {
        Unspecified,
        Standard = System.Printing.PageOrder.Standard,
        Reverse = System.Printing.PageOrder.Reverse
      }
    }

    #endregion

    #region static members

    private const string PRINTING = "PRINTING";

    private const string OUTPUT = "OUTPUT";
    private const string HIDE_DECORATIONS = "HIDE_DECORATIONS";
    private const string PRINT_RECTANGLE = "PRINT_RECTANGLE";

    private const string DOCUMENT_SETTINGS = "DOCUMENT_SETTINGS";
    private const string SCALE = "SCALE";
    private const string CENTER_CONTENT = "CENTER_CONTENT";
    private const string PAGE_MARK_PRINTING = "PAGE_MARK_PRINTING";
    private const string SCALE_DOWN_TO_FIT_PAGE = "SCALE_DOWN_TO_FIT_PAGE";
    private const string SCALE_UP_TO_FIT_PAGE = "SCALE_UP_TO_FIT_PAGE";
    private const string PrintDialogTitle = "Printing yFiles Example";

    #endregion

    /// <summary>
    ///   Helper method that allows for reusing this demo's graph in other applications.
    /// </summary>
    /// <param name="graphControl">The graph control.</param>
    public void ShowGraph(GraphControl graphControl) {
      this.graphControl.Graph = graphControl.Graph;
      this.graphControl.Selection.Clear(); // or possibly: = graphControl.Selection;
      this.graphControl.ContentBounds = graphControl.ContentBounds.GetEnlarged(20);

      // show all of the contents
      this.graphControl.FitContent();

      // or possibly the same viewport
      //      this.graphControl.Zoom = graphControl.Zoom;
      //      this.graphControl.ViewPoint = graphControl.ViewPoint;

      var inputMode = new MultiplexingInputMode();

      // set the whole content bounds as the export rectangle
      exportRect.SetShape(graphControl.ContentBounds);
      // or possibly just the visible viewport
      //exportRect.Set(graphControl.Viewport);

      AddExportRectInputModes(this.graphControl.InputModeContext, inputMode);
      this.graphControl.InputMode = inputMode;
    }

    [STAThread]
    private static void Main() {
      Run<PrintingDemo>();
    }
  }
}
