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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.CustomCopyAndPaste
{
  [Demo("custom-copy-and-paste", "input", "Custom Copy and Paste",
      Summary = "Shows how to copy and paste graph item styles.", Keywords = ["copy", "paste", "styles", "popover"],
      Tags = ["interaction"])]
  public partial class CustomCopyAndPasteDemo
  {
    private readonly DispatcherTimer toastTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private PopoverDescriptor? selectionPopover;

    public CustomCopyAndPasteDemo() {
      InitializeComponent();
      toastTimer.Tick += (_, _) => {
        toast.Visibility = Visibility.Collapsed;
        toastTimer.Stop();
      };
    }

    /// <summary>
    ///   Bootstraps the demo.
    /// </summary>
    protected override async void Initialize() {
      var graph = graphControl.Graph;

      // Enable graph editing.
      var inputMode = new GraphEditorInputMode();
      graphControl.InputMode = inputMode;

      // Load sample graph.
      BuildGraph.Build(graph, LoadGraphData());

      // Apply a hierarchical layout.
      await RunLayout(graph);

      // Make sure the graph is centered in the view.
      await graphControl.FitGraphBounds();

      // Enable undo after the initial graph was populated since we don't want to allow undoing that.
      graph.SetUndoEngineEnabled(true);

      // Register listeners for enabling the paste button, showing the popup, and the keyboard shortcut.
      RegisterListeners(inputMode);
    }

    private GraphData LoadGraphData() {
      using var stream = LoadAssemblyResource("graph-data.json");
      return JsonSerializer.Deserialize<GraphData>(stream) ??
          throw new InvalidDataException("The embedded graph data is invalid.");
    }

    private async Task RunLayout(IGraph graph) {
      var layout = new HierarchicalLayout {
          LayoutOrientation = LayoutOrientation.LeftToRight,
          MinimumLayerDistance = 50,
          NodeDistance = 50
      };
      var data = layout.CreateLayoutData(graph);
      // Place raw materials in a separate layer and refined materials in a separate layer.
      var rawMaterials = graph.Nodes.Where(node => Equals(node.Tag, "raw-material")).ToArray();
      foreach (var node in rawMaterials.Skip(1)) {
        data.LayerConstraints.PlaceInSameLayer(rawMaterials[0], node);
      }
      var refinedMaterials = graph.Nodes.Where(node => Equals(node.Tag, "refined-material")).ToArray();
      foreach (var node in refinedMaterials) {
        data.LayerConstraints.PlaceInSameLayer(refinedMaterials[^1], node);
      }

      var executor = new LayoutExecutor(graphControl, layout) {
          LayoutData = data,
          AnimationDuration = TimeSpan.Zero
      };
      // Use the specified label model parameters for positioning instead of the layout result.
      executor.LabelPlacementPolicies.Constant = LabelPlacementPolicy.PreferParameter;
      await executor.Start();
    }

    private void RegisterListeners(GraphEditorInputMode inputMode) {
      // Enable paste style button on cut and copy.
      graphControl.Clipboard.ItemsCopied += (_, _) => pasteStyleButton.IsEnabled = true;
      graphControl.Clipboard.ItemsCut += (_, _) => pasteStyleButton.IsEnabled = true;

      // Show popup for clipboard operations.
      var manager = inputMode.PopoverManager;
      inputMode.MultiSelectionFinished += async (_, _) => {
        manager.CloseAll();
        if (graphControl.Selection.Count > 0) {
          await ShowPopover(manager);
        }
      };
      inputMode.DeletedItem += (_, _) => manager.CloseAll();
      graphControl.Selection.ItemRemoved += (_, _) => {
        if (graphControl.Selection.Count == 0) {
          manager.CloseAll();
        }
      };
    }

    private bool PasteStyle => pasteStyleOption.IsChecked == true;

    private bool PasteNodeSize => pasteNodeSizeOption.IsChecked == true;

    private bool PasteLabelPositioning => pasteLabelPositionOption.IsChecked == true;

    /// <summary>
    ///   Shows a popover with a toolbar for clipboard operations at the bottom-right of the selection.
    /// </summary>
    private async Task ShowPopover(PopoverManager popoverManager) {
      var toolbar = new StackPanel { Orientation = Orientation.Horizontal };
      var copy = new Button {
          Content = "Copy",
          Padding = new Thickness(8, 5, 8, 5),
          Margin = new Thickness(2)
      };
      // Add a click listener that executes the copy command.
      copy.Click += (_, _) => {
        if (graphControl.InputMode is GraphEditorInputMode mode) {
          mode.Copy();
        }
        selectionPopover?.Close();
      };
      var pasteStyle = new Button {
          Content = "Paste Style",
          IsEnabled = !graphControl.Clipboard.IsEmpty,
          Padding = new Thickness(8, 5, 8, 5),
          Margin = new Thickness(2)
      };
      pasteStyle.Click += (_, _) => {
        ApplyCopiedStyle();
        selectionPopover?.Close();
      };
      toolbar.Children.Add(copy);
      toolbar.Children.Add(pasteStyle);

      var popoverDescriptor = new PopoverDescriptor {
          Content = new Border {
              Background = Brushes.White,
              BorderBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0xC8, 0xC8)),
              BorderThickness = new Thickness(1),
              Padding = new Thickness(3),
              Child = toolbar
          },
          // Small offset so the popover does not lie behind the cursor and show a tooltip instantly.
          Offset = new PointD(10, 10),
          Anchor = GetBounds(graphControl).GetBottomRight(),
          Behavior = PopoverBehavior.Auto
      };
      selectionPopover = popoverDescriptor;
      RegisterPopoverUpdateLocation(popoverDescriptor);
      await popoverManager.Open(popoverDescriptor);
    }

    /// <summary>
    ///   Registers a listener to lazily update the popover location when the selected bounds change.
    /// </summary>
    private void RegisterPopoverUpdateLocation(PopoverDescriptor popoverDescriptor) {
      var dirtyLocation = false;

      // Marks the popover location in need of update.
      NodeLayoutChangedHandler invalidateLocation = (_, _, _) => {
        if (popoverDescriptor.IsOpen) {
          dirtyLocation = true;
        }
      };

      // Actually updates the popover location.
      RoutedEventHandler updatePopoverLocation = (_, _) => {
        if (dirtyLocation) {
          popoverDescriptor.Anchor = GetBounds(graphControl).GetBottomRight();
          dirtyLocation = false;
        }
      };

      // Listen for changes and update the popover location after the visual tree has been updated.
      graphControl.Graph.NodeLayoutChanged += invalidateLocation;
      graphControl.UpdatedVisual += updatePopoverLocation;
      popoverDescriptor.Closed += (_, _) => {
        graphControl.Graph.NodeLayoutChanged -= invalidateLocation;
        graphControl.UpdatedVisual -= updatePopoverLocation;
        if (ReferenceEquals(selectionPopover, popoverDescriptor)) {
          selectionPopover = null;
        }
      };
    }

    /// <summary>
    ///   Calculates the bounding rectangle of the selected items.
    /// </summary>
    private static RectD GetBounds(GraphControl graphControl) {
      var selection = graphControl.Selection;

      var bounds = selection.Nodes.Aggregate(RectD.Empty,
          (current, node) => current + node.Style.Renderer.GetBoundsProvider(node, node.Style)
              .GetBounds(graphControl.CanvasContext));
      bounds = selection.Edges.Aggregate(bounds,
          (current, edge) => current + edge.Style.Renderer.GetBoundsProvider(edge, edge.Style)
              .GetBounds(graphControl.CanvasContext));
      bounds = selection.Labels.Aggregate(bounds,
          (current, label) => current + label.Style.Renderer.GetBoundsProvider(label, label.Style)
              .GetBounds(graphControl.CanvasContext));
      return bounds;
    }

    private void ApplyCopiedStyle() =>
        ApplyStyle.Apply(graphControl, PasteStyle, PasteNodeSize, PasteLabelPositioning,
            message => ApplyStyle.ShowToast(toast, toastText, toastTimer, message));

    // Paste style on Ctrl + Alt + V.
    private void OnGraphPreviewKeyDown(object sender, KeyEventArgs e) {
      if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt) && e.Key == Key.V &&
          graphControl.Selection.Count > 0 && !graphControl.Clipboard.IsEmpty) {
        ApplyCopiedStyle();
        e.Handled = true;
      }
    }

    private void OnPasteStyleClick(object sender, RoutedEventArgs e) => ApplyCopiedStyle();

    [STAThread]
    public static void Main() => Run<CustomCopyAndPasteDemo>();
  }
}
