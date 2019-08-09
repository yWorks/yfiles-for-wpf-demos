/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Editor;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.I18N;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace Demo.yFiles.Printing
{
  /// <summary>
  /// This demo shows how to use and customize printing functionality.
  /// </summary>
  /// <remarks>For more details, see the description file or run the application.</remarks>
  public partial class PrintingWindow
  {
    #region private fields

    // Option handler for print options
    private OptionHandler handler;
    // region that gets printed
    private MutableRectangle exportRect;
    // printable representation of the graph control
    private CanvasPrintDocument printDocument;
    // dialog that holds printer and page settings used for printing
    private PrintDialog printDialog;
    #endregion

    #region Constructors

    /// <summary>
    /// Instantiates class <see cref="PrintingWindow" />
    /// </summary>
    public PrintingWindow() {
      InitializeComponent();
    }

    private void OnLoaded(object source, EventArgs args){
      InitializeInputModes();
      InitializeGraph();

      SetupOptions();
      InitializePrinting();
    }

    #endregion

    #region Properties

    private OptionHandler Handler {
      get { return handler; }
    }

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
      new RectangleIndicatorInstaller(exportRect, RectangleIndicatorInstaller.SelectionTemplateKey)
        .AddCanvasObject(graphControl.CanvasContext, graphControl.BackgroundGroup, exportRect);

      AddExportRectInputModes(editMode);
    }

    /// <summary>
    /// Adds the view modes that handle the resizing and movement of the export rectangle.
    /// </summary>
    /// <param name="inputMode"></param>
    private void AddExportRectInputModes(MultiplexingInputMode inputMode){
      // create handles for interactively resizing the export rectangle
      var rectangleHandles = new RectangleReshapeHandleProvider(exportRect) {MinimumSize = new SizeD(1, 1)};

      // create a mode that deals with the handles
      var exportHandleInputMode = new HandleInputMode{Priority = 1};

      // add it to the graph editor mode
      inputMode.Add(exportHandleInputMode);

      // now the handles
      var inputModeContext = Contexts.CreateInputModeContext(exportHandleInputMode);
      exportHandleInputMode.Handles = new DefaultObservableCollection<IHandle>
                                        {
                                          rectangleHandles.GetHandle(inputModeContext, HandlePositions.NorthEast),
                                          rectangleHandles.GetHandle(inputModeContext, HandlePositions.NorthWest),
                                          rectangleHandles.GetHandle(inputModeContext, HandlePositions.SouthEast),
                                          rectangleHandles.GetHandle(inputModeContext, HandlePositions.SouthWest),
                                        };

      // create a mode that allows for dragging the export rectangle at the sides
      var moveInputMode = new MoveInputMode
                            {
                              PositionHandler = new ExportRectanglePositionHandler(exportRect),
                              HitTestable = HitTestables.Create(
                                (context, location) => {
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
      IGraph graph = graphControl.Graph;
      // initialize defaults
      graph.NodeDefaults.Style = new ShinyPlateNodeStyle { Brush = Brushes.DarkOrange };
      graph.EdgeDefaults.Style = new PolylineEdgeStyle {TargetArrow = Arrows.Default};

      // create sample graph
      graph.AddLabel(graph.CreateNode(new PointD(30, 30)), "Node");
      INode node = graph.CreateNode(new PointD(90, 30));
      graph.CreateEdge(node, graph.CreateNode(new PointD(90, 90)));

      graphControl.FitGraphBounds();
      // initially set the export rect to enclose part of the graph's contents
      exportRect.Reshape(graphControl.ContentRect);

      graph.CreateEdge(node, graph.CreateNode(new PointD(200, 30)));

      graphControl.FitGraphBounds();

    }

    private void InitializePrinting() {
      printDialog = new PrintDialog()
                      {PageRangeSelection = PageRangeSelection.AllPages, UserPageRangeEnabled = false};

      // create new canvas print document
      printDocument = new CanvasPrintDocument(){AddOverlay = true};
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
    /// Initializes the option handler for the export
    /// </summary>
    private void SetupHandler() {
      handler = new OptionHandler(PRINTING);

      OptionGroup currentGroup = handler.AddGroup(OUTPUT);
      currentGroup.AddBool(HIDE_DECORATIONS, true);
      currentGroup.AddBool(PRINT_RECTANGLE, true);

      currentGroup = handler.AddGroup(DOCUMENT_SETTINGS);

      var item = currentGroup.AddDouble(SCALE, 1.0);
      currentGroup.AddBool(CENTER_CONTENT, false);
      currentGroup.AddBool(PAGE_MARK_PRINTING, false);
      currentGroup.AddBool(SCALE_DOWN_TO_FIT_PAGE, false);
      currentGroup.AddBool(SCALE_UP_TO_FIT_PAGE, false);

      // localization
      var rm =
        new ResourceManager("Demo.yFiles.Printing.Printing",
                            Assembly.GetExecutingAssembly());
      var rmf = new ResourceManagerI18NFactory();
      rmf.AddResourceManager(Handler.Name, rm);
      Handler.I18nFactory = rmf;
    }

    #endregion

    #region eventhandlers

    private void printPreviewButton_Click(object sender, EventArgs e) {
      PreparePrinting();

      // show new PrintPreviewDialog
      printDocument.Print(printDialog, true, true, InsetsD.Empty, PrintDialogTitle);
    }

    private void PreparePrinting() {
      GraphControl control = graphControl;
      // check if the rectangular region or the whole viewport should be printed
      bool useRect = (bool)handler.GetValue(OUTPUT, PRINT_RECTANGLE);
      RectD bounds = useRect ? exportRect.ToRectD() : graphControl.ContentRect;

      // check whether decorations (selection, handles, ...) should be hidden
      bool hide = (bool)handler.GetValue(OUTPUT, HIDE_DECORATIONS);
      if (hide) {
        // if so, create a new graph control with the same graph
        control = new GraphControl { Graph = graphControl.Graph, FlowDirection = graphControl.FlowDirection };
      }

      // read CanvasPrintDocument options
      printDocument.Scale = (double)Handler.GetValue(DOCUMENT_SETTINGS, SCALE);
      printDocument.CenterContent = (bool)Handler.GetValue(DOCUMENT_SETTINGS, CENTER_CONTENT);
      printDocument.PageMarkPrinting = (bool)Handler.GetValue(DOCUMENT_SETTINGS, PAGE_MARK_PRINTING);
      printDocument.ScaleDownToFitPage = (bool)Handler.GetValue(DOCUMENT_SETTINGS, SCALE_DOWN_TO_FIT_PAGE);
      printDocument.ScaleUpToFitPage = (bool)Handler.GetValue(DOCUMENT_SETTINGS, SCALE_UP_TO_FIT_PAGE);
      // set GraphControl
      printDocument.Canvas = control;
      // set print area
      printDocument.PrintRectangle = bounds;
    }

    private void printerSetupButton_Click(object sender, EventArgs e) {
      // show new PrintDialog
      var showDialog = printDialog.ShowDialog();
      if (showDialog.HasValue && showDialog.Value) {
        PreparePrinting();
        printDocument.Print(printDialog, true, false, InsetsD.Empty, PrintDialogTitle);
      }
    }

    private void pageSetupButton_Click(object sender, EventArgs e) {
      OptionHandler optionHandler = new OptionHandler("PageSettings");

      //We use only a subset of the available settings, since we want to filter some values 
      //("Unknown" can be returned by some properties, but may not be set on a PrintTicket)
      var selectionProvider = new DefaultSelectionProvider<PrintTicketOptionsHelper>(new[] { new PrintTicketOptionsHelper(printDialog.PrintTicket) });
      selectionProvider.ContextLookup = Lookups.CreateContextLookupChainLink(OptionHandlerContextLookup);
      selectionProvider.UpdatePropertyViewsNow();
      //We populate the OptionHandler
      optionHandler.BuildFromSelection(selectionProvider, Lookups.CreateContextLookupChainLink(OptionHandlerContextLookup));

      EditorForm form = new EditorForm() { OptionHandler = optionHandler, IsAutoAdopt = true, IsAutoCommit = false, Title = "Page Setup"};
      form.ShowDialog();
    }

    private object OptionHandlerContextLookup(object subject, Type type) {
      if (type == typeof(IOptionBuilder) && (subject is PrintTicketOptionsHelper || subject is PrintTicket)) {
        return new AttributeBasedOptionBuilder();
      }
      if (type == typeof(IPropertyMapBuilder) && ( subject is PrintTicketOptionsHelper || subject is PrintTicket)) {
        return new AttributeBasedPropertyMapBuilderAttribute().CreateBuilder(subject.GetType());
      }
      return null;
    }    

    /// <summary>
    /// Helper class that serves as proxy for the most important settings of a print ticket
    /// </summary>
    public class PrintTicketOptionsHelper
    {
      private readonly PrintTicket ticket;
      public PrintTicketOptionsHelper(PrintTicket ticket) {
        this.ticket = ticket;
      }

      public int? CopyCount {
        get { return ticket.CopyCount; }
        set { ticket.CopyCount = value; }
      }

      [OptionItemAttribute(Name = OptionItem.CustomDialogitemEditor, Value="ConverterBased.OptionItemPresenter")]
      [OptionItemAttribute(Name = OptionItem.CustomValueConverterAttribute, Value=typeof(PageMediaSizeConverter))]
      public PageMediaSize PageMediaSize {
        get { return ticket.PageMediaSize; }
        set { ticket.PageMediaSize = value; }
      }

      public class PageMediaSizeConverter: IValueConverter
      {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var sz = (PageMediaSize)value;
            var pageMediaSizeName = sz.PageMediaSizeName;
            if (pageMediaSizeName.HasValue) {
              return pageMediaSizeName.Value;
            }
            return sz.Height.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) + "x" +
                   sz.Width.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
          string strValue = value as string;
          PageMediaSize retval = null;
          if(strValue != null) {
            try {
              var pageMediaSizeName = (PageMediaSizeName) Enum.Parse(typeof (PageMediaSizeName), strValue);
              retval = new PageMediaSize(pageMediaSizeName);
            }
            catch(Exception) {}
            if(retval == null) {
              var tokens = strValue.Split('x');
              if(tokens.Length == 2) {
                try {
                  double height = Double.Parse(tokens[0], CultureInfo.InvariantCulture);
                  double width = Double.Parse(tokens[1], CultureInfo.InvariantCulture);
                  retval = new PageMediaSize(width, height);
                } catch (Exception) {
                }
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
            return (MyPageOrder)pageOrder;
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
          if(pageOrientation.HasValue && pageOrientation != System.Printing.PageOrientation.Unknown) {
            return (MyPageOrientation)pageOrientation;
          }
          return MyPageOrientation.Unspecified;
        }
        set {
          if (value == MyPageOrientation.Unspecified) {
            ticket.PageOrientation = null;
          }
          else {
            ticket.PageOrientation = (PageOrientation)value;
          }
        }
      }

      public int? PagesPerSheet {
        get { return ticket.PagesPerSheet; }
        set { ticket.PagesPerSheet = value; }
      }

      /// <summary>
      /// Helper enum that filters out the "Unknown" value which may not be set interactively, and filters the NullableValues from the original enum...
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
      /// Helper enum that filters out the "Unknown" value which may not be set interactively, and filters the NullableValues from the original enum...
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
    /// Helper method that allows for reusing this window in other applications.
    /// </summary>
    /// <param name="graphControl">The graph control.</param>
    public void ShowGraph(GraphControl graphControl) {
      this.graphControl.Graph = graphControl.Graph;
      this.graphControl.Selection.Clear(); // or possibly: = graphControl.Selection;
      this.graphControl.ContentRect = graphControl.ContentRect.GetEnlarged(20);
      
      // show all of the contents
      this.graphControl.FitContent();
      
      // or possibly the same viewport
//      this.graphControl.Zoom = graphControl.Zoom;
//      this.graphControl.ViewPoint = graphControl.ViewPoint;

      var inputMode = new MultiplexingInputMode();

      // set the whole content rect as the export rectangle
      exportRect.Reshape(graphControl.ContentRect);
      // or possibly just the visible viewport
      //exportRect.Set(graphControl.Viewport);

      AddExportRectInputModes(inputMode);
      this.graphControl.InputMode = inputMode;
    }
  }
}
