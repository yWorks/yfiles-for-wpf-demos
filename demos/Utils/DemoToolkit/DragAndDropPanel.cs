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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo
{
  /// <summary>
  ///   An item container styled for a drag-and-drop palette.
  /// </summary>
  public sealed class DragAndDropListBoxItem : ListBoxItem
  {
    static DragAndDropListBoxItem() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(DragAndDropListBoxItem),
          new FrameworkPropertyMetadata(typeof(DragAndDropListBoxItem)));
    }
  }

  /// <summary>
  ///   A list box whose default item containers are specific to a drag-and-drop palette.
  /// </summary>
  public sealed class DragAndDropListBox : ListBox
  {
    static DragAndDropListBox() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(DragAndDropListBox),
          new FrameworkPropertyMetadata(typeof(DragAndDropListBox)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new DragAndDropListBoxItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is DragAndDropListBoxItem;
  }

  /// <summary>
  ///   A palette that renders graph items and starts drag operations for them.
  /// </summary>
  /// <remarks>
  ///   Nodes, edges, labels, and ports can be added with
  ///   <see cref="PopulatePanel(System.Collections.Generic.IEnumerable{yWorks.Graph.IModelItem})"/>.
  ///   A node tagged with an <see cref="IStripe"/> is dragged as that stripe, which supports table row and column palettes.
  /// </remarks>
  public sealed class DragAndDropPanel
  {
    private readonly DragAndDropListBox listBox = new();

    /// <summary>
    ///   Gets or sets the maximum width of a palette item's visualization.
    /// </summary>
    public double MaxItemWidth { get; set; } = 150;

    /// <summary>
    ///   Gets or sets the optional maximum height of a palette item's visualization.
    /// </summary>
    /// <remarks>
    ///   The default value, <see cref="double.PositiveInfinity"/>, leaves the visualization height unrestricted.
    /// </remarks>
    public double MaxItemHeight { get; set; } = double.PositiveInfinity;

    /// <summary>
    ///   Gets or sets whether labels of a dragged node are included in its preview model.
    /// </summary>
    public bool CopyNodeLabels { get; set; } = true;

    /// <summary>
    ///   Gets the graph item represented by the currently selected palette entry.
    /// </summary>
    public IModelItem? SelectedModelItem =>
        listBox.SelectedItem is DragAndDropListBoxItem { Tag: IModelItem item } ? item : null;

    /// <summary>
    ///   Creates the palette and places it into the given content control.
    /// </summary>
    /// <param name="container">The control that will host this palette, for example, a <see cref="SideBar"/>.</param>
    public DragAndDropPanel(ContentControl container) {
      listBox.PreviewMouseMove += OnPreviewMouseMove;

      container.Content = listBox;
    }

    /// <summary>
    ///   Replaces the contents of this panel with visualizations of the provided graph items.
    /// </summary>
    public void PopulatePanel(IEnumerable<IModelItem> items) {
      PopulatePanel(items.Select(item => new DragAndDropPanelItem(item)));
    }

    /// <summary>
    ///   Replaces the contents of this panel with visualizations and optional tooltips.
    /// </summary>
    public void PopulatePanel(IEnumerable<DragAndDropPanelItem> items) {
      var graphControl = new GraphControl {
          HorizontalScrollBarPolicy = ScrollBarVisibility.Hidden,
          VerticalScrollBarPolicy = ScrollBarVisibility.Hidden,
          AnimatedViewportChanges = ViewportChanges.None
      };

      listBox.Items.Clear();
      foreach (var item in items) {
        AddItem(graphControl, item.ModelItem, item.Tooltip);
      }
    }

    private void AddItem(GraphControl graphControl, IModelItem item, string? tooltip = null) {
      var visual = CreateVisual(graphControl, item);
      var container = new DragAndDropListBoxItem {
          Content = visual,
          Tag = item
      };
      if (!string.IsNullOrEmpty(tooltip)) {
        container.ToolTip = tooltip;
      }

      listBox.Items.Add(container);
    }

    private UIElement CreateVisual(GraphControl graphControl, IModelItem item) {
      var graph = graphControl.Graph;
      graph.Clear();

      switch (item) {
        case INode node:
          CreateNode(graph, node);
          break;
        case IEdge edge:
          CreateEdge(graph, edge);
          break;
        case ILabel label:
          CreateLabel(graph, label);
          break;
        case IPort port:
          CreatePort(graph, port);
          break;
        default:
          throw new ArgumentException($@"Unsupported drag and drop palette item: {item.GetType().Name}", nameof(item));
      }

      graphControl.FitGraphBounds();
      var bounds = graphControl.ContentBounds;
      if (bounds.Width <= 0 || bounds.Height <= 0) {
        bounds = new RectD(0, 0, 40, 40);
      }

      var exportBounds = bounds.GetEnlarged(new InsetsD(10, 10, 10, 10));
      var scale = Math.Min(1, Math.Min(MaxItemWidth / exportBounds.Width, MaxItemHeight / exportBounds.Height));
      var configurator = new ContextConfigurator(exportBounds) {
          Scale = scale,
          WorldBounds = exportBounds
      };

      return new UIElementExporter(configurator).Export(graphControl);
    }

    private static void CreateNode(IGraph graph, INode originalNode) {
      var node = graph.CreateNode(originalNode.Layout.ToRectD(), originalNode.Style, originalNode.Tag);
      foreach (var label in originalNode.Labels) {
        graph.AddLabel(node, label.Text, label.LayoutParameter, label.Style, label.PreferredSize, label.Tag);
      }

      foreach (var port in originalNode.Ports) {
        graph.AddPort(node, port.LocationParameter, port.Style, port.Tag);
      }
    }

    private static void CreateEdge(IGraph graph, IEdge originalEdge) {
      var source = graph.CreateNode(new RectD(0, 10, 0, 0), VoidNodeStyle.Instance);
      var target = graph.CreateNode(new RectD(50, 40, 0, 0), VoidNodeStyle.Instance);
      var edge = graph.CreateEdge(source, target, originalEdge.Style);
      graph.AddBend(edge, new PointD(25, 10));
      graph.AddBend(edge, new PointD(25, 40));
    }

    private static void CreateLabel(IGraph graph, ILabel originalLabel) {
      var node = graph.CreateNode(new RectD(0, 10, 0, 0), VoidNodeStyle.Instance);

      ILabelOwner labelOwner;
      switch (originalLabel.Owner) {
        case IPort:
          labelOwner = graph.AddPort(node, FreeNodePortLocationModel.Center, VoidPortStyle.Instance);
          break;
        case IEdge:
          labelOwner = graph.CreateEdge(node, node, VoidEdgeStyle.Instance);
          break;
        default:
          labelOwner = node;
          break;
      }

      graph.AddLabel(labelOwner, originalLabel.Text, originalLabel.LayoutParameter, originalLabel.Style,
          originalLabel.PreferredSize, originalLabel.Tag);
    }

    private static void CreatePort(IGraph graph, IPort originalPort) {
      var node = graph.CreateNode(new RectD(10, 10, 50, 30), VoidNodeStyle.Instance);
      graph.AddPort(node, originalPort.LocationParameter, originalPort.Style, originalPort.Tag);
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e) {
      if (e.LeftButton != MouseButtonState.Pressed || e.OriginalSource is not DependencyObject source) {
        return;
      }

      var container = ItemsControl.ContainerFromElement(listBox, source) as DragAndDropListBoxItem;
      if (container?.Tag is not IModelItem paletteItem) {
        return;
      }

      var data = CreateDragData(paletteItem);
      DragDrop.DoDragDrop(container, data, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
      e.Handled = true;
    }

    private DataObject CreateDragData(IModelItem item) {
      if (item is INode { Tag: IStripe stripe }) {
        return new DataObject(typeof(IStripe), stripe);
      }

      if (item is INode node) {
        var draggedNode = new SimpleNode {
            Layout = node.Layout.ToRectD(),
            // clone the node style in case it is a TableNodeStyle
            Style = (INodeStyle) node.Style.Clone(),
            Tag = node.Tag
        };
        if (CopyNodeLabels) {
          draggedNode.Labels = [
              .. node.Labels.Select(label =>
                  (ILabel) new SimpleLabel(draggedNode, label.Text, label.LayoutParameter) {
                      Style = label.Style,
                      Tag = label.Tag,
                      PreferredSize = label.PreferredSize
                  })
          ];
        }

        draggedNode.Ports = [
            .. node.Ports.Select(port =>
                (IPort) new SimplePort(draggedNode, port.LocationParameter) {
                    Style = port.Style,
                    Tag = port.Tag
                })
        ];
        return new DataObject(typeof(INode), draggedNode);
      }

      return item switch {
          IEdge edge => new DataObject(typeof(IEdge), edge),
          ILabel label => new DataObject(typeof(ILabel), label),
          IPort port => new DataObject(typeof(IPort), port),
          _ => throw new ArgumentException($@"Unsupported drag and drop palette item: {item.GetType().Name}",
              nameof(item))
      };
    }
  }

  /// <summary>
  ///   A graph item and optional tooltip to display in a <see cref="DragAndDropPanel"/>.
  /// </summary>
  public sealed record DragAndDropPanelItem(IModelItem ModelItem, string? Tooltip = null);
}
