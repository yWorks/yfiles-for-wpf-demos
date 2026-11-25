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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Tree;
using DefaultGraph = yWorks.Graph.DefaultGraph;

namespace yWorks.Demo.TreeMap
{
  /// <summary>
  ///   Visualizes a directory tree with nested rectangles whose sizes represent file sizes.
  /// </summary>
  [Demo("tree-map", "layout", "Tree Map", Summary = "Shows disk usage of a directory tree with the Tree Map layout.",
      Tags = ["layout", "tree map"],
      Keywords = ["tree map", "animations", "tool tips", "data analysis", "data management", "padding", "margin"],
      RelatedDemos = [])]
  public sealed partial class TreeMapDemo
  {
    /**
     * Maps each node to a preferred size.
     */
    private readonly Dictionary<INode, SizeD> preferredSizes = [];

    /**
     * The complete graph containing all nodes and the general hierarchy information.
     */
    private IGraph masterGraph = null!;

    /**
     * The input mode used to navigate the displayed graph.
     */
    private GraphViewerInputMode inputMode = null!;

    private FoldingManager? activeFoldingManager;

    public TreeMapDemo() {
      InitializeComponent();
    }

    /**
     * Starts and sets up the demo.
     */
    protected override async void Initialize() {
      InitializeGraph();
      InitializeInputModes();
      InitializeUI();
      await UpdateGraph();
    }

    /**
     * Initializes the default styles and loads a sample graph from JSON.
     */
    private void InitializeGraph() {
      masterGraph = new DefaultGraph();

      // Configure default styles to instantly apply to the sample graph.
      masterGraph.NodeDefaults.Style = new ColorNodeStyle();
      masterGraph.NodeDefaults.Size = new SizeD(70, 70);
      masterGraph.NodeDefaults.Labels.Style = new LabelStyle {
          TextSize = 9,
          TextBrush = Brushes.White,
          TextAlignment = TextAlignment.Center,
          VerticalTextAlignment = VerticalAlignment.Center
      };
      masterGraph.NodeDefaults.Labels.LayoutParameter = StretchNodeLabelModel.Center;
      masterGraph.NodeDefaults.Labels.ShareStyleInstance = false;
      masterGraph.NodeDefaults.ShareStyleInstance = false;

      masterGraph.GroupNodeDefaults.Style = new ShapeNodeStyle {
          Brush = new SolidColorBrush(Color.FromRgb(104, 104, 104)),
          Pen = null
      };
      masterGraph.GroupNodeDefaults.Labels.Style = new LabelStyle {
          BackgroundBrush = new SolidColorBrush(Color.FromRgb(77, 77, 77)),
          TextBrush = Brushes.White,
          TextSize = 14,
          TextAlignment = TextAlignment.Center,
          VerticalTextAlignment = VerticalAlignment.Center,
          Padding = new InsetsD(5, 3, 5, 3),
          TextWrapping = TextWrapping.Wrap,
          TextTrimming = TextTrimming.CharacterEllipsis
      };
      masterGraph.GroupNodeDefaults.Labels.LayoutParameter = StretchNodeLabelModel.Top;

      // Disable hierarchical nesting, to easily control the z-order of the nodes during animation. We assume
      // this graph has no edges.
      graphControl.GraphModelManager.HierarchicalNestingPolicy = HierarchicalNestingPolicy.None;

      // Load the sample graph.
      BuildGraphFromJson("TreeMapData.json", masterGraph, typeof(TreeMapTag), typeof(TreeMapTag));
    }

    /**
     * Initializes the input modes. Groups and folders can be navigated by clicking on the respective node.
     * Valid and clickable nodes get a highlight when the mouse hovers over them.
     */
    private void InitializeInputModes() {
      inputMode = new GraphViewerInputMode {
          SelectableItems = GraphItemTypes.None,
          FocusableItems = GraphItemTypes.None,
          ClickableItems = GraphItemTypes.Node,
          ToolTipItems = GraphItemTypes.Node
      };
      inputMode.ItemClicked += async (_, evt) => {
        if (evt.Item is INode node && await OnNodeClicked(node)) {
          evt.Handled = true;
        }
      };

      // Add tooltips that show the label text.
      inputMode.ToolTipInputMode.ToolTipLocationOffset = new PointD(10, 10);
      inputMode.QueryItemToolTip += (_, evt) => {
        if (!evt.Handled && evt.Item is INode node && node.Labels.Count > 0) {
          evt.ToolTip = node.Labels[0].Text;
          evt.Handled = true;
        }
      };

      inputMode.ItemHoverInputMode.HoverItems = GraphItemTypes.Node;
      inputMode.ItemHoverInputMode.HoveredItemChanged += OnHoveredItemChanged;
      graphControl.InputMode = inputMode;
    }

    private void OnHoveredItemChanged(object? sender, HoveredItemChangedEventArgs evt) {
      var highlights = graphControl.Highlights;
      highlights.Clear();

      var item = evt.Item as INode;
      inputMode.DefaultCursor = Cursors.Arrow;
      if (item is null) {
        return;
      }

      // Get the node from the master graph to be able to check the hierarchy information.
      var root = masterGraph.Nodes.FirstOrDefault(node => EqualTags(item, node));
      var itemGraph = graphControl.Graph;
      if (root is not null && masterGraph.IsGroupNode(root)) {
        // A group node can be entered, or it can lead back to a higher hierarchy level.
        if (itemGraph.GetParent(item) is null) {
          root = masterGraph.GetParent(root);
        }
        if (root is not null) {
          inputMode.DefaultCursor = Cursors.Hand;
          highlights.Add(item);
        }
      } else {
        var parent = root is null ? null : masterGraph.GetParent(root);
        if (parent is not null && (parent.Tag as TreeMapTag)?.ParentGroupRef != string.Empty) {
          var itemParent = itemGraph.GetParent(item);
          if (itemParent is not null) {
            inputMode.DefaultCursor = Cursors.Hand;
            highlights.Add(itemParent);
          }
        }
      }
    }

    /**
     * Handles node clicked events.
     * @param clickedNode the node that was clicked.
     * @returns true if the visible graph is changed due to the click and false otherwise.
     */
    private async Task<bool> OnNodeClicked(INode clickedNode) {
      // Remember if the click will be going downwards or upwards.
      var isDrillDown = true;

      // Get the node from the master graph to be able to check the hierarchy information.
      var root = masterGraph.Nodes.FirstOrDefault(node => EqualTags(clickedNode, node));

      // Group nodes can be entered, or they lead to a higher hierarchy level.
      if (root is not null && masterGraph.IsGroupNode(root)) {
        var visibleGraph = graphControl.Graph;
        if (visibleGraph.GetParent(clickedNode) is null) {
          // Up to a higher hierarchy level, take the parent of the root node as new root.
          root = masterGraph.GetParent(root);
          isDrillDown = false;
        } else {
          var preferredSize = preferredSizes.GetValueOrDefault(root);
          if (preferredSize == default) {
            preferredSizes[root] = new SizeD(clickedNode.Layout.Width, clickedNode.Layout.Height);
          }
        }

        if (root is not null) {
          masterGraph.SetNodeLayout(root, clickedNode.Layout.ToRectD());

          // Update the graph to only contain the new root and its children and grandchildren.
          await UpdateGraph(root, clickedNode, isDrillDown);
        }
        return true;
      }
      return false;
    }

    /**
     * Updates the current graph when the root node changes.
     */
    private async Task UpdateGraph(INode? root = null, INode? clickedNode = null, bool isDrillDown = false) {
      IGraph graph = new DefaultGraph {
          NodeDefaults = masterGraph.NodeDefaults,
          GroupNodeDefaults = masterGraph.GroupNodeDefaults
      };
      var copier = new GraphCopier();

      INode? clickedNodeCopy = null;
      if (root is not null) {
        // Collect the descendants of the root node that should be visible.
        var visibleNodes = new List<INode>();
        foreach (var child in masterGraph.GetChildren(root)) {
          visibleNodes.Add(child);
          visibleNodes.AddRange(masterGraph.GetChildren(child));
        }
        visibleNodes.Add(root);

        // Create a copy of the graph containing only the visible nodes.
        var items = visibleNodes.Cast<IModelItem>().Concat(visibleNodes.SelectMany(node => node.Labels));
        copier.Copy(masterGraph, graph, items, null, PointD.Origin);

        // Center nodes inside the root to get a smooth animation.
        foreach (var node in graph.Nodes) {
          graph.SetNodeCenter(node, root.Layout.GetCenter());

          // Animate the upwards direction by fitting the clicked node into the layout. This is achieved by keeping
          // the original layout for the clicked nodes and children.
          if (clickedNode is not null && !isDrillDown && EqualTags(node, clickedNode)) {
            graph.SetNodeLayout(node, clickedNode.Layout.ToRectD());
            clickedNodeCopy = node;
            var visibleGraph = graphControl.Graph;
            if (visibleGraph.IsGroupNode(clickedNode)) {
              // Also transfer the layout of the children.
              foreach (var child in visibleGraph.GetChildren(clickedNode)) {
                var copiedChild = graph.Nodes.FirstOrDefault(candidate => EqualTags(child, candidate));
                if (copiedChild is not null) {
                  graph.SetNodeLayout(copiedChild, child.Layout.ToRectD());
                }
              }
            }
          }
        }
      } else {
        // Use the master graph since the global root was selected.
        copier.Copy(masterGraph, graph);
        graphControl.Graph = graph;

        // Center nodes inside the viewport to get a smooth animation.
        var center = graphControl.Viewport.GetCenter();
        foreach (var node in graph.Nodes) {
          graph.SetNodeCenter(node, center);
        }
      }

      // Clear highlights to avoid artifact highlights of the previews sub-graph.
      graphControl.Highlights.Clear();

      // Create a folding view that collapses the grandchildren of the current root.
      var newFoldingManager = new FoldingManager(graph);
      newFoldingManager.FolderNodeConverter = new FolderNodeConverter {
          FolderNodeDefaults = {
              CopyLabels = true,
              Style = new ColorNodeStyle(),
              ShareStyleInstance = true,
              Labels = {
                  Style = masterGraph.NodeDefaults.Labels.Style,
                  LayoutParameter = StretchNodeLabelModel.Center
              }
          }
      };
      var graphToFold = graph;
      var foldingView = newFoldingManager.CreateFoldingView(isExpanded: node => {
        var parent = graphToFold.GetParent(node);
        return parent is null || graphToFold.GetParent(parent) is null;
      });
      graph = foldingView.Graph;

      // Update the label text; folders should show their sizes like files.
      foreach (var node in graph.Nodes) {
        if (!foldingView.IsExpanded(node) && node.Labels.Count > 0) {
          var label = node.Labels[0];
          if (node.Tag is TreeMapTag tag) {
            var text = label.Text;
            graph.SetLabelText(label, $"{text}\n({GetSizeString(tag.Size)})");
          }
        }
      }

      // Update the navigation.
      UpdateNavigation(root);

      // Register a highlight.
      graph.GetDecorator().Nodes.HighlightRenderer.AddConstant(new NodeStyleIndicatorRenderer {
          NodeStyle = new ShapeNodeStyle {
              Brush = null,
              Pen = new Pen(Brushes.Crimson, 3)
          }
      });

      graphControl.Graph = graph;
      activeFoldingManager?.Dispose();
      activeFoldingManager = newFoldingManager;

      // If this is an outward animation, bring the clicked node to the front.
      if (clickedNodeCopy is not null && !isDrillDown) {
        var groupTag = (clickedNodeCopy.Tag as TreeMapTag)?.GroupTag;
        var clickedItem = graph.Nodes.FirstOrDefault(node =>
            string.Equals((node.Tag as TreeMapTag)?.GroupTag, groupTag, StringComparison.Ordinal));
        if (clickedItem is not null) {
          graphControl.GraphModelManager.ToFront(clickedItem);
          if (graph.IsGroupNode(clickedItem)) {
            foreach (var child in graph.GetChildren(clickedItem)) {
              graphControl.GraphModelManager.ToFront(child);
            }
          }
        }
      }

      // Calculate a layout for the new sub-graph.
      await ApplyLayout();
    }

    private void UpdateNavigation(INode? root) {
      navigation.Items.Clear();
      if (root is null) {
        navigation.Items.Add(new BreadcrumbItem {
            Content = "yFiles-for-HTML",
            Tag = "global"
        });
        return;
      }

      var ancestors = new List<INode>();
      for (var node = masterGraph.GetParent(root); node is not null; node = masterGraph.GetParent(node)) {
        ancestors.Add(node);
      }
      ancestors.Reverse();
      foreach (var ancestor in ancestors) {
        navigation.Items.Add(new BreadcrumbItem {
            Content = (ancestor.Tag as TreeMapTag)?.Label,
            Tag = ancestor
        });
      }
      navigation.Items.Add(new BreadcrumbItem {
          Content = (root.Tag as TreeMapTag)?.Label,
          Tag = root
      });
    }

    private async void OnNavigationItemInvoked(object? sender, BreadcrumbItemInvokedEventArgs evt) {
      if (evt.Item is not BreadcrumbItem item) {
        return;
      }
      if (item.Tag as string == "global") {
        await UpdateGraph();
      } else if (item.Tag is INode root) {
        if ((root.Tag as TreeMapTag)?.GroupTag == "yFiles-for-HTML") {
          await UpdateGraph();
        } else {
          await UpdateGraph(root);
        }
      }
    }

    /**
     * Checks if the id and the groupTag properties of the tags of the given nodes are equal.
     */
    private static bool EqualTags(INode? n1, INode? n2) {
      if (n1?.Tag is not TreeMapTag t1 || n2?.Tag is not TreeMapTag t2) {
        return false;
      }
      return (!string.IsNullOrEmpty(t1.Id) && t1.Id == t2.Id) ||
          (!string.IsNullOrEmpty(t1.GroupTag) && t1.GroupTag == t2.GroupTag);
    }

    /**
     * Transforms the given size to a string showing the largest unit and the smallest number.
     */
    private static string GetSizeString(long size) {
      if (size >= 1000000) {
        return $"{size / 1000000d:0.00}mb";
      }
      if (size >= 1000) {
        return $"{size / 1000d:0.00}kb";
      }
      return $"{size}byte";
    }

    /**
     * Applies a TreeMapLayout to the current graph. The configuration is derived from the information
     * in the interaction panel.
     */
    private async Task ApplyLayout() {
      var graph = graphControl.Graph;

      // Configure the layout algorithm using the settings from the interaction panel.
      var layout = new TreeMapLayout {
          PreferredSize = GetPreferredSize(graph),
          AspectRatio = aspectRatioSlider.Value,
          TilingStrategy = tilingAlgorithmBox.SelectedIndex == 0
              ? TilingStrategy.Squarified
              : TilingStrategy.SliceAndDice,
          MinimumNodeSize = new SizeD(minimumWidthSlider.Value, minimumHeightSlider.Value),
          Spacing = spacingSlider.Value
      };

      // Determine the current weight range.
      const double maximumWeight = 500;
      const double minimumWeight = 10;
      var minWeight = double.MaxValue;
      var maxWeight = 0d;
      foreach (var node in graph.Nodes) {
        if (!graph.IsGroupNode(node)) {
          if (node.Tag is not TreeMapTag tag) {
            continue;
          }
          var weight = tag.Size;
          minWeight = Math.Min(minWeight, weight);
          maxWeight = Math.Max(maxWeight, weight);
        }
      }
      if (minWeight == double.MaxValue) {
        minWeight = 0;
      }
      var weightRange = maxWeight - minWeight;
      var goalWeightRange = maximumWeight - minimumWeight;

      // Add some padding to group nodes to avoid children overlapping group labels.
      graph.GetDecorator().Nodes.GroupPaddingProvider.AddConstant(node => graph.IsGroupNode(node),
          new GroupPaddingProvider(new InsetsD(1, 23, 1, 1)));

      var layoutData = layout.CreateLayoutData(graph);
      layoutData.NodeWeights.MapperFunction = node => {
        if (graph.IsGroupNode(node)) {
          return 1;
        }
        // Scale weights to avoid large differences.
        var weight = Math.Max((node.Tag as TreeMapTag)?.Size ?? 0.1, 0.1);
        if (maxWeight / Math.Max(minWeight, 0.1) > maximumWeight / minimumWeight && weightRange > goalWeightRange) {
          var scaledWeight = (weight - minWeight) / weightRange;
          return minimumWeight + scaledWeight * goalWeightRange;
        }
        return weight;
      };
      var ascending = sortingOrderBox.SelectedIndex == 1;
      var useNameAsCriterion = sortingCriterionBox.SelectedIndex == 1;
      layoutData.ChildNodeComparison = (node1, node2) => CompareNodes(node1, node2, useNameAsCriterion, ascending);

      // Hide labels during layout.
      var nodeLabelGroup = graphControl.GraphModelManager.NodeLabelGroup;
      nodeLabelGroup.Visible = false;

      await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(700), layoutData);

      // Update text sizes and show labels again.
      UpdateLabelTextSizes(graph);
      nodeLabelGroup.Visible = true;

      // Update the viewport limiter.
      graphControl.ViewportLimiter.Bounds = graphControl.Viewport;
    }

    /**
     * Determines the preferred layout size for the current graph.
     */
    private SizeD GetPreferredSize(IGraph graph) {
      const double defaultMapSize = 1000;
      var root = graph.Nodes.FirstOrDefault(node => graph.GetParent(node) is null);
      var groupTag = root?.Tag is TreeMapTag rootTag ? rootTag.GroupTag : null;
      var masterRoot = masterGraph.Nodes.FirstOrDefault(node => node.Tag is TreeMapTag tag && tag.GroupTag == groupTag);
      var preferredSize = masterRoot is not null && preferredSizes.TryGetValue(masterRoot, out var size)
          ? size
          : (SizeD?) null;

      if (zoomingModeBox.SelectedIndex == 1 && preferredSize is { } value) {
        // If a preferred size is specified, configure the layout to use its aspect ratio. The exact size is not
        // retained; the larger side always has size DEFAULT_MAP_SIZE.
        if (value.Height > value.Width) {
          var ratio = value.Width / value.Height;
          return new SizeD(defaultMapSize * ratio, defaultMapSize);
        }
        var otherRatio = value.Height / value.Width;
        return new SizeD(defaultMapSize, defaultMapSize * otherRatio);
      }
      return new SizeD(defaultMapSize, defaultMapSize);
    }

    /**
     * Updates the sizes of the label texts according to the sizes of their owner nodes.
     */
    private void UpdateLabelTextSizes(IGraph graph) {
      foreach (var node in graph.Nodes) {
        // Only adjust text sizes for normal nodes and folders.
        if (graph.IsGroupNode(node) || node.Labels.Count == 0) {
          continue;
        }

        var label = node.Labels[0];
        const double minSizeForLabel = 50;
        var style = (LabelStyle) graph.NodeDefaults.Labels.Style.Clone();
        style.TextWrapping = TextWrapping.Wrap;
        style.TextTrimming = TextTrimming.CharacterEllipsis;
        var layout = node.Layout;
        if (layout.Height < minSizeForLabel || layout.Width < minSizeForLabel) {
          // The node is rather small, do not display the label at all.
          graph.SetStyle(label, VoidLabelStyle.Instance);
        } else {
          graph.SetStyle(label, style);
          var labelInset = 10;
          // Perform a kind of binary search to find a good font size for the label text.
          var lowSize = 6d;
          var highSize = layout.Height / 2 - labelInset;
          while (highSize - lowSize > 1) {
            var midSize = Math.Round((highSize + lowSize) / 2);
            var text = new FormattedText(label.Text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                style.Typeface, midSize, Brushes.White, VisualTreeHelper.GetDpi(graphControl).PixelsPerDip);
            if (text.WidthIncludingTrailingWhitespace < layout.Width - labelInset &&
                text.Height < layout.Height - labelInset) {
              lowSize = midSize;
            } else {
              highSize = midSize;
            }
          }

          if (lowSize > 8) {
            // Update the label style with a re-sized font.
            style.TextSize = lowSize;
          } else {
            // Wrap the text if the font size would be too small to read.
            style.TextWrapping = TextWrapping.Wrap;
          }
        }
      }
    }

    /**
     * Wires up the toolbar and interaction panel elements.
     */
    private void InitializeUI() {
      navigation.ItemInvoked += OnNavigationItemInvoked;
      aspectRatioSlider.ValueChanged += (_, _) =>
          aspectRatioValue.Text = aspectRatioSlider.Value.ToString("0", CultureInfo.CurrentCulture);
      spacingSlider.ValueChanged += (_, _) =>
          spacingValue.Text = spacingSlider.Value.ToString("0", CultureInfo.CurrentCulture);
      minimumWidthSlider.ValueChanged += (_, _) =>
          minimumWidthValue.Text = minimumWidthSlider.Value.ToString("0", CultureInfo.CurrentCulture);
      minimumHeightSlider.ValueChanged += (_, _) =>
          minimumHeightValue.Text = minimumHeightSlider.Value.ToString("0", CultureInfo.CurrentCulture);
      applyLayoutButton.Click += async (_, _) => await ApplyLayout();

      // Update the labels of the sliders that show their current value.
      aspectRatioValue.Text = aspectRatioSlider.Value.ToString("0", CultureInfo.CurrentCulture);
      spacingValue.Text = spacingSlider.Value.ToString("0", CultureInfo.CurrentCulture);
      minimumWidthValue.Text = minimumWidthSlider.Value.ToString("0", CultureInfo.CurrentCulture);
      minimumHeightValue.Text = minimumHeightSlider.Value.ToString("0", CultureInfo.CurrentCulture);
    }

    /**
     * A flexible compare function which can be used for sorting groups and leaf nodes using different criteria.
     */
    private static int CompareNodes(INode node1, INode node2, bool useNameAsCriterion, bool ascending) {
      var result = 0;
      if (useNameAsCriterion && node1.Labels.Count > 0 && node2.Labels.Count > 0) {
        // Compare by name.
        result = string.Compare(node1.Labels[0].Text, node2.Labels[0].Text, StringComparison.CurrentCulture);
      } else {
        // Compare by size.
        var size1 = (node1.Tag as TreeMapTag)?.Size ?? 0;
        var size2 = (node2.Tag as TreeMapTag)?.Size ?? 0;
        result = size1.CompareTo(size2);
      }

      return ascending ? result : -result;
    }

    private sealed class TreeMapTag
    {
      public string? Id { get; set; }

      public string? GroupTag { get; set; }

      public string? Label { get; set; }

      public string? ParentGroupRef { get; set; }

      public string? GroupRef { get; set; }

      public long Size { get; set; }

      public string Color { get; } = "rgb(0, 113, 112)";
    }

    /**
     * A simple node style which draws a rectangle. The color for the rectangle is provided in the tag.
     */
    private sealed class ColorNodeStyle : NodeStyleBase<DrawingVisual>
    {
      protected override DrawingVisual CreateVisual(IRenderContext context, INode node) {
        var visual = new DrawingVisual();
        Render(visual, node);
        return visual;
      }

      protected override DrawingVisual UpdateVisual(IRenderContext context, DrawingVisual oldVisual, INode node) {
        Render(oldVisual, node);
        return oldVisual;
      }

      private static void Render(DrawingVisual visual, INode node) {
        var layout = node.Layout;
        var color = ParseColor((node.Tag as TreeMapTag)?.Color ?? "rgb(0, 113, 112)");
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        using var drawing = visual.RenderOpen();
        drawing.DrawRectangle(brush, null, new Rect(layout.X, layout.Y, layout.Width, layout.Height));
      }

      private static Color ParseColor(string value) {
        if (value.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && value.EndsWith(')')) {
          var parts = value[4..^1].Split(',');
          return Color.FromRgb(byte.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
              byte.Parse(parts[1].Trim(), CultureInfo.InvariantCulture),
              byte.Parse(parts[2].Trim(), CultureInfo.InvariantCulture));
        }
        return (Color) ColorConverter.ConvertFromString(value)!;
      }
    }

    [STAThread]
    private static void Main() => Run<TreeMapDemo>();
  }
}
