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
using System.Linq;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.Bridges
{
  /// <summary>
  ///   Interaction logic for BridgesDemo.xaml
  /// </summary>
  [Demo("bridges", "view", "Bridges",
      Summary = "Shows the capabilities of the BridgeManager class for inserting bridges into edge paths.",
      Tags = ["line gaps", "line jumps"], Keywords = ["layoutfeatures", "intersection", "intersecting", "groups"])]
  public sealed partial class BridgesDemo
  {
    /**
     * Holds the graphComponent.
     */
    private GraphControl graphComponent = null!;

    /**
     * Holds the bridgeManager.
     */
    private BridgeManager bridgeManager = null!;

    private BridgeSettings settings = null!;

    /// <summary>
    ///   Runs the demo.
    /// </summary>
    protected override void Initialize() {
      graphComponent = graphControl;
      var graph = graphComponent.Graph;

      // draw edges in front, so that group nodes don't hide the bridges
      graphComponent.GraphModelManager.EdgeGroup.ToFront();
      graphComponent.GraphModelManager.HierarchicalNestingPolicy = HierarchicalNestingPolicy.Nodes;

      DemoStyles.InitDemoStyles(graph);

      graphComponent.InputMode = new GraphEditorInputMode();

      ConfigureBridges();

      InitializeToolBarElements();

      CreateSampleGraph();
    }

    /// <summary>
    ///   Adds and configures the <see cref="BridgeManager"/>.
    /// </summary>
    private void ConfigureBridges() {
      bridgeManager = new BridgeManager();

      // We would like to change the custom bridge rendering default,
      // this can be done by decorating the existing default callback
      bridgeManager.DefaultBridgeCreator = new CustomCallback(bridgeManager.DefaultBridgeCreator);

      // Convenience class that just queries all model item
      var provider = new GraphObstacleProvider();

      // We also want to query nodes for potential obstacles (disabled by default)
      provider.QueryNodes = true;

      // Register an IObstacleProvider, bridgeManager will query all registered obstacle providers
      // to determine if a bridge must be created
      bridgeManager.AddObstacleProvider(provider);
      // Bind the bridge manager to the GraphComponent...
      bridgeManager.CanvasControl = graphComponent;

      // We register a custom obstacle provider in the node's lookup of group nodes
      // that can be used by bridgeManager (through provider...)
      graphComponent.Graph.GetDecorator().Nodes.ObstacleProvider.AddFactory(
          node => graphComponent.Graph.IsGroupNode(node), node => new GroupNodeObstacleProvider(node));
    }

    /// <summary>
    ///   Initializes the combo boxes and the text-boxes of the settings panel.
    /// </summary>
    private void InitializeToolBarElements() {
      settings = new BridgeSettings(bridgeManager);
      settingsPanel.DataContext = settings;
      settings.SettingChanged += (_, _) => {
        bridgeManager.DefaultBridgeCrossingStyle = settings.CrossingStyle;
        bridgeManager.BridgeCrossingPolicy = settings.CrossingPolicy;
        bridgeManager.DefaultBridgeOrientationStyle = settings.OrientationStyle;
        bridgeManager.DefaultBridgeWidth = settings.BridgeWidth;
        bridgeManager.DefaultBridgeHeight = settings.BridgeHeight;
        bridgeManager.MaximumBridgeWidth = settings.MaximumBridgeWidth;
        graphComponent.Invalidate();
      };
    }

    /// <summary>
    ///   Creates the sample graph.
    /// </summary>
    private void CreateSampleGraph() {
      var graph = graphComponent.Graph;

      const int gridSize = 4;
      const double nodeDist = 40;
      for (var i = 1; i <= gridSize; i++) {
        // create a vertical and a horizontal node pair
        for (var dimension = 0; dimension < 2; dimension++) {
          var isVertical = dimension % 2 == 0;
          var location1 = isVertical ? new PointD(nodeDist * i, 0) : new PointD(0, nodeDist * i);
          var location2 = isVertical
              ? new PointD(nodeDist * i, (gridSize + 1) * nodeDist)
              : new PointD((gridSize + 1) * nodeDist, nodeDist * i);
          var n1 = graph.CreateNode(location1);
          var n2 = graph.CreateNode(location2);

          // create edge with alternating direction
          var even = i % 2 == 0;
          var source1 = even ? n1 : n2;
          var target1 = even ? n2 : n1;
          graph.CreateEdge(source1, target1);
        }
      }

      // create a second edge between node 0 and 1
      var nodes = graph.Nodes.ToList();
      var p1 = graph.AddPort(nodes[1], new PointD(0, 0));
      graph.SetRelativePortLocation(p1, new PointD(5, 0));
      var p2 = graph.AddPort(nodes[0], new PointD(0, 0));
      graph.SetRelativePortLocation(p2, new PointD(5, 0));
      graph.CreateEdge(p1, p2);

      // label nodes with their index
      for (var i = 0; i < nodes.Count; i++) {
        graph.AddLabel(nodes[i], i.ToString());
      }

      // create edges with different angles
      var n3 = graph.CreateNode(new PointD(300, 50));
      var n4 = graph.CreateNode(new PointD(750, 50));
      graph.CreateEdge(n3, n4);
      var n5 = graph.CreateNode(new PointD(350, 0));
      for (var i = 0; i < 6; i++) {
        var n = graph.CreateNode(new PointD(350 + i * 70, 100));
        graph.CreateEdge(n5, n);
      }

      var n6 = graph.CreateNode(new PointD(300, 200));
      var n7 = graph.CreateNode(new PointD(400, 200));
      var n8 = graph.CreateNode(new PointD(500, 200));
      graph.CreateEdge(n6, n8);

      // create a group node to show the effect of the custom GroupNodeObstacleProvider
      var groupNode = graph.CreateGroupNode();
      graph.AddLabel(groupNode, "Group Node");
      graph.SetParent(n7, groupNode);
      graph.AdjustGroupNodeLayout(groupNode);
      graph.SetNodeLayout(groupNode, groupNode.Layout.ToRectD().GetEnlarged(new InsetsD(15, 0, 15, 0)));

      graphComponent.FitGraphBounds();
    }

    [STAThread]
    private static void Main(string[] args) {
      Run<BridgesDemo>();
    }

    public BridgesDemo() {
      InitializeComponent();
    }
  }
}
