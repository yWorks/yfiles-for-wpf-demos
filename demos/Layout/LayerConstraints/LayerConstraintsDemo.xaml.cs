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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.LayerConstraints
{
  [Demo("layer-constraints", "layout", "Layer Constraints",
      Summary = "Shows how to use layer constraints to prescribe the node layering in hierarchical layouts.",
      Tags = ["layout", "hierarchical"], Keywords = ["layoutfeatures"])]
  public partial class LayerConstraintsDemo
  {
    private GraphEditorInputMode graphEditorInputMode = null!;

    private GraphControl GraphControl => graphControl;

    private IGraph Graph => GraphControl.Graph;

    protected override async void Initialize() {
      InitializeInputMode();
      InitializeGraph();
      CreateGraph(Graph);
      await RunLayoutAsync();
      await GraphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Runs a hierarchical layout with layer constraints configured from the nodes' data.
    /// </summary>
    private async Task RunLayoutAsync() {
      // create a new layout algorithm
      var hierarchicalLayout = new HierarchicalLayout {
          FromScratchLayeringStrategy = LayeringStrategy.HierarchicalTopmost
      };

      // and layout data for it
      var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(Graph);
      var layerConstraints = hierarchicalLayoutData.LayerConstraints;

      // assign constraints for all nodes in the graph
      foreach (var node in Graph.Nodes) {
        var data = node.Tag as LayerConstraintsData;
        if (data is not { Constraints: true }) {
          continue;
        }

        // Ensure that nodes with value 0 and 7 are always at the top and bottom, respectively
        if (data.Value == 0) {
          layerConstraints.PlaceAtTop(node);
        }
        if (data.Value == 7) {
          layerConstraints.PlaceAtBottom(node);
        } else {
          // All nodes can then be sorted into their respective layers by their value
          layerConstraints.NodeComparables.Mapper[node] = data.Value;
        }
      }

      // perform the layout operation
      SetUiDisabled(true);
      try {
        await GraphControl.ApplyLayoutAnimated(hierarchicalLayout, TimeSpan.FromSeconds(1), hierarchicalLayoutData);
      } finally {
        SetUiDisabled(false);
      }
    }

    /// <summary>
    ///   Initializes the input mode for interaction.
    /// </summary>
    private void InitializeInputMode() {
      var inputMode = new GraphEditorInputMode {
          NodeCreator = CreateNodeCallback,
          LabelEditableItems = GraphItemTypes.Edge | GraphItemTypes.EdgeLabel,
          ShowHandleItems = GraphItemTypes.All ^ GraphItemTypes.Node
      };
      graphEditorInputMode = inputMode;

      inputMode.EditLabelInputMode.ValidateLabelText += (_, args) => {
        var newText = args.NewText.Trim();
        // only allow numbers between 0 and 100
        if (newText.Length == 0 ||
            !double.TryParse(newText, NumberStyles.Float, CultureInfo.CurrentCulture, out var value) || value < 0 ||
            value > 100) {
          args.Cancel = true;
        } else {
          args.NewText = newText;
        }
      };

      // listener for the buttons on the nodes
      inputMode.ItemClicked += (_, args) => {
        if (args.Item is not INode node) {
          return;
        }

        var location = args.Location;
        var layout = node.Layout;
        var data = node.Tag as LayerConstraintsData;
        if (data is null) {
          return;
        }

        if (data.Constraints) {
          if (location.Y > layout.Y + layout.Height * 0.5) {
            if (location.X < layout.X + layout.Width * 0.3) {
              node.Tag = data with { Value = Math.Max(0, data.Value - 1) };
            } else if (location.X > layout.X + layout.Width * 0.7) {
              node.Tag = data with { Value = Math.Min(7, data.Value + 1) };
            } else {
              node.Tag = data with { Constraints = !data.Constraints };
            }
          }
        } else {
          node.Tag = data with { Constraints = !data.Constraints };
        }

        GraphControl.Invalidate();
      };

      GraphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Initializes the graph instance setting default styles and creates a small sample graph.
    /// </summary>
    private void InitializeGraph() {
      // minimum size for nodes
      Graph.NodeDefaults.Size = new(60, 50);

      // set the style as the default for all new nodes
      Graph.NodeDefaults.Style = StyleTemplates.ConstraintNodeStyle;

      // create a simple label style
      var labelStyle = new LabelStyle {
          Typeface = new Typeface("Arial"),
          BackgroundBrush = Brushes.White,
          AutoFlip = true,
          Padding = new InsetsD(5, 3, 5, 3)
      };

      // set the style as the default for all new labels
      Graph.NodeDefaults.Labels.Style = labelStyle;
      Graph.EdgeDefaults.Labels.Style = labelStyle;
      Graph.EdgeDefaults.Labels.LayoutParameter = new EdgeSegmentLabelModel().CreateParameterFromCenter();
    }

    /// <summary>
    ///   Clears the existing graph and creates a new random graph.
    /// </summary>
    private void CreateGraph(IGraph graph) {
      // remove all nodes and edges from the graph
      graph.Clear();

      // create a new random graph
      var nodes = new List<INode>(20);
      for (var i = 0; i < 20; i++) {
        nodes.Add(CreateNodeCallback(null!, graph, PointD.Origin, null));
      }

      var random = Random.Shared;
      var createdEdges = new HashSet<(int Source, int Target)>();
      while (createdEdges.Count < 25) {
        var source = random.Next(nodes.Count);
        var target = random.Next(nodes.Count);
        if (source != target && createdEdges.Add((source, target))) {
          graph.CreateEdge(nodes[source], nodes[target]);
        }
      }
    }

    private async void OnNewGraphClick(object sender, RoutedEventArgs args) {
      CreateGraph(Graph);
      await RunLayoutAsync();
    }

    private void OnEnableAllConstraintsClick(object sender, RoutedEventArgs args) => SetConstraintsEnabled(true);

    private void OnDisableAllConstraintsClick(object sender, RoutedEventArgs args) => SetConstraintsEnabled(false);

    private async void OnLayoutClick(object sender, RoutedEventArgs args) => await RunLayoutAsync();

    /// <summary>
    ///   Callback that actually creates the node and its business object.
    /// </summary>
    private INode CreateNodeCallback(IInputModeContext context, IGraph graph, PointD location, INode? parent) {
      var bounds = RectD.FromCenter(location, graph.NodeDefaults.Size);
      return graph.CreateNode(parent, bounds,
          tag: new LayerConstraintsData(Random.Shared.Next(0, 8), Random.Shared.NextDouble() < 0.9));
    }

    /// <summary>
    ///   Enables or disables all constraints for the graph's nodes.
    /// </summary>
    private void SetConstraintsEnabled(bool enabled) {
      foreach (var node in Graph.Nodes) {
        if (node.Tag is LayerConstraintsData data) {
          node.Tag = data with { Constraints = enabled };
        }
      }
      GraphControl.Invalidate();
    }

    /// <summary>
    ///   Disables the toolbar and the input mode while the layout is running.
    /// </summary>
    private void SetUiDisabled(bool disabled) {
      graphEditorInputMode.Enabled = !disabled;
      newGraphButton.IsEnabled = !disabled;
      enableConstraintsButton.IsEnabled = !disabled;
      disableConstraintsButton.IsEnabled = !disabled;
      layoutButton.IsEnabled = !disabled;
    }

    public LayerConstraintsDemo() {
      InitializeComponent();
    }

    [STAThread]
    private static void Main() => Run<LayerConstraintsDemo>();

    internal sealed record LayerConstraintsData(int Value, bool Constraints);
  }
}
