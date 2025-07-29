/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using Demo.yFiles.Option.Constraint;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.I18N;
using Demo.yFiles.Toolkit;
using Microsoft.Win32;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;
using Path = System.IO.Path;
using Rect = System.Windows.Rect;

namespace Demo.yFiles.ImageExport
{
  /// <summary>
  ///   This demo shows how to export a graph or part of it to different
  ///   pixel and vector format images like JPEG, PNG, TIFF, GIF, BMP, or XPS.
  /// </summary>
  /// <remarks>For more details, see the description file or run the application.</remarks>
  public partial class ImageExportWindow
  {
    #region private fields

    // Option handler for export options

    // Currently focused CanvasControl for which the zoom buttons work
    private CanvasControl focusedCanvas;

    // region that gets exported
    private MutableRectangle exportRect;

    #endregion

    #region Constructors

    public ImageExportWindow() {
      InitializeComponent();
    }

    #endregion

    private void OnLoaded(object src, EventArgs args) {
      InitializeInputModes();
      InitializeGraph();
      SetupOptions();
    }

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
            return path.PathContains(location, context.HitTestRadius);
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

    #endregion

    #region export option handler

    private void SetupOptions() {
      // create the options
      SetupHandler();
      // populate the control to visualize them
      editorControl.IsAutoAdopt = true;
      editorControl.IsAutoCommit = true;
      editorControl.OptionHandler = Handler;
    }

    /// <summary>
    ///   Initializes the option handler for the export
    /// </summary>
    private void SetupHandler() {
      Handler = new OptionHandler(IMAGE_EXPORT);
      Handler.PropertyChanged += handler_PropertyChanged;
      var currentGroup = Handler.AddGroup(OUTPUT);
      OptionItem formatItem = currentGroup.AddList(FORMAT, Formats.Keys, FORMAT_JPG);
      currentGroup.AddBool(HIDE_DECORATIONS, true);
      currentGroup.AddBool(EXPORT_RECTANGLE, true);
      currentGroup = Handler.AddGroup(BOUNDS);

      OptionItem sizeItem = currentGroup.AddList(SIZE, SizeModes, USE_ORIGINAL_SIZE);
      var widthItem = currentGroup.AddInt(WIDTH, DefaultWidth, 1, int.MaxValue);
      var heightItem = currentGroup.AddInt(HEIGHT, DefaultHeight, 1, int.MaxValue);

      currentGroup.AddDouble(SCALE, DefaultScale);

      currentGroup = Handler.AddGroup(MARGINS);

      currentGroup.AddInt(LEFT_MARGIN, DefaultMargins);
      currentGroup.AddInt(RIGHT_MARGIN, DefaultMargins);
      currentGroup.AddInt(TOP_MARGIN, DefaultMargins);
      currentGroup.AddInt(BOTTOM_MARGIN, DefaultMargins);

      currentGroup = Handler.AddGroup(JPG);
      var qualityItem = currentGroup.AddDouble(QUALITY, DefaultQuality);

      currentGroup = Handler.AddGroup(TIFF);
      OptionItem compressionItem = currentGroup.AddList(COMPRESSION, Compressions.Keys, DefaultCompression);

      currentGroup = Handler.AddGroup(PNG);
      var transparentItem = currentGroup.AddBool(TRANSPARENT, false);

      var cm = new ConstraintManager(Handler);
      cm.SetEnabledOnValueEquals(sizeItem, SPECIFY_WIDTH, widthItem);
      cm.SetEnabledOnValueEquals(sizeItem, SPECIFY_HEIGHT, heightItem);

      cm.SetEnabledOnValueEquals(formatItem, FORMAT_PNG, transparentItem);

      cm.SetEnabledOnValueEquals(formatItem, FORMAT_JPG, qualityItem);
      cm.SetEnabledOnValueEquals(formatItem, FORMAT_TIFF, compressionItem);

      // localization
      var rm = new ResourceManager("Demo.yFiles.ImageExport.ImageExport", Assembly.GetExecutingAssembly());
      var rmf = new ResourceManagerI18NFactory();
      rmf.AddResourceManager(Handler.Name, rm);
      Handler.I18nFactory = rmf;
    }

    #endregion

    #region ImageExport

    private void ImageExportToPreview() {
      ImageExport();
    }

    [CanBeNull]
    private ImageSource ImageExport() {
      var control = graphControl;
      // check if the rectangular region or the whole viewport should be exported
      var useRect = (bool) Handler.GetValue(OUTPUT, EXPORT_RECTANGLE);
      var bounds = useRect ? exportRect.ToRectD() : control.Viewport;

      // get the format
      var formatChoice = (string) Handler.GetValue(OUTPUT, FORMAT);
      var format = Formats[formatChoice];
      // check whether decorations (selection, handles, ...) should be hidden
      var hide = (bool) Handler.GetValue(OUTPUT, HIDE_DECORATIONS);
      if (hide) {
        // if so, create a new graph control with the same graph
        control = new GraphControl {
            Graph = graphControl.Graph,
            FlowDirection = graphControl.FlowDirection,
            Background = graphControl.Background,
            Projection = graphControl.Projection
        };
      }
      IImageExporter exporter;
      var config = GetConfig(bounds, !useRect);

      var transparentBackground = (bool) Handler.GetValue(PNG, TRANSPARENT);

      if (format.Equals("XPS")) {
        // create the exporter
        var xpsExporter = new XpsExporter(config);
        var tempFileName = Path.GetTempFileName();
        using (var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite)) {
          xpsExporter.Export(control, fileStream);
        }

        documentViewer.Visibility = Visibility.Visible;
        previewCanvas.Visibility = Visibility.Hidden;
        documentViewer.Document = new XpsDocument(tempFileName, FileAccess.Read).GetFixedDocumentSequence();
        return null;
      }
      // create the exporter
      var pixelImageExport = new PixelImageExporter(config);
      AddExportParameters(pixelImageExport, format);
      pixelImageExport.OutputFormat = format;
      // check if the format is transparent PNG
      if (!format.Equals("image/png") || !transparentBackground) {
        // if not, set the background color
        pixelImageExport.Background = control.Background ?? Brushes.White;
      }
      exporter = pixelImageExport;
      var memoryStream = new MemoryStream();
      try {
        exporter.Export(control, memoryStream);

        // reset the stream
        memoryStream.Position = 0;
        // and read back the image for display
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memoryStream;
        bitmapImage.EndInit();

        var renderTree = previewCanvas.RenderTree;
        if (renderTree.RootGroup.First != null) {
          renderTree.Remove(renderTree.RootGroup.First);
        }
        documentViewer.Visibility = Visibility.Hidden;
        previewCanvas.Visibility = Visibility.Visible;
        var image = new Image { Source = bitmapImage, Width = bitmapImage.Width, Height = bitmapImage.Height };
        image.SetCanvasArrangeRect(new Rect(0, 0, image.Width, image.Height));
        renderTree.CreateElement(renderTree.RootGroup, image);
        previewCanvas.ContentBounds = new RectD(0, 0, bitmapImage.Width, bitmapImage.Height).GetEnlarged(20);

        return bitmapImage;
      } catch (IOException exception) {
        MessageBox.Show(exception.Message, "I/O Error", MessageBoxButton.OK);
        return null;
      } finally {
        if (control != graphControl) {
          // Don't keep the graph around in the temporary control to avoid memory leaks.
          control.ClearValue(GraphControl.GraphProperty);
        }
      }
    }

    private void ImageExportToFile() {
      var control = graphControl;
      // check if the rectangular region or the whole viewport should be printed
      var useRect = (bool) Handler.GetValue(OUTPUT, EXPORT_RECTANGLE);
      var bounds = useRect ? exportRect.ToRectD() : control.Viewport;

      // get the format
      var formatChoice = (string) Handler.GetValue(OUTPUT, FORMAT);
      var format = Formats[formatChoice];
      // check whether decorations (selection, handles, ...) should be hidden
      var hide = (bool) Handler.GetValue(OUTPUT, HIDE_DECORATIONS);
      if (hide) {
        // if so, create a new graph control with the same graph
        control = new GraphControl { Graph = graphControl.Graph, FlowDirection = graphControl.FlowDirection };
      }
      IImageExporter exporter;
      var config = GetConfig(bounds, !useRect);

      if (format.Equals("XPS")) {
        // create the exporter
        var emfImageExporter = new XpsExporter(config);
        exporter = emfImageExporter;
        saveFileDialog.Reset();
        saveFileDialog.Filter = "XPS files (*.xps)|*.xps";
        if (saveFileDialog.ShowDialog(this) == true) {
          var fileName = saveFileDialog.FileName;
          if (File.Exists(fileName)) {
            File.Delete(fileName);
          }
          using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite)) {
            exporter.Export(control, stream);
          }
        }
      } else {
        var transparentBackground = (bool) Handler.GetValue(PNG, TRANSPARENT);
        // create the exporter
        var pixelImageExport = new PixelImageExporter(config);
        AddExportParameters(pixelImageExport, format);
        pixelImageExport.OutputFormat = format;
        // check if the format is transparent PNG
        if (!format.Equals("image/png") || !transparentBackground) {
          // if not, set the background color
          pixelImageExport.Background = control.Background ?? Brushes.White;
        }
        exporter = pixelImageExport;

        saveFileDialog.Reset();
        saveFileDialog.Filter = format + "|*." + format.Substring(6);
        if (saveFileDialog.ShowDialog(this) == true) {
          var stream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write);
          exporter.Export(control, stream);
          stream.Flush();
          stream.Close();

          if (control != graphControl) {
            // Don't keep the graph around in the temporary control to avoid memory leaks.
            control.ClearValue(GraphControl.GraphProperty);
          }
        }
      }
    }

    /// <summary>
    ///   Gets the export configuration
    /// </summary>
    private ContextConfigurator GetConfig(RectD worldBounds, bool useViewport) {
      var config = new ContextConfigurator(worldBounds) { Projection = graphControl.Projection };
      SetScale(config, useViewport);
      // get the margins
      var leftMargin = (int) Handler.GetValue(MARGINS, LEFT_MARGIN);
      var rightMargin = (int) Handler.GetValue(MARGINS, RIGHT_MARGIN);
      var topMargin = (int) Handler.GetValue(MARGINS, TOP_MARGIN);
      var bottomMargin = (int) Handler.GetValue(MARGINS, BOTTOM_MARGIN);
      config.Margins = new InsetsD(leftMargin, topMargin, rightMargin, bottomMargin);
      return config;
    }

    /// <summary>
    ///   Gets the various export parameters for the chosen format
    /// </summary>
    private void AddExportParameters(PixelImageExporter pixelImageExport, string format) {
      if (format.Equals("image/jpeg")) {
        // get jpeg quality
        pixelImageExport.Quality = (double) Handler.GetValue(JPG, QUALITY);
      } else if (format.Equals("image/tiff")) {
        // get tiff compression
        var compressionChoice = (string) Handler.GetValue(TIFF, COMPRESSION);
        pixelImageExport.Compression = !compressionChoice.Equals(COMPRESSION_NONE)
            ? TiffCompressOption.Default
            : TiffCompressOption.None;
      }
    }

    /// <summary>
    ///   Sets the export scale
    /// </summary>
    private void SetScale(ContextConfigurator config, bool useViewport) {
      // get the scale value specified in the options
      var userScale = (double) Handler.GetValue(BOUNDS, SCALE);
      // consider the zoom level
      if (useViewport) {
        userScale *= graphControl.Zoom;
      }
      // look if a fixed size has been specified
      var sizeChoice = (string) Handler.GetValue(BOUNDS, SIZE);
      switch (sizeChoice) {
        case SPECIFY_WIDTH:
          var newWidth = (int) Handler.GetValue(BOUNDS, WIDTH);
          config.Scale = config.CalculateScaleForWidth((int) (userScale * newWidth));
          break;
        case SPECIFY_HEIGHT:
          var newHeight = (int) Handler.GetValue(BOUNDS, HEIGHT);
          config.Scale = config.CalculateScaleForHeight((int) (userScale * newHeight));
          break;
        default:
          config.Scale = (float) userScale;
          break;
      }
    }

    #endregion

    #region eventhandlers

    private void exportButton_Click(object sender, EventArgs e) {
      // select the export tab, export gets triggered automatically when tab is selected
      if (graphTabControl.SelectedIndex != 1) {
        graphTabControl.SelectedIndex = 1;
      } else {
        // already displayed - update export
        ImageExportToPreview();
      }
    }

    private void exportToFileButton_Click(object sender, EventArgs e) {
      ImageExportToFile();
    }

    private void graphTabControl_SelectedIndexChanged(object sender, EventArgs e) {
      if (Handler != null) {
        if (graphTabControl.SelectedIndex == 1) {
          // if preview tab has been selected, trigger export
          ImageExportToPreview();
          var i = ((TabControl) sender).SelectedIndex;
          focusedCanvas = i == 1 ? previewCanvas : graphControl;
          focusedCanvas.Focus();
        }
      }
    }

    /// <summary>
    ///   Called when a value in the option handler is changed
    /// </summary>
    private void handler_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      ImageExportToPreview();
    }

    private void copyToClipboardButton_Click(object sender, EventArgs e) {
      // Clipboard.SetDataObject(ImageExport());
      var img = ImageExport();
      if (img is BitmapSource) {
        var image = img as BitmapSource;
        // This does not work either for all kinds of applications - it does for some, however
        Clipboard.SetImage(image);
      }
    }

    #endregion

    #region static members

    private const string IMAGE_EXPORT = "IMAGE_EXPORT";
    private const string BOUNDS = "BOUNDS";
    private const string SIZE = "SIZE";
    private const string USE_ORIGINAL_SIZE = "USE_ORIGINAL_SIZE";
    private const string SPECIFY_WIDTH = "SPECIFY_WIDTH";
    private const string SPECIFY_HEIGHT = "SPECIFY_HEIGHT";
    private const string WIDTH = "WIDTH";
    private const string HEIGHT = "HEIGHT";
    private const string MARGINS = "MARGINS";
    private const string LEFT_MARGIN = "LEFT_MARGIN";
    private const string RIGHT_MARGIN = "RIGHT_MARGIN";
    private const string TOP_MARGIN = "TOP_MARGIN";
    private const string BOTTOM_MARGIN = "BOTTOM_MARGIN";
    private const string TRANSPARENT = "TRANSPARENT";
    private const string SCALE = "SCALE";
    private const string JPG = "JPG";
    private const string XPS = "EMF";
    private const string PNG = "PNG";
    private const string QUALITY = "QUALITY";
    private const string TIFF = "TIFF";
    private const string COLOR_DEPTH = "COLOR_DEPTH";
    private const string COMPRESSION = "COMPRESSION";
    private const string COMPRESSION_NONE = "COMPRESSION_NONE";
    private const string COMPRESSION_LZW = "COMPRESSION_LZW";
    private const string OUTPUT = "OUTPUT";
    private const string FORMAT = "FORMAT";
    private const string FORMAT_JPG = "FORMAT_JPG";
    private const string FORMAT_PNG = "FORMAT_PNG";
    private const string FORMAT_TIFF = "FORMAT_TIFF";
    private const string FORMAT_GIF = "FORMAT_GIF";
    private const string FORMAT_BMP = "FORMAT_BMP";
    private const string FORMAT_XPS = "FORMAT_EMF";
    private const string HIDE_DECORATIONS = "HIDE_DECORATIONS";
    private const string EXPORT_RECTANGLE = "EXPORT_RECTANGLE";

    private const string GRAPHICS = "GRAPHICS";
    private const string SMOOTHING = "SMOOTHING";
    private const string INTERPOLATION = "INTERPOLATION";
    private const string PIXELOFFSET = "PIXELOFFSET";
    private const string TEXTRENDERING = "TEXTRENDERING";


    private const int DefaultWidth = 500;
    private const int DefaultHeight = 500;
    private const int DefaultMargins = 0;
    private const double DefaultScale = 1.0;
    private const double DefaultQuality = 0.75;
    private const int DefaultDepth = 24;
    private const string DefaultCompression = COMPRESSION_LZW;

    private static readonly List<string> SizeModes = new();

    private static readonly Dictionary<string, TiffCompressOption> Compressions = new();

    private static readonly Dictionary<string, string> Formats = new();

    private readonly SaveFileDialog saveFileDialog = new();

    static ImageExportWindow() {
      SizeModes.Add(USE_ORIGINAL_SIZE);
      SizeModes.Add(SPECIFY_WIDTH);
      SizeModes.Add(SPECIFY_HEIGHT);

      Compressions.Add(COMPRESSION_NONE, TiffCompressOption.None);
      Compressions.Add(COMPRESSION_LZW, TiffCompressOption.Lzw);

      Formats.Add(FORMAT_JPG, "image/jpeg");
      Formats.Add(FORMAT_TIFF, "image/tiff");
      Formats.Add(FORMAT_GIF, "image/gif");
      Formats.Add(FORMAT_PNG, "image/png");
      Formats.Add(FORMAT_BMP, "image/bmp");
      Formats.Add(FORMAT_XPS, "XPS");
    }

    #endregion

    /// <summary>
    ///   Helper method that allows for reusing this window in other applications.
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

      // create mode which supports moving the viewport by pressing ctrl
      var moveViewportInputMode = new MoveViewportInputMode {
          BeginRecognizer = (source, args) => EventRecognizers.MouseDown(source, args) &&
              (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
      };

      inputMode.Add(moveViewportInputMode);

      AddExportRectInputModes(this.graphControl.InputModeContext, inputMode);
      this.graphControl.InputMode = inputMode;
    }
  }
}
