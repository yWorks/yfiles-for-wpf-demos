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
using System.Text.Json;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.DataBinding;
using yWorks.Graph.Styles;

namespace yWorks.Demo.CircleSnapping
{
  [Demo("circle-snapping", "input", "Circular Snapping",
      Summary = "Shows how to enable circular and angle snapping (guidelines) for interactive changes.",
      Keywords = ["move", "resize", "resizing", "guides"], Tags = ["interaction", "snapping", "circle"])]
  public partial class CircleSnappingDemo
  {
    public CircleSnappingDemo() {
      InitializeComponent();
    }

    protected override void Initialize() {
      var editorInputMode = new GraphEditorInputMode {
          AllowCreateNode = false,
          AllowCreateEdge = false,
          ShowHandleItems = GraphItemTypes.None
      };
      graphControl.InputMode = editorInputMode;

      // configures default styles for newly created graph elements
      InitializeGraphDefaults();

      // enable circular snapping for nodes
      InitializeCircularSnapping(editorInputMode);

      // build a sample graph from the given data set
      BuildGraph();

      // enable undo after the initial graph was populated since we don't want to allow undoing that
      graphControl.Graph.SetUndoEngineEnabled(true);
      graphControl.FitGraphBounds();
    }

    private void BuildGraph() {
      var data = LoadGraphData();
      var builder = new GraphBuilder(graphControl.Graph);
      var nodes = builder.CreateNodesSource(data.NodeList, item => item.Id);
      nodes.NodeCreator.LayoutProvider = item => new RectD(
          item.Layout.X, item.Layout.Y, item.Layout.Width, item.Layout.Height);
      builder.CreateEdgesSource(data.EdgeList, item => item.Source, item => item.Target);
      builder.BuildGraph();
    }

    /// <summary>
    ///   Configures snapping in a way that is suited to circular/radial arrangements of child nodes around parent nodes.
    /// </summary>
    private void InitializeCircularSnapping(GraphEditorInputMode editorInputMode) {
      editorInputMode.SnapContext = new GraphSnapContext {
          SnapDistance = 20,
          SnappableItems = SnappableItems.Node,
          ConsiderNodeLabelAllNodeLabelsDistanceSnapping = false,
          CollectNodePairCenterSnapLines = false,
          CollectNodePairSegmentSnapLines = false,
          ConsiderNodeLabelOwnerDistanceSnapping = false,
          ConsiderNodeLabelOwnerLabelsDistanceSnapping = false,
          CollectNodePairSnapLines = false,
          ConsiderEdgeLabelOwnerDistanceSnapping = false,
          ConsiderEdgeLabelOwnerLabelsDistanceSnapping = false,
          CollectNodeSizes = false,
          CollectEdgeSnapReferences = false,
          CollectPortSnapReferences = false,
          ConsiderEdgeLabelOwnerPathSnapping = false,
          ConsiderEdgeLabelAllEdgeLabelsDistanceSnapping = false,
          ConsiderInitialLabelLocationSnapping = false,
          ConsiderPortLabelOwnerLocationSnapping = false
      };

      var nodes = graphControl.Graph.GetDecorator().Nodes;
      nodes.SnapReferenceProvider.AddFactory(node => new SnapCircleProvider(node));
      nodes.SnapResultProvider.AddConstant(new CircleSnapResultProvider());
    }

    private void InitializeGraphDefaults() {
      DemoStyles.InitDemoStyles(graphControl.Graph);
      graphControl.Graph.NodeDefaults.Style = DemoStyles.CreateDemoShapeNodeStyle(ShapeNodeShape.Ellipse);
    }

    private GraphData LoadGraphData() {
      using var stream = LoadAssemblyResource("circle-snapping.json");
      return JsonSerializer.Deserialize<GraphData>(stream,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private sealed class GraphData
    {
      public List<NodeData> NodeList { get; set; } = [];

      public List<EdgeData> EdgeList { get; set; } = [];
    }

    private sealed class NodeData
    {
      public int Id { get; set; }

      public NodeLayout Layout { get; } = new();
    }

    private sealed class NodeLayout
    {
      public double X { get; set; }

      public double Y { get; set; }

      public double Width { get; set; }

      public double Height { get; set; }
    }

    private sealed class EdgeData
    {
      public int Source { get; set; }

      public int Target { get; set; }
    }

    [STAThread]
    public static void Main() {
      Run<CircleSnappingDemo>();
    }
  }
}
