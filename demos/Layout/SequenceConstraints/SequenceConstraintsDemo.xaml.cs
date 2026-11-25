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
using System.Threading.Tasks;
using System.Windows;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Hierarchical;

namespace yWorks.Demo.SequenceConstraints
{
  /// <summary>
  ///   Demonstrates how to use sequence constraints to restrict the node sequencing in a <see cref="HierarchicalLayout"/>.
  /// </summary>
  [Demo("sequence-constraints", "layout", "Sequence Constraints",
      Summary = "Shows how to use sequence constraints to prescribe the node sequencing in hierarchical layouts.",
      Tags = ["layout", "hierarchical"], Keywords = ["layoutfeatures"])]
  public sealed partial class SequenceConstraintsDemo
  {
    public SequenceConstraintsDemo() {
      InitializeComponent();
    }

    protected override async void Initialize() {
      InitializeInputMode();
      InitializeGraph(graphControl.Graph);

      CreateGraph(graphControl.Graph);

      await RunLayout();
    }

    /// <summary>
    ///   Runs a hierarchical layout with sequence constraints derived from the node data.
    /// </summary>
    private async Task RunLayout() {
      // create a new layout algorithm
      var hierarchicalLayout = new HierarchicalLayout();

      // and layout data for it
      var hierarchicalLayoutData = hierarchicalLayout.CreateLayoutData(graphControl.Graph);

      // this is the factory that we apply the constraints to
      var sequenceConstraints = hierarchicalLayoutData.SequenceConstraints;

      // assign constraints for the nodes in the graph
      foreach (var node in graphControl.Graph.Nodes) {
        if (node.Tag is SequenceConstraintsData data && data.Constraints) {
          if (data.Value == 0) {
            sequenceConstraints.PlaceAtHead(node);
          } else if (data.Value == 7) {
            sequenceConstraints.PlaceAtTail(node);
          } else {
            sequenceConstraints.ItemComparables.Mapper[node] = data.Value;
          }
        }
      }

      // perform the layout operation
      SetUIDisabled(true);
      try {
        await graphControl.ApplyLayoutAnimated(hierarchicalLayout, TimeSpan.FromSeconds(1), hierarchicalLayoutData);
      } finally {
        SetUIDisabled(false);
      }
    }

    /// <summary>
    ///   Disables the toolbar buttons while a layout is running.
    /// </summary>
    /// <param name="disabled">True to disable the UI, false to enable it.</param>
    private void SetUIDisabled(bool disabled) {
      newGraphButton.IsEnabled = !disabled;
      enableAllConstraintsButton.IsEnabled = !disabled;
      disableAllConstraintsButton.IsEnabled = !disabled;
      layoutButton.IsEnabled = !disabled;
    }

    /// <summary>
    ///   Initializes the input mode for interaction.
    /// </summary>
    private void InitializeInputMode() {
      var inputMode = new GraphEditorInputMode {
          NodeCreator = CreateNodeCallback,
          LabelEditableItems = GraphItemTypes.None,
          ShowHandleItems = GraphItemTypes.All & ~GraphItemTypes.Node
      };

      // listener for the buttons on the nodes
      inputMode.ItemClicked += (_, args) => {
        if (args.Item is not INode node || node.Tag is not SequenceConstraintsData data) {
          return;
        }

        var location = args.Location;
        var layout = node.Layout;
        var changed = false;
        if (data.Constraints) {
          if (location.Y > layout.Y + layout.Height * 0.5) {
            if (location.X < layout.X + layout.Width * 0.3) {
              node.Tag = data with { Value = Math.Max(0, data.Value - 1) };
              changed = true;
            } else if (location.X > layout.X + layout.Width * 0.7) {
              node.Tag = data with { Value = Math.Min(7, data.Value + 1) };
              changed = true;
            } else {
              node.Tag = data with { Constraints = !data.Constraints };
              changed = true;
            }
          }
        } else {
          node.Tag = data with { Constraints = !data.Constraints };
          changed = true;
        }

        if (changed) {
          graphControl.UpdateVisual();
        }
      };

      graphControl.InputMode = inputMode;
    }

    /// <summary>
    ///   Initializes the graph instance setting default styles and creates a small sample graph.
    /// </summary>
    private static void InitializeGraph(IGraph graph) {
      // minimum size for nodes
      var size = new SizeD(60, 50);

      // set the style as the default for all new nodes
      graph.NodeDefaults.Style = new ConstraintNodeStyle();
      graph.NodeDefaults.Size = size;
    }

    /// <summary>
    ///   Clears the existing graph and creates a new random graph.
    /// </summary>
    private static void CreateGraph(IGraph graph) {
      // remove all nodes and edges from the graph
      graph.Clear();

      // create a new random graph
      const int nodeCount = 20;
      const int edgeCount = 25;
      var nodes = new INode[nodeCount];
      for (var i = 0; i < nodes.Length; i++) {
        var location = new PointD(Random.Shared.NextDouble() * 600, Random.Shared.NextDouble() * 400);
        nodes[i] = CreateNodeCallback(null!, graph, location, null);
      }

      var edges = new HashSet<(int Source, int Target)>();
      while (edges.Count < edgeCount) {
        var source = Random.Shared.Next(nodeCount);
        var target = Random.Shared.Next(nodeCount);
        if (source != target && edges.Add((source, target))) {
          graph.CreateEdge(nodes[source], nodes[target]);
        }
      }
    }

    private async void OnNewGraphClick(object sender, RoutedEventArgs args) {
      CreateGraph(graphControl.Graph);
      await RunLayout();
    }

    private void OnEnableAllConstraintsClick(object sender, RoutedEventArgs args) => SetConstraintsEnabled(true);

    private void OnDisableAllConstraintsClick(object sender, RoutedEventArgs args) => SetConstraintsEnabled(false);

    private async void OnLayoutClick(object sender, RoutedEventArgs args) => await RunLayout();

    /// <summary>
    ///   Callback that actually creates the node and its business object.
    /// </summary>
    private static INode CreateNodeCallback(IInputModeContext context, IGraph graph, PointD location, INode? parent) {
      var bounds = RectD.FromCenter(location, graph.NodeDefaults.Size);
      return graph.CreateNode(parent, bounds,
          tag: new SequenceConstraintsData(Random.Shared.Next(8), Random.Shared.NextDouble() < 0.9));
    }

    /// <summary>
    ///   Enables or disables all constraints for the graph's nodes.
    /// </summary>
    private void SetConstraintsEnabled(bool enabled) {
      var graph = graphControl.Graph;
      foreach (var node in graph.Nodes) {
        if (node.Tag is SequenceConstraintsData data) {
          node.Tag = data with { Constraints = enabled };
        }
      }
      graphControl.UpdateVisual();
    }

    [STAThread]
    private static void Main() => Run<SequenceConstraintsDemo>();
  }

  internal sealed record SequenceConstraintsData(int Value, bool Constraints);
}
