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
using System.Threading.Tasks;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Grouping;
using yWorks.Layout.Hierarchical;
using yWorks.Layout.Router;

namespace yWorks.Demo.SplitEdges
{
  /// <summary>
  ///   Demonstrates how to use split IDs to align edges outside with edges inside group nodes in
  ///   <see cref="RecursiveGroupLayout"/>.
  /// </summary>
  [Demo("split-edges", "layout", "Split Edges",
      Summary = "Shows how to align edges at group nodes using RecursiveGroupLayout with HierarchicalLayout.",
      Tags = ["layout", "hierarchical", "tree"], Keywords = ["layoutfeatures", "context menu", "recursive"])]
  public sealed partial class SplitEdgesDemo
  {
    private GraphEditorInputMode? inputMode;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    public SplitEdgesDemo() {
      InitializeComponent();
    }

    /// <summary>
    ///   Sets a <see cref="GraphEditorInputMode"/> and initializes the context menu.
    /// </summary>
    protected override async void Initialize() {
      ConfigureInteraction();
      InitializeDefaults();
      InitializeUI();
      await LoadGraph();
    }

    private void ConfigureInteraction() {
      inputMode = new GraphEditorInputMode {
          SelectableItems = GraphItemTypes.Edge | GraphItemTypes.Node,
          ContextMenuItems = GraphItemTypes.Edge | GraphItemTypes.Node
      };
      inputMode.ContextMenuInputMode.ClearMenu = true;
      var contextMenuSupport = new ContextMenuSupport(GraphControl, RunLayout);
      inputMode.PopulateItemContextMenu += contextMenuSupport.PopulateContextMenu;
      GraphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Initializes graph defaults.
    /// </summary>
    private void InitializeDefaults() {
      Graph.NodeDefaults.Style = DemoStyles.CreateDemoNodeStyle(Themes.Palette58);
      Graph.NodeDefaults.Size = new SizeD(50, 30);
      Graph.GroupNodeDefaults.Style = DemoStyles.CreateDemoGroupStyle(Themes.Palette58);

      var defaultEdgeBrush = new SolidColorBrush(Color.FromRgb(0x4e, 0x4e, 0x4e));
      Graph.EdgeDefaults.Style = new PolylineEdgeStyle {
          Pen = new Pen(defaultEdgeBrush, 3),
          TargetArrow = new Arrow(brush: defaultEdgeBrush, type: ArrowType.Triangle),
          SmoothingLength = 15
      };
      Graph.EdgeDefaults.ShareStyleInstance = false;

      Graph.GetDecorator().Edges.SelectionRenderer.AddConstant(new EdgeStyleIndicatorRenderer {
          EdgeStyle = new HighlightEdgeStyle(),
          ZoomPolicy = StyleIndicatorZoomPolicy.WorldCoordinates
      });
    }

    /// <summary>
    ///   Binds the various actions to the buttons in the toolbar.
    /// </summary>
    private void InitializeUI() {
      layoutButton.Click += async (_, _) => await RunLayout();
      resetButton.Click += async (_, _) => await LoadGraph();
    }

    private async Task LoadGraph() {
      Graph.Clear();
      // Build the graph from the given data set.
      BuildGraphFromJson("sample.json", Graph, edgeTagType: typeof(EdgeData));

      foreach (var edge in Graph.Edges) {
        if (edge.Tag is EdgeData { Tag: { } tag }) {
          edge.Tag = tag;
          UpdateEdgeColor(edge, tag.Color ?? "#4e4e4e");
        }
      }

      await RunLayout();
    }

    /// <summary>
    ///   Runs a <see cref="RecursiveGroupLayout"/> with <see cref="HierarchicalLayout"/> as its core layout.
    /// </summary>
    private async Task RunLayout() {
      var hierarchicalLayout = new HierarchicalLayout {
          LayoutOrientation = LayoutOrientation.LeftToRight,
          DefaultEdgeDescriptor = new HierarchicalLayoutEdgeDescriptor { DirectGroupContentEdgeRouting = true }
      };

      var edgeRouter = new EdgeRouter();
      edgeRouter.DefaultEdgeDescriptor.DirectGroupContentEdgeRouting = true;

      var recursiveGroupLayout = new RecursiveGroupLayout(hierarchicalLayout) { InterEdgeRouter = edgeRouter };

      // Use the split IDs from the edge tags.
      var recursiveGroupLayoutData = recursiveGroupLayout.CreateLayoutData(Graph);
      recursiveGroupLayoutData.SourceSplitIds.MapperFunction = edge => (edge.Tag as SplitEdgeTag)?.SourceSplitId;
      recursiveGroupLayoutData.TargetSplitIds.MapperFunction = edge => (edge.Tag as SplitEdgeTag)?.TargetSplitId;

      var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(Graph);
      hierarchicalLayoutData.EdgeThickness.Constant = 3;

      var genericLayoutData = Graph.CreateGenericLayoutData();
      genericLayoutData.AddItemMapping(LayoutKeys.GroupNodePaddingDataKey).Constant = new InsetsD(20, 35, 20, 20);

      var layoutData = recursiveGroupLayoutData.CombineWith(hierarchicalLayoutData).CombineWith(genericLayoutData);

      SetUIDisabled(true);
      try {
        await GraphControl.ApplyLayoutAnimated(recursiveGroupLayout, TimeSpan.FromMilliseconds(700), layoutData);
      } finally {
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Disables the UI.
    /// </summary>
    /// <param name="disabled">True if the element should be disabled; false otherwise.</param>
    private void SetUIDisabled(bool disabled) {
      layoutButton.IsEnabled = !disabled;
      resetButton.IsEnabled = !disabled;
      if (inputMode is not null) {
        inputMode.Enabled = !disabled;
      }
    }

    private void UpdateEdgeColor(IEdge edge, string color) {
      if (edge.Style is not PolylineEdgeStyle edgeStyle) {
        return;
      }
      var brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(color)!);
      edgeStyle.Pen = new Pen(brush, 3);
      edgeStyle.TargetArrow = new Arrow(brush: brush, type: ArrowType.Triangle);
    }

    /// <summary>
    ///   An edge style to draw the selection highlight below the edge.
    /// </summary>
    private sealed class HighlightEdgeStyle : EdgeStyleBase<DrawingVisual>
    {
      protected override DrawingVisual CreateVisual(IRenderContext context, IEdge edge) {
        var visual = new DrawingVisual();
        Render(context, visual, edge);
        return visual;
      }

      protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, IEdge edge) {
        Render(context, oldVisual, edge);
        return oldVisual;
      }

      private void Render(IRenderContext context, DrawingVisual visual, IEdge edge) {
        if (edge.Style is not PolylineEdgeStyle style || style.Pen?.Brush is not SolidColorBrush edgeBrush) {
          return;
        }

        var edgePath = GetPath(edge);
        if (edgePath is null) {
          return;
        }
        var croppedPath = CropPath(edge, style.SourceArrow, style.TargetArrow, edgePath);
        if (croppedPath is null) {
          return;
        }
        var path = croppedPath.CreateSmoothedPath(15);
        var highlightBrush = new SolidColorBrush(edgeBrush.Color) { Opacity = 0.75 };
        var highlightPen = new Pen(highlightBrush, 6);
        using var drawing = visual.RenderOpen();
        path.Render(drawing, null!, highlightPen);
      }
    }

    private sealed class EdgeData
    {
      public SplitEdgeTag? Tag { get; set; }
    }

    internal sealed class SplitEdgeTag
    {
      public string? SourceSplitId { get; set; }

      public string? TargetSplitId { get; set; }

      public string? Color { get; set; }
    }

    [STAThread]
    private static void Main() => Run<SplitEdgesDemo>();
  }
}
